using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;
using Translumo.Utils.Http;
using static Translumo.Translation.Deepl.DeepLRequest;
using static Translumo.Translation.Deepl.DeepLResponse;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Translumo.Translation.Deepl
{
    public sealed class DeepLTranslator : BaseTranslator<DeeplContainer>
    {
        private const string DEEPL_API_URL = "https://www2.deepl.com/jsonrpc";
        
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
            //TODO: Temp implementation for specific lang
            if (_unsupportedLanguages.Contains(TargetLangDescriptor.Language))
            {
                throw new TransactionException("DeepL translator is unavailable for this language");
            }

            return base.TranslateTextAsync(sourceText);
        }

        protected override async Task<string> TranslateTextInternal(DeeplContainer container, string sourceText)
        {
            var sourceLangCode = SourceLangDescriptor.IsoCode.ToUpper();
            var targetLangCode = TargetLangDescriptor.IsoCode.ToUpper();
            var regionalCode = TargetLangDescriptor.RegionalVariant ? TargetLangDescriptor.Code : string.Empty;

            var request = new DeepLTranslatorRequest(container.DeeplId, sourceText, sourceLangCode, targetLangCode, regionalCode);
            string dataIn = request.ToJsonString();
            HttpResponse httpResponse = await container.Reader.RequestWebDataAsync(DEEPL_API_URL, HttpMethods.POST, dataIn, acceptCookie: true)
                .ConfigureAwait(false);
            container.DeeplId++;

            if (httpResponse.IsSuccessful)
            {
                DeepLTranslationResponse deepLTranslationResponse = JsonSerializer.Deserialize<DeepLTranslationResponse>(httpResponse.Body);
                if (deepLTranslationResponse?.Result?.Translations != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    for (var i = 0; i < deepLTranslationResponse.Result.Translations.Count; i++)
                    {
                        Beam beam = deepLTranslationResponse.Result.Translations[i].Beams.FirstOrDefault();
                        if (beam?.PostProcessedSentence != null)
                        {
                            stringBuilder.Append(beam.PostProcessedSentence);
                            if (i < request.Params.Jobs.Count && request.Params.Jobs[i].NewLineFollows)
                            {
                                stringBuilder.Append(Environment.NewLine);
                            }
                            else
                            {
                                stringBuilder.Append(" ");
                            }
                        }
                    }

                    return stringBuilder.ToString().TrimEnd();
                }


                throw new TranslationException($"Unexpected body translation response: '{httpResponse.Body}'");
            }

            throw new TranslationException($"Response by translator service is not successful: '{httpResponse.Body}'", httpResponse.InnerException);
        }

        protected override IList<DeeplContainer> CreateContainers(TranslationConfiguration configuration)
        {
            var result = configuration.ProxySettings.Select(proxy => new DeeplContainer(proxy)).ToList();
            result.Add(new DeeplContainer(isPrimary: true));

            return result;
        }
    }
}
