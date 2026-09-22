using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Translumo.Translation.Deepl
{
    public class DeepLResponse
    {
        [JsonPropertyName("translations")]
        public List<DeepLTranslation> Translations { get; set; }
    }

    public class DeepLTranslation
    {
        [JsonPropertyName("detected_source_language")]
        public string DetectedSourceLanguage { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
