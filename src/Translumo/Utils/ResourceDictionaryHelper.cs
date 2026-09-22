using System.Windows;

namespace Translumo.Utils
{
    internal static class ResourceDictionaryHelper
    {
        public static ResourceDictionary FindBySource(ResourceDictionary owner, string sourceFragment)
        {
            foreach (var dictionary in owner.MergedDictionaries)
            {
                if (dictionary.Source?.OriginalString.Contains(sourceFragment) ?? false)
                {
                    return dictionary;
                }

                var nestedDictionary = FindBySource(dictionary, sourceFragment);
                if (nestedDictionary != null)
                {
                    return nestedDictionary;
                }
            }

            return null;
        }

        public static bool TryReplace(ResourceDictionary owner, ResourceDictionary target, ResourceDictionary replacement)
        {
            for (var i = 0; i < owner.MergedDictionaries.Count; i++)
            {
                if (ReferenceEquals(owner.MergedDictionaries[i], target))
                {
                    owner.MergedDictionaries[i] = replacement;

                    return true;
                }

                if (TryReplace(owner.MergedDictionaries[i], target, replacement))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
