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

namespace Translumo.Translation.Ai
{
    public class AiTranslator : BaseTranslator<AiContainer>
    {
        public AiTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger) 
            : base(translationConfiguration, languageService, logger)
        {
        }

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        protected override async Task<string> TranslateTextInternal(AiContainer container, string sourceText)
        {
            var provider = TranslationConfiguration.AiProvider;
            
            string apiKey = string.Empty;
            if (provider == AiTranslatorProvider.Gemini) apiKey = TranslationConfiguration.GeminiApiKey;
            else if (provider == AiTranslatorProvider.DeepSeek) apiKey = TranslationConfiguration.DeepSeekApiKey;
            else if (provider == AiTranslatorProvider.OpenRouter) apiKey = TranslationConfiguration.OpenRouterApiKey;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new TranslationException($"API Key for {provider} is empty. Please configure it in Settings.");
            }

            string prompt;
            try
            {
                prompt = string.Format(
                    TranslationConfiguration.AiPromptTemplate ?? "{0} to {1}",
                    SourceLangDescriptor.Language.ToString(),
                    TargetLangDescriptor.Language.ToString()
                );
            }
            catch (FormatException)
            {
                prompt = $"Translate from {SourceLangDescriptor.Language} to {TargetLangDescriptor.Language}.\n\nAdditional Instructions:\n{TranslationConfiguration.AiPromptTemplate}";
            }

            string modelStr = string.Empty;
            if (provider == AiTranslatorProvider.Gemini) modelStr = TranslationConfiguration.GeminiAiModel;
            else if (provider == AiTranslatorProvider.DeepSeek) modelStr = TranslationConfiguration.DeepSeekAiModel;
            else if (provider == AiTranslatorProvider.OpenRouter) modelStr = TranslationConfiguration.OpenRouterAiModel;
            
            string model;

            string url;
            string requestBody;

            // Clear previous headers to be safe
            container.Reader.OptionalHeaders.Clear();

            if (provider == AiTranslatorProvider.Gemini)
            {
                model = string.IsNullOrWhiteSpace(modelStr) ? "gemini-3.5-flash" : modelStr.Trim();
                url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                
                var geminiRequest = new GeminiRequest
                {
                    contents = new List<GeminiContent>
                    {
                        new GeminiContent
                        {
                            parts = new List<GeminiPart>
                            {
                                new GeminiPart { text = $"{prompt}\n\nText to translate:\n{sourceText}" }
                            }
                        }
                    }
                };

                requestBody = JsonSerializer.Serialize(geminiRequest);
            }
            else if (provider == AiTranslatorProvider.DeepSeek)
            {
                model = string.IsNullOrWhiteSpace(modelStr) ? "deepseek-v4-flash" : modelStr.Trim();
                url = "https://api.deepseek.com/v1/chat/completions";
                container.Reader.OptionalHeaders["Authorization"] = $"Bearer {apiKey}";

                var chatRequest = new OpenAiChatRequest
                {
                    model = model,
                    messages = new List<OpenAiMessage>
                    {
                        new OpenAiMessage { role = "system", content = prompt },
                        new OpenAiMessage { role = "user", content = sourceText }
                    }
                };

                requestBody = JsonSerializer.Serialize(chatRequest);
            }
            else if (provider == AiTranslatorProvider.OpenRouter)
            {
                model = modelStr?.Trim() ?? string.Empty;
                url = "https://openrouter.ai/api/v1/chat/completions";
                container.Reader.OptionalHeaders["Authorization"] = $"Bearer {apiKey}";
                container.Reader.OptionalHeaders["HTTP-Referer"] = "https://github.com/ramjke/Translumo";
                container.Reader.OptionalHeaders["X-Title"] = "Translumo";

                var chatRequest = new OpenAiChatRequest
                {
                    model = model,
                    messages = new List<OpenAiMessage>
                    {
                        new OpenAiMessage { role = "system", content = prompt },
                        new OpenAiMessage { role = "user", content = sourceText }
                    }
                };

                requestBody = JsonSerializer.Serialize(chatRequest);
            }
            else
            {
                throw new NotSupportedException($"AI Provider {provider} is not supported.");
            }

            HttpResponse requestResult = await container.Reader.RequestWebDataAsync(url, HttpMethods.POST, requestBody, true)
                .ConfigureAwait(false);

            if (requestResult.IsSuccessful)
            {
                try
                {
                    if (provider == AiTranslatorProvider.Gemini)
                    {
                        var response = JsonSerializer.Deserialize<GeminiResponse>(requestResult.Body, JsonOptions);
                        var text = response?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text;
                        if (!string.IsNullOrEmpty(text))
                        {
                            return text.Trim();
                        }
                    }
                    else // DeepSeek and OpenRouter
                    {
                        var response = JsonSerializer.Deserialize<OpenAiChatResponse>(requestResult.Body, JsonOptions);
                        var text = response?.choices?.FirstOrDefault()?.message?.content;
                        if (!string.IsNullOrEmpty(text))
                        {
                            return text.Trim();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new TranslationException($"Failed to deserialize AI translator response: '{requestResult.Body}'", ex);
                }
            }

            throw new TranslationException($"AI translator request failed. Error: {requestResult.InnerException?.Message}. Response: '{requestResult.Body}'");
        }

        protected override IList<AiContainer> CreateContainers(TranslationConfiguration configuration)
        {
            var result = configuration.ProxySettings.Select(proxy => new AiContainer(proxy)).ToList();
            result.Add(new AiContainer(isPrimary: true));

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

        private class GeminiRequest
        {
            public List<GeminiContent> contents { get; set; }
        }

        private class GeminiContent
        {
            public List<GeminiPart> parts { get; set; }
        }

        private class GeminiPart
        {
            public string text { get; set; }
        }

        private class GeminiResponse
        {
            public List<GeminiCandidate> candidates { get; set; }
        }

        private class GeminiCandidate
        {
            public GeminiContent content { get; set; }
        }

        #endregion
    }
}
