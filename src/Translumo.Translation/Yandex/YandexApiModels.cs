using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Translumo.Translation.Yandex
{
    public class YandexApiRequest
    {
        [JsonPropertyName("texts")]
        public string[] Texts { get; set; }

        [JsonPropertyName("sourceLanguageCode")]
        public string SourceLanguageCode { get; set; }

        [JsonPropertyName("targetLanguageCode")]
        public string TargetLanguageCode { get; set; }

        public YandexApiRequest(string text, string sourceLanguageCode, string targetLanguageCode)
        {
            Texts = new[] { text };
            SourceLanguageCode = sourceLanguageCode;
            TargetLanguageCode = targetLanguageCode;
        }
    }

    public class YandexApiResponse
    {
        [JsonPropertyName("translations")]
        public List<YandexApiTranslation> Translations { get; set; }
    }

    public class YandexApiTranslation
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("detectedLanguageCode")]
        public string DetectedLanguageCode { get; set; }
    }
}
