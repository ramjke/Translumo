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

namespace Translumo.Translation.Openrouter
{
    public sealed class OpenrouterTranslator : BaseTranslator<OpenrouterContainer>
    {
        private const string API_URL = "https://openrouter.ai/api/v1/chat/completions";

        public OpenrouterTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger)
            : base(translationConfiguration, languageService, logger)
        {
        }

        protected override async Task<string> TranslateTextInternal(OpenrouterContainer container, string sourceText)
        {
            if (string.IsNullOrEmpty(container.ApiKey))
            {
                throw new TranslationException("Openrouter API Key is not configured.");
            }

            var modelName = !string.IsNullOrWhiteSpace(TranslationConfiguration.OpenrouterModel)
                ? TranslationConfiguration.OpenrouterModel
                : "nvidia/nemotron-3.5-content-safety:free";

            var payload = new
            {
                model = modelName,
                messages = new[]
                {
                    new { role = "system", content = $"You are a highly accurate translator. Translate the following text from {SourceLangDescriptor.IsoCode} to {TargetLangDescriptor.IsoCode}. Reply ONLY with the translation, no extra text, no explanations." },
                    new { role = "user", content = sourceText }
                },
                temperature = 0.3
            };

            var dataIn = JsonSerializer.Serialize(payload);
            HttpResponse httpResponse = await container.Reader.RequestWebDataAsync(API_URL, HttpMethods.POST, dataIn, acceptCookie: false).ConfigureAwait(false);

            if (httpResponse.IsSuccessful)
            {
                try
                {
                    using var doc = JsonDocument.Parse(httpResponse.Body);
                    var translation = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                    return translation.Trim();
                }
                catch (Exception ex)
                {
                    throw new TranslationException($"Failed to parse Openrouter response: '{httpResponse.Body}'", ex);
                }
            }

            throw new TranslationException($"Openrouter API returned error: '{httpResponse.Body}'", httpResponse.InnerException);
        }

        protected override IList<OpenrouterContainer> CreateContainers(TranslationConfiguration configuration)
        {
            var result = configuration.ProxySettings.Select(proxy => new OpenrouterContainer(configuration.OpenrouterApiKey, proxy)).ToList();
            result.Add(new OpenrouterContainer(configuration.OpenrouterApiKey, isPrimary: true));
            return result;
        }
    }
}
