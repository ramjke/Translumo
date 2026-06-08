using System.Collections.Generic;
using Translumo.Infrastructure.Language;
using Translumo.Utils;

namespace Translumo.Translation.Configuration
{
    public class TranslationConfiguration : BindableBase
    {
        public static TranslationConfiguration Default => new TranslationConfiguration()
        {
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian,
            ProxySettings = new List<Proxy>()
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

        public string DeepseekApiKey
        {
            get => _deepseekApiKey;
            set
            {
                SetProperty(ref _deepseekApiKey, value);
            }
        }

        public string GeminiApiKey
        {
            get => _geminiApiKey;
            set
            {
                SetProperty(ref _geminiApiKey, value);
            }
        }

        public string OpenrouterApiKey
        {
            get => _openrouterApiKey;
            set
            {
                SetProperty(ref _openrouterApiKey, value);
            }
        }

        public string OpenrouterModel
        {
            get => _openrouterModel;
            set
            {
                SetProperty(ref _openrouterModel, value);
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
        private string _deepseekApiKey;
        private string _geminiApiKey;
        private string _openrouterApiKey;
        private string _openrouterModel = "nvidia/nemotron-3.5-content-safety:free";
        private List<Proxy> _proxySettings = new List<Proxy>();
    }
}
