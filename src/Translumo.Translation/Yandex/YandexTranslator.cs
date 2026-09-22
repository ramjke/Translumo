using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;
using Translumo.Utils.Http;

namespace Translumo.Translation.Yandex
{
    public sealed class YandexTranslator : BaseTranslator<YandexContainer>
    {
        private const string CLOUD_API_URL = "https://translate.api.cloud.yandex.net/translate/v2/translate";

        public YandexTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger)
            : base(translationConfiguration, languageService, logger)
        {
        }

        public override Task<string> TranslateTextAsync(string sourceText)
        {
            if (string.IsNullOrWhiteSpace(TranslationConfiguration.YandexApiKey))
            {
                throw new TranslationException("Yandex API key is not set. Add it in the language settings");
            }

            return base.TranslateTextAsync(sourceText);
        }

        protected override async Task<string> TranslateTextInternal(YandexContainer container, string sourceText)
        {
            var apiKey = TranslationConfiguration.YandexApiKey.Trim();
            var targetLangCode = TargetLangDescriptor.RegionalVariant ? TargetLangDescriptor.Code : TargetLangDescriptor.IsoCode;
            var request = new YandexApiRequest(sourceText, SourceLangDescriptor.IsoCode, targetLangCode);
            container.Reader.OptionalHeaders["Authorization"] = $"Api-Key {apiKey}";

            var response = await container.Reader
                .RequestWebDataAsync(CLOUD_API_URL, HttpMethods.POST, JsonSerializer.Serialize(request))
                .ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                throw new TranslationException($"Yandex request failed: '{DescribeFailure(response)}'", response.InnerException);
            }

            try
            {
                var translated = JsonSerializer.Deserialize<YandexApiResponse>(response.Body)?.Translations?.FirstOrDefault()?.Text;
                if (translated == null)
                {
                    throw new TranslationException($"Unexpected Yandex response: '{response.Body}'");
                }

                return translated;
            }
            catch (JsonException ex)
            {
                throw new TranslationException($"Unexpected Yandex response: '{response.Body}'", ex);
            }
        }

        protected override IList<YandexContainer> CreateContainers(TranslationConfiguration configuration)
        {
            return new List<YandexContainer> { new YandexContainer(isPrimary: true) };
        }

        private static string DescribeFailure(HttpResponse response)
        {
            if (response.InnerException is WebException webException && webException.Response is HttpWebResponse httpResponse)
            {
                switch ((int)httpResponse.StatusCode)
                {
                    case 401:
                        return "the API key was rejected";
                    case 403:
                        return "the API key has no access to the translate service";
                    case 429:
                        return "too many requests, try again later";
                    default:
                        return $"service responded with {(int)httpResponse.StatusCode}";
                }
            }

            return response.InnerException?.Message ?? response.Body ?? "empty response";
        }
    }
}
