using System.Text.Json.Serialization;

namespace Translumo.Translation.LibreTranslate
{
    public class LibreTranslateResponse
    {
        [JsonPropertyName("translatedText")]
        public string TranslatedText { get; set; }
    }
}
