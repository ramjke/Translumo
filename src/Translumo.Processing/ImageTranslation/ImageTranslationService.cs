using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Language;
using Translumo.OCR.WindowsOCR;
using Translumo.Translation.Google;

namespace Translumo.Processing.ImageTranslation
{
    /// <summary>
    /// Orchestrates the instant image-translation ("Google Lens") flow: positional OCR of a captured
    /// region (with source-language auto-detect) + per-line translation with source auto-detect.
    /// The WPF layer captures the region bytes and renders the returned lines over their boxes.
    /// </summary>
    public sealed class ImageTranslationService
    {
        private const int MAX_CONCURRENT_TRANSLATIONS = 3;

        private readonly LanguageService _languageService;
        private readonly ILogger _logger;
        private readonly AutoSourceGoogleTranslator _translator = new AutoSourceGoogleTranslator();

        public ImageTranslationService(LanguageService languageService, ILogger<ImageTranslationService> logger)
        {
            _languageService = languageService;
            _logger = logger;
        }

        /// <summary>Installed Windows OCR source languages as (BCP-47 tag, display name), for the override dropdown.</summary>
        public IReadOnlyList<(string Tag, string DisplayName)> GetAvailableSourceLanguages()
        {
            return WindowsOcrPositional.GetInstalledRecognizers();
        }

        /// <param name="regionImage">Encoded screenshot bytes of the selected region.</param>
        /// <param name="forcedSourceTag">BCP-47 recognizer tag to force, or null to auto-detect.</param>
        /// <param name="target">Target translation language.</param>
        public async Task<ImageTranslationResult> TranslateRegionAsync(byte[] regionImage, string forcedSourceTag, Languages target)
        {
            var ocr = await WindowsOcrPositional.DetectAndRecognizeAsync(regionImage, forcedSourceTag).ConfigureAwait(false);
            if (ocr == null || ocr.Lines.Count == 0)
            {
                return new ImageTranslationResult
                {
                    DetectedLanguageTag = ocr?.LanguageTag,
                    ImageWidth = ocr?.ImageWidth ?? 0,
                    ImageHeight = ocr?.ImageHeight ?? 0
                };
            }

            var targetIso = _languageService.GetLanguageDescriptor(target).IsoCode;
            var translations = await TranslateLinesAsync(ocr.Lines.Select(l => l.Text), targetIso).ConfigureAwait(false);

            var lines = ocr.Lines
                .Select(l => new TranslatedLine
                {
                    Source = l.Text,
                    Translation = translations.TryGetValue(l.Text, out var tr) ? tr : l.Text,
                    Box = l.Box
                })
                .ToList();

            return new ImageTranslationResult
            {
                Lines = lines,
                DetectedLanguageTag = ocr.LanguageTag,
                ImageWidth = ocr.ImageWidth,
                ImageHeight = ocr.ImageHeight
            };
        }

        private async Task<IDictionary<string, string>> TranslateLinesAsync(IEnumerable<string> sources, string targetIso)
        {
            var distinct = sources.Distinct().ToList();
            var map = new ConcurrentDictionary<string, string>();
            using var throttle = new SemaphoreSlim(MAX_CONCURRENT_TRANSLATIONS);

            await Task.WhenAll(distinct.Select(async src =>
            {
                await throttle.WaitAsync().ConfigureAwait(false);
                try
                {
                    map[src] = await _translator.TranslateAsync(src, targetIso).ConfigureAwait(false);
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning(ex, "Image line translation failed; keeping source text");
                    map[src] = src;
                }
                finally
                {
                    throttle.Release();
                }
            })).ConfigureAwait(false);

            return map;
        }
    }
}
