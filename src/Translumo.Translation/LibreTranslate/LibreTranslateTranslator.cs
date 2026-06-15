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

namespace Translumo.Translation.LibreTranslate
{
    public class LibreTranslateTranslator : BaseTranslator<LibreTranslateContainer>
    {
        public LibreTranslateTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger)
            : base(translationConfiguration, languageService, logger)
        {
        }

        protected override async Task<string> TranslateTextInternal(LibreTranslateContainer container, string sourceText)
        {
            var baseUrl = TranslationConfiguration.LibreTranslateUrl ?? "http://localhost:5000";
            var url = baseUrl.TrimEnd('/') + "/translate";

            var requestModel = new LibreTranslateRequest(
                sourceText,
                SourceLangDescriptor.IsoCode,
                TargetLangDescriptor.IsoCode
            );

            var requestBody = JsonSerializer.Serialize(requestModel);

            HttpResponse requestResult = await container.Reader.RequestWebDataAsync(url, HttpMethods.POST, requestBody, true)
                .ConfigureAwait(false);

            if (requestResult.IsSuccessful)
            {
                try
                {
                    var responseModel = JsonSerializer.Deserialize<LibreTranslateResponse>(requestResult.Body);
                    if (responseModel != null && !string.IsNullOrEmpty(responseModel.TranslatedText))
                    {
                        return responseModel.TranslatedText;
                    }
                }
                catch (Exception ex)
                {
                    throw new TranslationException($"Failed to deserialize LibreTranslate response: '{requestResult.Body}'", ex);
                }
            }

            throw new TranslationException($"LibreTranslate request failed. Error: {requestResult.InnerException?.Message}. Response: '{requestResult.Body}'");
        }

        protected override IList<LibreTranslateContainer> CreateContainers(TranslationConfiguration configuration)
        {
            var result = configuration.ProxySettings.Select(proxy => new LibreTranslateContainer(proxy)).ToList();
            result.Add(new LibreTranslateContainer(isPrimary: true));

            return result;
        }
    }
}
