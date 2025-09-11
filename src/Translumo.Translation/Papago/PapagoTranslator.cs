using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;
using Translumo.Utils.Extensions;

namespace Translumo.Translation.Papago
{
    public sealed class PapagoTranslator : BaseTranslator<PapagoContainer>
    {
        private readonly AutoResetEvent _sync;
        private readonly HashSet<Languages> _unsupportedLanguages = new(new[]
        {
            Languages.Turkish, Languages.Arabic, Languages.PortugueseBrazil, Languages.Greek, Languages.Belarusian, Languages.Polish, Languages.Persian
        });

        public PapagoTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger) : 
            base(translationConfiguration, languageService, logger)
        {
            this._sync = new AutoResetEvent(true);
        }

        public override Task<string> TranslateTextAsync(string sourceText)
        {
            //TODO: Temp implementation for specific lang
            if (_unsupportedLanguages.Contains(TargetLangDescriptor.Language))
            {
                throw new TransactionException("Papago translator is unavailable for this language");
            }

            return base.TranslateTextAsync(sourceText);
        }


        protected override async Task<string> TranslateTextInternal(PapagoContainer container, string sourceText)
        {
            if (string.IsNullOrEmpty(container.AuthKey))
            {
                await _sync.WaitOneAsync(CancellationToken.None);
                try
                {
                    if (string.IsNullOrEmpty(container.AuthKey))
                    {
                        container.AuthKey = await container.Reader.RequestAuthKeyAsync();
                        if (string.IsNullOrEmpty(container.AuthKey))
                        {
                            throw new TranslationException($"Auth key extraction error");
                        }

                        Containers.Where(c => c != container).ForEach(c => c.AuthKey = container.AuthKey);
                    }
                }
                finally
                {
                    _sync.Set();
                }
            }

            var request = PapagoRequestFactory.CreateRequest(container, sourceText, SourceLangDescriptor.IsoCode,
                TargetLangDescriptor.IsoCode);

            return await container.Reader.RequestTranslationAsync(request);
        }

        protected override IList<PapagoContainer> CreateContainers(TranslationConfiguration configuration)
        {
            var result = configuration.ProxySettings.Select(proxy => new PapagoContainer(proxy)).ToList();
            result.Add(new PapagoContainer(isPrimary: true));

            return result;
        }
    }
}
