using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Translumo.Translation.DeepSeek
{
    public sealed class DeepSeekChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        public DeepSeekChatMessage()
        {
        }

        public DeepSeekChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }

    public sealed class DeepSeekRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        public IList<DeepSeekChatMessage> Messages { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        public DeepSeekRequest(IList<DeepSeekChatMessage> messages, string model, double temperature)
        {
            Model = model;
            Messages = messages;
            Temperature = temperature;
            Stream = false;
        }
    }

    public sealed class DeepSeekResponseMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public sealed class DeepSeekResponseChoice
    {
        [JsonPropertyName("message")]
        public DeepSeekResponseMessage Message { get; set; }
    }

    public sealed class DeepSeekResponseError
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }
    }

    public sealed class DeepSeekResponse
    {
        [JsonPropertyName("choices")]
        public IList<DeepSeekResponseChoice> Choices { get; set; }

        [JsonPropertyName("error")]
        public DeepSeekResponseError Error { get; set; }
    }
}
