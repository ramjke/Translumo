using System.Text.Json.Serialization;

namespace Translumo.Translation.Deepl
{
    public class DeepLRequest
    {
        [JsonPropertyName("text")]
        public string[] Text { get; set; }

        [JsonPropertyName("source_lang")]
        public string SourceLang { get; set; }

        [JsonPropertyName("target_lang")]
        public string TargetLang { get; set; }

        public DeepLRequest(string text, string sourceLang, string targetLang)
        {
            Text = new[] { text };
            SourceLang = sourceLang;
            TargetLang = targetLang;
        }
    }
}
