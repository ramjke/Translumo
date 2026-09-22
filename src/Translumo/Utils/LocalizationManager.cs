using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using Translumo.Utils.Extensions;

namespace Translumo.Utils
{
    internal class CallbackContext
    {
        public Action<string, string> Callback { get; set; }

        public object Caller { get; set; }

        public string Value { get; set; }
    }

    public static class LocalizationManager
    {
        public static IEnumerable<CultureInfo> AvailableLocalizations = new[]
        {
            new CultureInfo("en-US"), 
            new CultureInfo("ru-RU"),
            new CultureInfo("zh-CN")
        };

        private const string LocalizationSourcePrefix = "Resources/Localization/lang.";

        private static readonly IDictionary<string, CallbackContext> ChangedValueCallbacks;

        static LocalizationManager()
        {
            ChangedValueCallbacks = new Dictionary<string, CallbackContext>();
        }

        public static CultureInfo GetSystemLocalization()
        {
            var systemCulture = CultureInfo.InstalledUICulture;

            return AvailableLocalizations.FirstOrDefault(lang => lang.Name == systemCulture.Name)
                   ?? AvailableLocalizations.FirstOrDefault(lang => lang.TwoLetterISOLanguageName == systemCulture.TwoLetterISOLanguageName)
                   ?? AvailableLocalizations.First(lang => lang.Name == "en-US");
        }

        public static string GetValue(string key, bool lineBreakReplacement = false, Action<string, string> changeValueCallback = null, object caller = null)
        {
            string value = Application.Current.TryFindResource(key) as string;
            if (lineBreakReplacement)
            {
                value = value?.Replace("&#13;", "\n");
            }

            if (changeValueCallback != null)
            {
                ChangedValueCallbacks[key] = new CallbackContext() { Callback = changeValueCallback, Caller = caller, Value = value };
            }

            return value;
        }


        public static void ChangeAppCulture(CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            var resources = Application.Current?.Resources;
            if (resources == null)
            {
                return;
            }

            var source = $"{LocalizationSourcePrefix}{cultureInfo.Name}.xaml";
            var currentDictionary = ResourceDictionaryHelper.FindBySource(resources, LocalizationSourcePrefix);
            if (currentDictionary == null || currentDictionary.Source?.OriginalString == source)
            {
                return;
            }

            var newDictionary = new ResourceDictionary() { Source = new Uri(source, UriKind.Relative) };
            if (ResourceDictionaryHelper.TryReplace(resources, currentDictionary, newDictionary))
            {
                NotifyChangedValues();
            }
        }

        public static void ReleaseChangedValuesCallbacks(object caller)
        {
            var toRemove = ChangedValueCallbacks.Where(ctx => ctx.Value.Caller == caller).ToArray();
            
            toRemove.ForEach(item => ChangedValueCallbacks.Remove(item));
        }



        private static void NotifyChangedValues()
        {
            foreach (var changedValueCallback in ChangedValueCallbacks)
            {
                changedValueCallback.Value.Callback.Invoke(changedValueCallback.Key, changedValueCallback.Value.Value);
            }
        }
    }
}
