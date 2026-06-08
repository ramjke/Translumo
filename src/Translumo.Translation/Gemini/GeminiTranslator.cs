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

namespace Translumo.Translation.Gemini
{
    public sealed class GeminiTranslator : BaseTranslator<GeminiContainer>
    {
        private const string API_URL_TEMPLATE = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={0}";

        public GeminiTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger)
            : base(translationConfiguration, languageService, logger)
        {
        }

        protected override async Task<string> TranslateTextInternal(GeminiContainer container, string sourceText)
        {
            if (string.IsNullOrEmpty(container.ApiKey))
            {
                throw new TranslationException("Gemini API Key is not configured.");
            }

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"You are a highly accurate translator. Translate the following text from {SourceLangDescriptor.IsoCode} to {TargetLangDescriptor.IsoCode}. Reply ONLY with the translation, no extra text, no explanations.\n\nText: {sourceText}" }
                        }
                    }
                }
            };

            var dataIn = JsonSerializer.Serialize(payload);
            var apiUrl = string.Format(API_URL_TEMPLATE, container.ApiKey);
            HttpResponse httpResponse = await container.Reader.RequestWebDataAsync(apiUrl, HttpMethods.POST, dataIn, acceptCookie: false).ConfigureAwait(false);

            if (httpResponse.IsSuccessful)
            {
                try
                {
                    using var doc = JsonDocument.Parse(httpResponse.Body);
                    var candidates = doc.RootElement.GetProperty("candidates");
                    if (candidates.GetArrayLength() > 0)
                    {
                        var translation = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                        return translation.Trim();
                    }
                }
                catch (Exception ex)
                {
                    throw new TranslationException($"Failed to parse Gemini response: '{httpResponse.Body}'", ex);
                }
            }

            throw new TranslationException($"Gemini API returned error: '{httpResponse.Body}'", httpResponse.InnerException);
        }

        protected override IList<GeminiContainer> CreateContainers(TranslationConfiguration configuration)
        {
            var result = configuration.ProxySettings.Select(proxy => new GeminiContainer(configuration.GeminiApiKey, proxy)).ToList();
            result.Add(new GeminiContainer(configuration.GeminiApiKey, isPrimary: true));
            return result;
        }
    }
}
