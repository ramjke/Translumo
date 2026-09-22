using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;
using Translumo.Utils.Http;

namespace Translumo.Translation.Google
{
    public class GoogleTranslator : BaseTranslator<GoogleContainer>
    {
        private const string TRANSLATE_URL =
            "https://translate.googleapis.com/translate_a/single?client=gtx&dt=t&ie=UTF-8&oe=UTF-8&sl={0}&tl={1}";

        private const string RESERVE_TRANSLATE_URL =
            "https://clients5.google.com/translate_a/t?client=dict-chrome-ex&ie=UTF-8&oe=UTF-8&sl={0}&tl={1}";

        public GoogleTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger) 
            : base(translationConfiguration, languageService, logger)
        {
        }

        protected override async Task<string> TranslateTextInternal(GoogleContainer container, string sourceText)
        {
            var sourceLangCode = SourceLangDescriptor.IsoCode;
            var targetLangCode = TargetLangDescriptor.RegionalVariant ? TargetLangDescriptor.Code : TargetLangDescriptor.IsoCode;
            var requestData = $"q={HttpUtility.UrlEncode(sourceText)}";

            var response = await container.Reader
                .RequestWebDataAsync(string.Format(TRANSLATE_URL, sourceLangCode, targetLangCode), HttpMethods.POST, requestData)
                .ConfigureAwait(false);
            if (TryReadTranslation(response, out var translation))
            {
                return translation;
            }

            Logger.LogTrace($"Primary google endpoint is unavailable ({DescribeFailure(response)}), trying reserve one");

            var reserveResponse = await container.Reader
                .RequestWebDataAsync(string.Format(RESERVE_TRANSLATE_URL, sourceLangCode, targetLangCode), HttpMethods.POST, requestData)
                .ConfigureAwait(false);
            if (TryReadTranslation(reserveResponse, out translation))
            {
                return translation;
            }

            throw new TranslationException($"Unexpected web response: '{DescribeFailure(reserveResponse)}'");
        }
        
        protected override IList<GoogleContainer> CreateContainers(TranslationConfiguration configuration)
        {
            var result = configuration.ProxySettings.Select(proxy => new GoogleContainer(proxy)).ToList();
            result.Add(new GoogleContainer(isPrimary: true));

            return result;
        }

        private static bool TryReadTranslation(HttpResponse response, out string translation)
        {
            translation = null;
            if (!response.IsSuccessful || string.IsNullOrEmpty(response.Body))
            {
                return false;
            }

            try
            {
                using var document = JsonDocument.Parse(response.Body);
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
                {
                    return false;
                }

                var payload = root[0];
                if (payload.ValueKind == JsonValueKind.String)
                {
                    translation = payload.GetString();

                    return !string.IsNullOrEmpty(translation);
                }

                if (payload.ValueKind != JsonValueKind.Array)
                {
                    return false;
                }

                var builder = new StringBuilder();
                foreach (var sentence in payload.EnumerateArray())
                {
                    if (sentence.ValueKind == JsonValueKind.String)
                    {
                        builder.Append(sentence.GetString());
                    }
                    else if (sentence.ValueKind == JsonValueKind.Array && sentence.GetArrayLength() > 0 &&
                             sentence[0].ValueKind == JsonValueKind.String)
                    {
                        builder.Append(sentence[0].GetString());
                    }
                }

                translation = builder.ToString();

                return translation.Length > 0;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static string DescribeFailure(HttpResponse response) =>
            response.InnerException?.Message ?? response.Body ?? "empty response";
    }
}
