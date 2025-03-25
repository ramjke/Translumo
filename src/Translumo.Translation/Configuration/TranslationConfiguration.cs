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
            OllamaModel = "llama3",
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

        public string OllamaModel
        {
            get => _ollamaModel;
            set
            {
                SetProperty(ref _ollamaModel, value);
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
        private string _ollamaModel;
        private List<Proxy> _proxySettings = new List<Proxy>();
    }
}
