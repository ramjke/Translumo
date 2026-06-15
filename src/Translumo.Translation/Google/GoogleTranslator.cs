using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Constants;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;
using Translumo.Utils.Http;

namespace Translumo.Translation.Google
{
    public class GoogleTranslator : BaseTranslator<GoogleContainer>
    {
        private const string TRANSLATE_URL = "https://translate.google.com/m?hl={1}&sl={0}&tl={1}&ie=UTF-8&prev=_m&q={2}";

        public GoogleTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger)
            : base(translationConfiguration, languageService, logger)
        {
        }

        public override Task<string> TranslateTextAsync(string sourceText)
        {
            //TODO: Temp implementation for specific lang
            if (TargetLangDescriptor.Language == Languages.PortugueseBrazil)
            {
                throw new TransactionException("Google translate is unavailable for this language");
            }

            return base.TranslateTextAsync(sourceText);
        }



        protected override async Task<string> TranslateTextInternal(GoogleContainer container, string sourceText)
        {
            string url = string.Format(TRANSLATE_URL, SourceLangDescriptor.IsoCode, TargetLangDescriptor.IsoCode,
                HttpUtility.UrlEncode(sourceText));
            HttpResponse requestResult = await container.Reader.RequestWebDataAsync(url, HttpMethods.GET, true)
                .ConfigureAwait(false);
            if (requestResult.IsSuccessful)
            {
                var matchResult = RegexStorage.GoogleTranslateResultRegex.Match(requestResult.Body);
                if (!matchResult.Success)
                {
                    throw new TranslationException($"Unexpected web response: '{requestResult.Body}'");
                }

                return WebUtility.HtmlDecode(matchResult.Value);
            }

            throw new TranslationException($"Unexpected web response: '{requestResult.Body}'");
        }

        protected override IList<GoogleContainer> CreateContainers(TranslationConfiguration configuration)
        {
            var result = configuration.ProxySettings.Select(proxy => new GoogleContainer(proxy)).ToList();
            result.Add(new GoogleContainer(isPrimary: true));

            return result;
        }
    }
}
