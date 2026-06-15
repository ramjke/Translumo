using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Constants;
using Translumo.Infrastructure.Language;
using Translumo.Infrastructure.MachineLearning;
using Translumo.Utils;

namespace Translumo.Processing.TextProcessing
{
    public class TextValidityPredictor : IDisposable
    {
        public Languages Language
        {
            get => _language;
            set
            {
                _language = value;
                Init();
            }
        }

        private Languages _language;
        private LanguageDescriptor _languageDescriptor;
        private Regex _langAlphabetRegex;
        private TextTokenizer _textTokenizer;

        private readonly IPredictor<InputTextPrediction, OutputTextPrediction> _validityPredictor;
        private readonly LanguageService _languageService;
        private readonly string _predictionModelsPath = Path.Combine(Global.ModelsPath, "Prediction");
        private readonly ILogger _logger;
        private readonly ManualResetEvent _sync = new ManualResetEvent(false);

        private readonly IReadOnlyDictionary<string, string> _replacers = new Dictionary<string, string>()
        {
            { "，", "," },
            { " 、", "," },
            { "！", "!" },
            { "？", "?" },
            { "；", ";" },
            { "：", ":" },
            { "（", "(" },
            { "）", ")" },
            { "［", "[" },
            { "］", "]" },
            { "【", "[" },
            { "】", "]" },
            { "、", "," },
            { "…", "..." },
            { "⸺", "-" },
            { "．", ". " },
            { "♪", "" },
            { "〟", "\"" },
            { "〝", "\"" },
            { "”", "\"" },
            { "“", "\"" },
            { "・・・", "..." },
            { "‥", ".." },
            { "`", "'" },
            { "—", "-" },
            { "′", "'" },

            { "０", "0" },
            { "１", "1" },
            { "２", "2" },
            { "３", "3" },
            { "４", "4" },
            { "５", "5" },
            { "６", "6" },
            { "７", "7" },
            { "８", "8" },
            { "９", "9" },
        };

        public TextValidityPredictor(IPredictor<InputTextPrediction, OutputTextPrediction> validityPredictor, LanguageService languageService,
            ILogger<TextValidityPredictor> logger)
        {
            _validityPredictor = validityPredictor;
            _languageService = languageService;
            _logger = logger;
        }

        public float Predict(string[] textLines, out string validatedText)
        {
            const int PREDICT_ATTEMPTS = 3;
            validatedText = string.Join(' ', textLines);
            if (string.IsNullOrWhiteSpace(validatedText) || !_langAlphabetRegex.IsMatch(validatedText))
            {
                return 0.0f;
            }

            validatedText = PreProcessText(textLines);
            if (_textTokenizer != null)
            {
                validatedText = _textTokenizer.Tokenize(validatedText);
            }

            var curAttempt = 0;
            if (!_validityPredictor.Loaded)
            {
                _sync.WaitOne(6000);
            }

            while (true)
            {
                curAttempt++;
                try
                {
                    return _validityPredictor.PredictResult(new InputTextPrediction() { Text = validatedText }).Validity;
                }
                catch (Exception)
                {
                    if (curAttempt >= PREDICT_ATTEMPTS)
                    {
                        return default(float);
                    }
                }
            }
        }

        private void Init()
        {
            _sync.Reset();
            _logger.LogTrace($"Initialization prediction model (lang: {Language})");
            _languageDescriptor = _languageService.GetLanguageDescriptor(Language);
            var modelPath = Path.Combine(_predictionModelsPath, _languageDescriptor.TextScorePredictorModel);

            try
            {
                _validityPredictor.LoadModel(modelPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Predictor model loading error");
                throw;
            }

            _sync.Set();
            _langAlphabetRegex = BuildAlphabetRegex(_languageDescriptor.SupportedNamedBlocks);
            _textTokenizer = _languageDescriptor.UseWordTokenizer ? new TextTokenizer(_languageDescriptor.Code) : null;
        }

        private string PreProcessText(IEnumerable<string> textLines)
        {
            var result = new StringBuilder();
            foreach (var textLine in textLines)
            {
                var lineRes = NormalizeString(textLine);
                lineRes = lineRes.Trim('-', ' ');
                if (lineRes.Length > 1 && lineRes[0] == lineRes[^1] && (lineRes[0] == '\'' || lineRes[0] == '"'))
                {
                    lineRes = lineRes[1..^1];
                }

                lineRes = RegexStorage.EndDotRegex.Replace(RegexStorage.StartDotRegex.Replace(lineRes, string.Empty),
                    string.Empty);

                if (!string.IsNullOrWhiteSpace(lineRes))
                {
                    result.Append((result.Length > 0 ? " " : string.Empty) + lineRes);
                }
            }

            var resultStr = result.ToString().Trim();
            if (_languageDescriptor.UseEndPunctuation && resultStr.Length > 0 && !resultStr[^1].IsSeparator())
            {
                resultStr += ".";
            }

            return RegexStorage.MultipleSpacesRegex.Replace(resultStr, " ");
        }

        private string NormalizeString(string str)
        {
            var strBuilder = new StringBuilder(str.Length);
            for (var i = 0; i < str.Length; i++)
            {
                if (_replacers.ContainsKey(str[i].ToString()))
                {
                    strBuilder.Append(_replacers[str[i].ToString()]);
                }
                else
                {
                    strBuilder.Append(str[i]);
                }
            }

            return strBuilder.ToString();
        }

        private Regex BuildAlphabetRegex(string[] supportedNamedBlocks)
        {
            var supportedBlocks = string.Join('|', supportedNamedBlocks.Select(block => $@"(\p{{{block}}})"));

            return new Regex($@"(?={supportedBlocks})(?=[^\d\W])", RegexOptions.Compiled);
        }

        public void Dispose()
        {
            _validityPredictor?.Dispose();
        }
    }
}
