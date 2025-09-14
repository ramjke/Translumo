using System.Collections.Generic;

namespace Translumo.Infrastructure.Language
{
    public class LanguageDescriptorFactory
    {
        public IEnumerable<LanguageDescriptor> GetAll()
        {
            return new LanguageDescriptor[]
            {
                new LanguageDescriptor()
                {
                    Language = Languages.English, Code = "en-US", EasyOcrCode = "en", EasyOcrModel = "english_g2",
                    TesseractCode = "eng", IsoCode = "en", TextScorePredictorModel = "eng",
                    SupportedNamedBlocks = new[] { "IsBasicLatin" }, UseEndPunctuation = true, UseWordTokenizer = false, UseSpaceRemover = false
                },
                new LanguageDescriptor()
                {
                    Language = Languages.Russian, Code = "ru-RU", EasyOcrCode = "ru", EasyOcrModel = "cyrillic_g2",
                    TesseractCode = "rus", IsoCode = "ru", TextScorePredictorModel = "rus",
                    SupportedNamedBlocks = new[] { "IsCyrillic" }, UseEndPunctuation = true, UseWordTokenizer = false, UseSpaceRemover = false
                },
                new LanguageDescriptor()
                {
                    Language = Languages.Chinese, Code = "zh-CN", EasyOcrCode = "ch_sim", EasyOcrModel = "zh_sim_g2",
                    TesseractCode = "chi_sim", IsoCode = "zh", TextScorePredictorModel = "chi",
                    SupportedNamedBlocks = new[] { "IsCJKUnifiedIdeographs" }, UseEndPunctuation = false,
                    UseWordTokenizer = true, UseSpaceRemover = true, Asian = true
                },
                new LanguageDescriptor()
                {
                    Language = Languages.Japanese, Code = "ja-JP", EasyOcrCode = "ja", EasyOcrModel = "japanese_g2",
                    TesseractCode = "jpn", IsoCode = "ja", TextScorePredictorModel = "jap",
                    SupportedNamedBlocks = new[] { "IsHiragana", "IsKatakana" }, UseEndPunctuation = false,
                    UseWordTokenizer = true, UseSpaceRemover = true, Asian = true
                },
                new LanguageDescriptor()
                {
                    Language = Languages.Korean, Code = "ko-KR", EasyOcrCode = "ko", EasyOcrModel = "korean_g2",
                    TesseractCode = "kor", IsoCode = "ko", TextScorePredictorModel = "kor",
                    SupportedNamedBlocks = new[] { "IsHangulJamo", "IsHangulSyllables" }, UseEndPunctuation = false,
                    UseWordTokenizer = true, UseSpaceRemover = false, Asian = true
                },
                new LanguageDescriptor() { Language = Languages.Italian, Code = "it-IT", IsoCode = "it", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.French, Code = "fr-FR", IsoCode = "fr", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.German, Code = "de-DE", IsoCode = "de", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Spanish, Code = "es-ES", IsoCode = "es", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Portuguese, Code = "pt-PT", IsoCode = "pt", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Vietnamese, Code = "vi-VN", IsoCode = "vi", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Turkish, Code = "tr-TR", IsoCode = "tr", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Thai, Code = "th-TH", IsoCode = "th", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Arabic, Code = "ar-SA", IsoCode = "ar", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Greek, Code = "el-GR", IsoCode = "el", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.PortugueseBrazil, Code = "pt-BR", IsoCode = "pt", TranslationOnly = true, RegionalVariant = true },
                new LanguageDescriptor() { Language = Languages.Polish, Code = "pl-PL", IsoCode = "pl", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Belarusian, Code = "be-BY", IsoCode = "be", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Persian, Code = "fa-IR", IsoCode = "fa", TranslationOnly = true, Asian = true },
                new LanguageDescriptor() { Language = Languages.Indonesian, Code = "id-ID", IsoCode = "id", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Bulgarian, Code = "bg-BG", IsoCode = "bg", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Czech, Code = "cs-CZ", IsoCode = "cs", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Danish, Code = "da-DK", IsoCode = "da", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Estonian, Code = "et-EE", IsoCode = "et", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Finnish, Code = "fi-FI", IsoCode = "fi", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Hungarian, Code = "hu-HU", IsoCode = "hu", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Lithuanian, Code = "lt-LT", IsoCode = "lt", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Latvian, Code = "lv-LV", IsoCode = "lv", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Dutch, Code = "nl-NL", IsoCode = "nl", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Romanian, Code = "ro-RO", IsoCode = "ro", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Slovak, Code = "sk-SK", IsoCode = "sk", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Slovenian, Code = "sl-SI", IsoCode = "sl", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Swedish, Code = "sv-SE", IsoCode = "sv", TranslationOnly = true },
                new LanguageDescriptor() { Language = Languages.Ukrainian, Code = "uk-UA", IsoCode = "uk", TranslationOnly = true },
            };
        }
    }
}
