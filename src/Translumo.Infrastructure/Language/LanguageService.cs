using System;
using System.Collections.Generic;
using System.Linq;

namespace Translumo.Infrastructure.Language
{
    public class LanguageService
    {
        private readonly IDictionary<Languages, LanguageDescriptor> _langDescriptors;

        public LanguageService(LanguageDescriptorFactory langFactory)
        {
            _langDescriptors = langFactory.GetAll().ToDictionary(lang => lang.Language, lang => lang);
        }

        public LanguageDescriptor GetLanguageDescriptor(Languages language)
        {
            if (!_langDescriptors.ContainsKey(language))
            {
                throw new ArgumentException(nameof(language), "Unknown language");
            }

            return _langDescriptors[language];
        }

        public IEnumerable<LanguageDescriptor> GetAll(bool includeTranslationOnly = false)
        {
            return _langDescriptors.Where(lang => !lang.Value.TranslationOnly || includeTranslationOnly)
                .Select(lang => lang.Value)
                .ToArray();
        }
    }
}
