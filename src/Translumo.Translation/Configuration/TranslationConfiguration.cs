using System.Collections.Generic;
using Translumo.Infrastructure.Language;
using Translumo.Utils;
using Translumo.Translation.Ai;

namespace Translumo.Translation.Configuration
{
    public class TranslationConfiguration : BindableBase
    {
        public static TranslationConfiguration Default => new TranslationConfiguration()
        {
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian,
            ProxySettings = new List<Proxy>(),
            LibreTranslateUrl = "http://localhost:5000",
            AiProvider = AiTranslatorProvider.Gemini,
            AiApiKey = string.Empty,
            AiModel = "gemini-3.5-flash",
            AiPromptTemplate = "You are an expert translator specializing in video game localization. Translate the text contextually and naturally from {0} to {1}. Output ONLY the direct translated text. Absolutely NO explanations, NO introductory phrases, NO markdown formatting, and NO code blocks. If the text is a single word or phrase, translate it as such."
        };

        public Languages TranslateFromLang
        {
            get => _translateFromLang;
            set
            {
                SetProperty(ref _translateFromLang, value);
            }
        }

        public Languages TranslateToLang
        {
            get => _translateToLang;
            set
            {
                SetProperty(ref _translateToLang, value);
            }
        }

        public Translators Translator
        {
            get => _translator;
            set
            {
                SetProperty(ref _translator, value);
            }
        }

        public string LibreTranslateUrl
        {
            get => _libreTranslateUrl;
            set
            {
                SetProperty(ref _libreTranslateUrl, value);
            }
        }

        public AiTranslatorProvider AiProvider
        {
            get => _aiProvider;
            set
            {
                SetProperty(ref _aiProvider, value);
            }
        }

        public string AiApiKey
        {
            get => _aiApiKey;
            set
            {
                SetProperty(ref _aiApiKey, value);
            }
        }

        public string GeminiApiKey
        {
            get => _geminiApiKey;
            set => SetProperty(ref _geminiApiKey, value);
        }

        public string DeepSeekApiKey
        {
            get => _deepSeekApiKey;
            set => SetProperty(ref _deepSeekApiKey, value);
        }

        public string OpenRouterApiKey
        {
            get => _openRouterApiKey;
            set => SetProperty(ref _openRouterApiKey, value);
        }

        public string AiModel
        {
            get => _aiModel;
            set
            {
                SetProperty(ref _aiModel, value);
            }
        }

        public string GeminiAiModel
        {
            get => _geminiAiModel;
            set => SetProperty(ref _geminiAiModel, value);
        }

        public string DeepSeekAiModel
        {
            get => _deepSeekAiModel;
            set => SetProperty(ref _deepSeekAiModel, value);
        }

        public string OpenRouterAiModel
        {
            get => _openRouterAiModel;
            set => SetProperty(ref _openRouterAiModel, value);
        }

        public string AiPromptTemplate
        {
            get => _aiPromptTemplate;
            set
            {
                SetProperty(ref _aiPromptTemplate, value);
            }
        }

        public List<Proxy> ProxySettings
        {
            get => _proxySettings;
            set
            {
                SetProperty(ref _proxySettings, value);
            }
        }

        private Languages _translateFromLang;
        private Languages _translateToLang;
        private Translators _translator;
        private string _libreTranslateUrl = "http://localhost:5000";
        private AiTranslatorProvider _aiProvider = AiTranslatorProvider.Gemini;
        private string _aiApiKey = string.Empty;
        private string _geminiApiKey = string.Empty;
        private string _deepSeekApiKey = string.Empty;
        private string _openRouterApiKey = string.Empty;
        private string _aiModel = "gemini-3.5-flash";
        private string _geminiAiModel = "gemini-3.5-flash";
        private string _deepSeekAiModel = "deepseek-v4-flash";
        private string _openRouterAiModel = string.Empty;
        private string _aiPromptTemplate = "You are an expert translator specializing in video game localization. Translate the text contextually and naturally from {0} to {1}. Output ONLY the direct translated text. Absolutely NO explanations, NO introductory phrases, NO markdown formatting, and NO code blocks. If the text is a single word or phrase, translate it as such.";
        private List<Proxy> _proxySettings = new List<Proxy>();
    }
}
