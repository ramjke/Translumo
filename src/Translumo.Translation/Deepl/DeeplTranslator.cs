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

namespace Translumo.Translation.Deepl
{
    public sealed class DeepLTranslator : BaseTranslator<DeeplContainer>
    {
        private const string FREE_API_URL = "https://api-free.deepl.com/v2/translate";
        private const string PRO_API_URL = "https://api.deepl.com/v2/translate";
        private const string FREE_KEY_SUFFIX = ":fx";

        private readonly HashSet<Languages> _unsupportedLanguages = new(new[]
        {
            Languages.Vietnamese, Languages.Thai, Languages.Belarusian, Languages.Persian
        });

        public DeepLTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger)
            : base(translationConfiguration, languageService, logger)
        {
        }

        public override Task<string> TranslateTextAsync(string sourceText)
        {
            if (_unsupportedLanguages.Contains(TargetLangDescriptor.Language))
            {
                throw new TranslationException("DeepL translator is unavailable for this language");
            }

            if (string.IsNullOrWhiteSpace(TranslationConfiguration.DeeplApiKey))
            {
                throw new TranslationException("DeepL API key is not set. Add it in the language settings");
            }

            return base.TranslateTextAsync(sourceText);
        }

        protected override async Task<string> TranslateTextInternal(DeeplContainer container, string sourceText)
        {
            var apiKey = TranslationConfiguration.DeeplApiKey.Trim();
            var targetLangCode = TargetLangDescriptor.RegionalVariant
                ? TargetLangDescriptor.Code.ToUpperInvariant()
                : TargetLangDescriptor.IsoCode.ToUpperInvariant();

            var request = new DeepLRequest(sourceText, SourceLangDescriptor.IsoCode.ToUpperInvariant(), targetLangCode);
            container.Reader.OptionalHeaders["Authorization"] = $"DeepL-Auth-Key {apiKey}";

            var response = await container.Reader
                .RequestWebDataAsync(GetApiUrl(apiKey), HttpMethods.POST, JsonSerializer.Serialize(request))
                .ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                throw new TranslationException($"DeepL request failed: '{DescribeFailure(response)}'", response.InnerException);
            }

            try
            {
                var translated = JsonSerializer.Deserialize<DeepLResponse>(response.Body)?.Translations?.FirstOrDefault()?.Text;
                if (translated == null)
                {
                    throw new TranslationException($"Unexpected DeepL response: '{response.Body}'");
                }

                return translated;
            }
            catch (JsonException ex)
            {
                throw new TranslationException($"Unexpected DeepL response: '{response.Body}'", ex);
            }
        }

        protected override IList<DeeplContainer> CreateContainers(TranslationConfiguration configuration)
        {
            return new List<DeeplContainer> { new DeeplContainer(isPrimary: true) };
        }

        private static string GetApiUrl(string apiKey)
        {
            return apiKey.EndsWith(FREE_KEY_SUFFIX, StringComparison.OrdinalIgnoreCase) ? FREE_API_URL : PRO_API_URL;
        }

        private static string DescribeFailure(HttpResponse response)
        {
            if (response.InnerException is WebException webException && webException.Response is HttpWebResponse httpResponse)
            {
                switch ((int)httpResponse.StatusCode)
                {
                    case 403:
                        return "the API key was rejected";
                    case 429:
                        return "too many requests, try again later";
                    case 456:
                        return "translation quota exceeded for this key";
                    default:
                        return $"service responded with {(int)httpResponse.StatusCode}";
                }
            }

            return response.InnerException?.Message ?? response.Body ?? "empty response";
        }
    }
}
