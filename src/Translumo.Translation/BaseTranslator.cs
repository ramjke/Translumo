using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;

namespace Translumo.Translation
{
    public abstract class BaseTranslator<TContainer> : ITranslator
        where TContainer : TranslationContainer
    {
        public int AttemptDelayMs { get; set; } = 700;

        protected IList<TContainer> Containers;

        protected readonly LanguageDescriptor SourceLangDescriptor;
        protected readonly LanguageDescriptor TargetLangDescriptor;

        protected readonly TranslationConfiguration TranslationConfiguration;
        protected readonly ILogger Logger;

        protected BaseTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger)
        {
            this.TranslationConfiguration = translationConfiguration;
            this.Logger = logger;
            SourceLangDescriptor = languageService.GetLanguageDescriptor(translationConfiguration.TranslateFromLang);
            TargetLangDescriptor = languageService.GetLanguageDescriptor(translationConfiguration.TranslateToLang);
        }

        public virtual async Task<string> TranslateTextAsync(string sourceText)
        {
            if (SourceLangDescriptor.Language == TargetLangDescriptor.Language)
            {
                return sourceText;
            }

            if (Containers == null)
            {
                Containers = CreateContainers(TranslationConfiguration);
            }

            var container = GetContainer(true);
            while (true)
            {
                try
                {
                    var result = await TranslateTextInternal(container, sourceText);
                    container.MarkContainerIsUsed(true);

                    return result;
                }
                catch (TranslationException ex)
                {
                    container.MarkContainerIsUsed(false);
                    if (container.IsBlocked && !container.IsPrimary)
                    {
                        Logger.LogWarning($"Translation container is blocked until {container.BlockedUntilUtc.Value.ToLocalTime()} ({container.Proxy})");
                    }
                    
                    var backupContainer = GetContainer(false, container);
                    if (backupContainer == null)
                    {
                        throw;
                    }

                    if (backupContainer == container)
                    {
                        await Task.Delay(AttemptDelayMs);
                    }

                    container = backupContainer;
                }
                catch (Exception ex)
                {
                    container.MarkContainerIsUsed(false);
                    throw;
                }
            }
        }

        protected abstract Task<string> TranslateTextInternal(TContainer container, string sourceText);
        protected abstract IList<TContainer> CreateContainers(TranslationConfiguration configuration);

        protected virtual TContainer GetContainer(bool usePrimary, TContainer lastUsedContainer = null)
        {
            var targetContainer = Containers.Where(container => !container.IsBlocked)
                .MinBy(container => container == lastUsedContainer ? DateTime.MaxValue :  container.LastTimeUsedUtc);

            if (targetContainer == null && usePrimary)
            {
                targetContainer = Containers.First(container => container.IsPrimary);
                targetContainer.Restore();
            }

            return targetContainer;
        }

    }
}
