using System.Net;
using System.Threading.Tasks;
using System.Web;
using Translumo.Infrastructure.Constants;
using Translumo.Translation.Exceptions;
using Translumo.Utils.Http;

namespace Translumo.Translation.Google
{
    /// <summary>
    /// Lightweight Google translator used by the instant image-translation feature. It always sends
    /// <c>sl=auto</c> so Google detects the source language server-side, and lets the target language
    /// be chosen per request. It reuses <see cref="GoogleContainer"/> (proxy-aware reader) and
    /// <see cref="RegexStorage.GoogleTranslateResultRegex"/> without touching the config-bound
    /// <see cref="GoogleTranslator"/> used by continuous translation.
    /// </summary>
    public sealed class AutoSourceGoogleTranslator
    {
        private const string TRANSLATE_URL = "https://translate.google.com/m?hl={0}&sl=auto&tl={0}&ie=UTF-8&prev=_m&q={1}";

        private readonly GoogleContainer _container = new GoogleContainer(isPrimary: true);

        public async Task<string> TranslateAsync(string sourceText, string targetIsoCode)
        {
            if (string.IsNullOrWhiteSpace(sourceText))
            {
                return sourceText;
            }

            var url = string.Format(TRANSLATE_URL, targetIsoCode, HttpUtility.UrlEncode(sourceText));
            var response = await _container.Reader.RequestWebDataAsync(url, HttpMethods.GET, true).ConfigureAwait(false);
            if (response.IsSuccessful)
            {
                var match = RegexStorage.GoogleTranslateResultRegex.Match(response.Body);
                if (match.Success)
                {
                    return WebUtility.HtmlDecode(match.Value);
                }
            }

            throw new TranslationException($"Unexpected web response: '{response.Body}'");
        }
    }
}
