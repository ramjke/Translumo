using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Constants;
using Translumo.Infrastructure.Dispatching;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;
using Translumo.Utils.Extensions;

namespace Translumo.Translation.Yandex
{
    public sealed class YandexTranslator : BaseTranslator<YandexContainer>
    {
        private readonly IActionDispatcher _actionDispatcher;

        private readonly AutoResetEvent _sync;

        public YandexTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService,
            IActionDispatcher actionDispatcher, ILogger logger) : base(translationConfiguration, languageService, logger)
        {
            _actionDispatcher = actionDispatcher;
            _sync = new AutoResetEvent(true);
        }

        protected override async Task<string> TranslateTextInternal(YandexContainer container, string sourceText)
        {
            var sourceLangCode = SourceLangDescriptor.IsoCode;
            var targetLangCode = TargetLangDescriptor.RegionalVariant ? TargetLangDescriptor.Code : TargetLangDescriptor.IsoCode;
            if (string.IsNullOrEmpty(container.Sid))
            {
                await _sync.WaitOneAsync(CancellationToken.None);
                try
                {
                    if (string.IsNullOrEmpty(container.Sid))
                    {
                        container.Sid = await container.Reader.RequestSidAsync().ConfigureAwait(false);
                        if (string.IsNullOrEmpty(container.Sid))
                        {
                            Logger.LogTrace("Trying to request SID through browser");
                            container.Sid = await RequestSidThroughBrowseAsync(container);
                            if (string.IsNullOrEmpty(container.Sid))
                            {
                                throw new TranslationException($"Sid extraction error");
                            }
                        }
                    }
                }
                finally
                {
                    _sync.Set();
                }
            }

            var request = YandexRequestFactory.CreateRequest(container, sourceText, sourceLangCode, targetLangCode);
            string translatedText = await container.Reader.RequestTranslationAsync(request).ConfigureAwait(false);

            return translatedText;
        }

        protected override IList<YandexContainer> CreateContainers(TranslationConfiguration configuration)
        {
            var result = configuration.ProxySettings.Select(proxy => new YandexContainer(proxy)).ToList();
            result.Add(new YandexContainer(isPrimary: true));

            return result;
        }

        private async Task<string> RequestSidThroughBrowseAsync(YandexContainer container)
        {
            var browseResult = await _actionDispatcher
                .DispatchActionAsync<BrowseSiteDispatchArg, BrowseSiteDispatchResult>(DispatcherActions.PASS_SITE,
                    new BrowseSiteDispatchArg()
                    {
                        SourceUrl = container.Reader.YandexRuUrl,
                        TargetUrl = container.Reader.YandexRuUrl,
                        Proxy = container.Proxy?.ToWebProxy()
                    }).ConfigureAwait(false);

            if (browseResult?.Cookies?.Any() ?? false)
            {
                browseResult.Cookies.ForEach(cookie => container.Reader.HttpReader.Cookies.Add(cookie));
            }

            return container.Reader.ExtractSid(browseResult?.HtmlPage);
        }
    }
}
