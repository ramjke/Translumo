using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Translumo.Infrastructure.Constants;
using Translumo.Infrastructure.Language;
using Translumo.OCR;
using Translumo.Processing.Configuration;
using Translumo.Processing.Exceptions;
using static System.Threading.Tasks.Task;

namespace Translumo.Processing.TextProcessing
{
    public class TextDetectionProvider : IDisposable
    {
        public Languages Language
        {
            get => _textValidityPredictor.Language;
            set
            {
                _textValidityPredictor.Language = value;
                _languageDescriptor = _languageService.GetLanguageDescriptor(value);
            }
        }

        private LanguageDescriptor _languageDescriptor;

        private readonly TextValidityPredictor _textValidityPredictor;
        private readonly LanguageService _languageService;
        private readonly TextProcessingConfiguration _configuration;

        public TextDetectionProvider(TextValidityPredictor textValidityPredictor, LanguageService languageService,
            TextProcessingConfiguration configuration)
        {
            _textValidityPredictor = textValidityPredictor;
            _languageService = languageService;
            _configuration = configuration;
        }

        public virtual TextDetectionResult GetText(IOCREngine ocrEngine, byte[] img)
        {
            try
            {
                var detectedLines = ocrEngine.GetTextLines(img);
                var resultText = PreProcessTextLines(detectedLines);
                var scorePrediction = _textValidityPredictor.Predict(detectedLines, out var validatedText);

                return new TextDetectionResult(ocrEngine, _languageDescriptor)
                {
                    ValidityScore = scorePrediction,
                    Text = _configuration.KeepFormatting ? string.Join(Environment.NewLine, detectedLines) : resultText,
                    ValidatedText = validatedText
                };
            }
            catch (Exception ex)
            {
                throw new TextDetectionException("Text detection failed", ocrEngine.GetType(), ex);
            }
        }

        public virtual Task<TextDetectionResult> GetTextAsync(IOCREngine ocrEngine, byte[] img)
        {
            return Factory.StartNew(() => GetText(ocrEngine, img));
        }

        private string PreProcessTextLines(IEnumerable<string> textLines)
        {
            return RegexStorage.MultipleSpacesRegex.Replace(string.Join(' ', textLines), " ");
        }

        public void Dispose()
        {
            _textValidityPredictor.Dispose();
        }
    }
}
