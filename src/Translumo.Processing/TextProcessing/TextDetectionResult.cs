using System;
using Translumo.Infrastructure.Language;
using Translumo.OCR;

namespace Translumo.Processing.TextProcessing
{
    public class TextDetectionResult : IComparable
    {
        public string Text { get; set; }

        public string ValidatedText { get; set; }

        public float ValidityScore { get; set; }

        public IOCREngine SourceEngine { get; }

        public LanguageDescriptor Language { get; }

        private const float LONG_SCORE_THRESHOLD = 3.81f;
        private const float EQUAL_TOLERANCE = 0.001f;

        public TextDetectionResult(IOCREngine engine, LanguageDescriptor languageDescriptor)
        {
            SourceEngine = engine;
            Language = languageDescriptor;
        }

        public int CompareTo(object obj)
        {
            var anotherResult = obj as TextDetectionResult;
            if (anotherResult == null)
            {
                return 1;
            }

            var shorterLength = Math.Min(this.ValidatedText.Length, anotherResult.ValidatedText.Length);
            var longerLength = Math.Max(this.ValidatedText.Length, anotherResult.ValidatedText.Length);

            var multiplicator = Math.Max(2.5 - shorterLength, 0) + 2;
            if (shorterLength > 0 && longerLength / (double)shorterLength >= multiplicator)
            {
                if (longerLength == this.ValidatedText.Length && this.ValidityScore >= LONG_SCORE_THRESHOLD)
                {
                    return 1;
                }

                if (longerLength == anotherResult.ValidatedText.Length && anotherResult.ValidityScore >= LONG_SCORE_THRESHOLD)
                {
                    return -1;
                }
            }

            if (Math.Abs(ValidityScore - anotherResult.ValidityScore) < EQUAL_TOLERANCE)
            {
                return SourceEngine.Confidence > anotherResult.SourceEngine.Confidence ? 1 : -1;
            }

            return ValidityScore.CompareTo(anotherResult.ValidityScore);
        }
    }
}
