using System.Text.Json.Serialization;

namespace Translumo.Translation.Yandex
{
    public class YandexResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("lang")]
        public string Lang { get; set; }

        [JsonPropertyName("text")]
        public string[] Text { get; set; }
    }
}
