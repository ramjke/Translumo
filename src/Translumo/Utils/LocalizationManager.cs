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

        private static readonly IDictionary<string, CallbackContext> ChangedValueCallbacks;

        static LocalizationManager()
        {
            Thread.CurrentThread.CurrentUICulture = AvailableLocalizations.First(lang => lang.Name == "en-US");
            ChangedValueCallbacks = new Dictionary<string, CallbackContext>();
        }


        public static string GetValue(string key, bool lineBreakReplacement = false, Action<string, string> changeValueCallback = null, object caller = null)
        {
            string value = null;
            if (lineBreakReplacement)
            {
                value = (Application.Current.TryFindResource(key) as string)?.Replace("&#13;", "\n");
            }

            value = Application.Current.TryFindResource(key) as string;
            if (changeValueCallback != null)
            {
                ChangedValueCallbacks[key] = new CallbackContext() { Callback = changeValueCallback, Caller = caller, Value = value };
            }

            return value;
        }


        public static void ChangeAppCulture(CultureInfo cultureInfo)
        {
            if (Thread.CurrentThread.CurrentUICulture.Equals(cultureInfo))
            {
                return;
            }

            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri(string.Format("Resources/Localization/lang.{0}.xaml", cultureInfo.Name),
                UriKind.Relative);

            var oldDictionary =
                Application.Current.Resources.MergedDictionaries.FirstOrDefault(dict =>
                    dict.Source?.OriginalString.Contains("Localization/lang.") ?? false);
            if (oldDictionary != null)
            {
                int index = Application.Current.Resources.MergedDictionaries.IndexOf(oldDictionary);
                Application.Current.Resources.MergedDictionaries.Remove(oldDictionary);
                Application.Current.Resources.MergedDictionaries.Insert(index, resourceDictionary);
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }

            NotifyChangedValues();
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
