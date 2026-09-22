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
            Translator = Translators.Google,
            DeeplApiKey = string.Empty,
            YandexApiKey = string.Empty
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

        public string DeeplApiKey
        {
            get => _deeplApiKey;
            set
            {
                SetProperty(ref _deeplApiKey, value);
            }
        }

        public string YandexApiKey
        {
            get => _yandexApiKey;
            set
            {
                SetProperty(ref _yandexApiKey, value);
            }
        }


        private Languages _translateFromLang;
        private Languages _translateToLang;
        private Translators _translator;
        private string _deeplApiKey = string.Empty;
        private string _yandexApiKey = string.Empty;
    }
}
