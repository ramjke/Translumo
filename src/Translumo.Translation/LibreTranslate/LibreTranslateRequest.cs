using System.Text.Json.Serialization;

namespace Translumo.Translation.LibreTranslate
{
    public class LibreTranslateRequest
    {
        [JsonPropertyName("q")]
        public string Text { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("target")]
        public string Target { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; } = "text";

        [JsonPropertyName("api_key")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ApiKey { get; set; }

        public LibreTranslateRequest(string text, string source, string target, string apiKey = null)
        {
            this.Text = text;
            this.Source = source;
            this.Target = target;
            this.ApiKey = apiKey;
        }
    }
}
