using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;
using Translumo.Utils.Http;

namespace Translumo.Translation.Riva
{
    public class RivaTranslator : BaseTranslator<RivaContainer>
    {
        public RivaTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger)
            : base(translationConfiguration, languageService, logger)
        {
        }

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        protected override async Task<string> TranslateTextInternal(RivaContainer container, string sourceText)
        {
            string url = TranslationConfiguration.RivaUrl;
            string apiKey = TranslationConfiguration.RivaApiKey;

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new TranslationException("Riva URL is not configured. Please configure it in Settings.");
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new TranslationException("Riva API Key is not configured. Please configure it in Settings.");
            }

            container.Reader.OptionalHeaders.Clear();
            container.Reader.OptionalHeaders["Authorization"] = $"Bearer {apiKey}";
            container.Reader.OptionalHeaders["Accept"] = "application/json";

            var requestPayload = new OpenAiChatRequest
            {
                model = "nvidia/riva-translate-4b-instruct-v1.1",
                messages = new List<OpenAiMessage>
                {
                    new OpenAiMessage 
                    { 
                        role = "user", 
                        content = $"Translate from {SourceLangDescriptor.Language} to {TargetLangDescriptor.Language}:\n{sourceText}" 
                    }
                }
            };

            string requestBody = JsonSerializer.Serialize(requestPayload);

            HttpResponse requestResult = await container.Reader.RequestWebDataAsync(url, HttpMethods.POST, requestBody, true)
                .ConfigureAwait(false);

            if (requestResult.IsSuccessful)
            {
                try
                {
                    var response = JsonSerializer.Deserialize<OpenAiChatResponse>(requestResult.Body, JsonOptions);
                    var text = response?.choices?.FirstOrDefault()?.message?.content;
                    if (!string.IsNullOrEmpty(text))
                    {
                        return text.Trim();
                    }
                }
                catch (Exception ex)
                {
                    throw new TranslationException($"Failed to deserialize Riva response: '{requestResult.Body}'", ex);
                }
            }

            throw new TranslationException($"Riva translation failed. Response: '{requestResult.Body}'");
        }

        protected override IList<RivaContainer> CreateContainers(TranslationConfiguration configuration)
        {
            var result = configuration.ProxySettings.Select(proxy => new RivaContainer(proxy)).ToList();
            result.Add(new RivaContainer(isPrimary: true));

            return result;
        }

        #region JSON Request/Response Models

        private class OpenAiChatRequest
        {
            public string model { get; set; }
            public List<OpenAiMessage> messages { get; set; }
            public double temperature { get; set; } = 0.1;
        }

        private class OpenAiMessage
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        private class OpenAiChatResponse
        {
            public List<OpenAiChoice> choices { get; set; }
        }

        private class OpenAiChoice
        {
            public OpenAiMessage message { get; set; }
        }

        #endregion
    }
}
