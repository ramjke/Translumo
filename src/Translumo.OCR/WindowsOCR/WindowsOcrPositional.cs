using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace Translumo.OCR.WindowsOCR
{
    /// <summary>
    /// Positional Windows OCR used by the instant image-translation feature.
    /// Unlike <see cref="WindowsOCREngine"/> (plain text), this keeps per-line bounding boxes and
    /// can auto-detect the source language by trying the installed recognizer packs and scoring the
    /// result by how many characters match each language's writing system (the UWP OCR API does not
    /// expose per-word confidence, so a script-match heuristic is used instead).
    /// </summary>
    public static class WindowsOcrPositional
    {
        /// <summary>Installed Windows OCR recognizers as (BCP-47 tag, human-readable name).</summary>
        public static IReadOnlyList<(string Tag, string DisplayName)> GetInstalledRecognizers()
        {
            return OcrEngine.AvailableRecognizerLanguages
                .Select(l => (l.LanguageTag, l.DisplayName))
                .ToArray();
        }

        /// <summary>
        /// Recognizes <paramref name="image"/> (encoded bytes). When <paramref name="forcedLanguageTag"/>
        /// is null the source language is auto-detected across installed recognizer packs.
        /// Returns null when no OCR recognizer is available at all.
        /// </summary>
        public static async Task<OcrRegionResult> DetectAndRecognizeAsync(byte[] image, string forcedLanguageTag = null)
        {
            var (softwareBitmap, width, height) = await DecodeAsync(image).ConfigureAwait(false);

            IReadOnlyList<string> candidates = forcedLanguageTag != null
                ? new[] { forcedLanguageTag }
                : GetCandidateLanguageTags();

            if (candidates.Count == 0)
            {
                return null;
            }

            OcrRegionResult best = null;
            var bestScore = -1;
            foreach (var tag in candidates)
            {
                var engine = OcrEngine.TryCreateFromLanguage(new Language(tag));
                if (engine == null)
                {
                    continue;
                }

                OcrResult ocr = await engine.RecognizeAsync(softwareBitmap).AsTask().ConfigureAwait(false);
                var lines = ExtractLines(ocr);
                var score = ScoreForLanguage(ocr.Text, tag);

                if (best == null || score > bestScore)
                {
                    bestScore = score;
                    best = new OcrRegionResult
                    {
                        LanguageTag = tag,
                        Lines = lines,
                        ImageWidth = width,
                        ImageHeight = height
                    };
                }
            }

            return best;
        }

        private static async Task<(SoftwareBitmap bitmap, int width, int height)> DecodeAsync(byte[] image)
        {
            using var memory = new MemoryStream(image);
            var decoder = await BitmapDecoder.CreateAsync(memory.AsRandomAccessStream()).AsTask().ConfigureAwait(false);
            var bitmap = await decoder.GetSoftwareBitmapAsync().AsTask().ConfigureAwait(false);

            return (bitmap, (int)decoder.PixelWidth, (int)decoder.PixelHeight);
        }

        private static List<PositionalOcrLine> ExtractLines(OcrResult ocr)
        {
            var result = new List<PositionalOcrLine>();
            foreach (var line in ocr.Lines)
            {
                if (line.Words.Count == 0)
                {
                    continue;
                }

                double left = double.MaxValue, top = double.MaxValue, right = 0, bottom = 0;
                foreach (var word in line.Words)
                {
                    var r = word.BoundingRect;
                    left = Math.Min(left, r.X);
                    top = Math.Min(top, r.Y);
                    right = Math.Max(right, r.X + r.Width);
                    bottom = Math.Max(bottom, r.Y + r.Height);
                }

                var text = line.Text?.Trim();
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                result.Add(new PositionalOcrLine
                {
                    Text = text,
                    Box = new System.Drawing.RectangleF(
                        (float)left, (float)top, (float)(right - left), (float)(bottom - top))
                });
            }

            return result;
        }

        /// <summary>
        /// One representative installed recognizer per writing system. CJK scripts (zh/ja/ko) are kept
        /// separate because they need different packs; the many Latin/Cyrillic packs collapse to one
        /// representative each to keep the number of OCR passes small.
        /// </summary>
        private static IReadOnlyList<string> GetCandidateLanguageTags()
        {
            var byFamily = new Dictionary<string, string>();
            foreach (var lang in OcrEngine.AvailableRecognizerLanguages)
            {
                var tag = lang.LanguageTag;
                var family = ScriptFamily(tag);
                // Keep the canonical representative when available, otherwise the first seen.
                if (!byFamily.ContainsKey(family) || IsCanonicalRepresentative(tag, family))
                {
                    byFamily[family] = tag;
                }
            }

            return byFamily.Values.ToArray();
        }

        private static bool IsCanonicalRepresentative(string tag, string family)
        {
            var shortTag = ShortTag(tag);
            return family switch
            {
                "latin" => shortTag == "en",
                "cyrillic" => shortTag == "ru",
                "arabic" => shortTag == "ar",
                "greek" => shortTag == "el",
                _ => false
            };
        }

        private static string ScriptFamily(string tag)
        {
            var s = ShortTag(tag);
            switch (s)
            {
                case "zh":
                    return "zh";
                case "ja":
                    return "ja";
                case "ko":
                    return "ko";
                case "ru":
                case "uk":
                case "be":
                case "bg":
                case "sr":
                case "mk":
                    return "cyrillic";
                case "ar":
                case "fa":
                case "ur":
                    return "arabic";
                case "el":
                    return "greek";
                default:
                    return "latin";
            }
        }

        private static string ShortTag(string tag) => tag.Split('-')[0].ToLowerInvariant();

        /// <summary>Number of characters in the recognized text that belong to the language's script.</summary>
        private static int ScoreForLanguage(string text, string tag)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            var family = ScriptFamily(tag);
            var score = 0;
            foreach (var c in text)
            {
                if (MatchesScript(c, family))
                {
                    score++;
                }
            }

            return score;
        }

        private static bool MatchesScript(char c, string family)
        {
            switch (family)
            {
                case "zh":
                    return IsCjk(c);
                case "ja":
                    return IsCjk(c) || IsKana(c);
                case "ko":
                    return IsHangul(c);
                case "cyrillic":
                    return c >= 0x0400 && c <= 0x04FF;
                case "arabic":
                    return (c >= 0x0600 && c <= 0x06FF) || (c >= 0x0750 && c <= 0x077F) ||
                           (c >= 0xFB50 && c <= 0xFDFF) || (c >= 0xFE70 && c <= 0xFEFF);
                case "greek":
                    return c >= 0x0370 && c <= 0x03FF;
                default:
                    return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ||
                           (c >= 0x00C0 && c <= 0x024F);
            }
        }

        private static bool IsCjk(char c) => (c >= 0x4E00 && c <= 0x9FFF) || (c >= 0x3400 && c <= 0x4DBF);

        private static bool IsKana(char c) => c >= 0x3040 && c <= 0x30FF;

        private static bool IsHangul(char c) =>
            (c >= 0xAC00 && c <= 0xD7A3) || (c >= 0x1100 && c <= 0x11FF) || (c >= 0x3130 && c <= 0x318F);
    }
}
