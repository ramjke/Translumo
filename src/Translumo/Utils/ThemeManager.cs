using System;
using System.Windows;
using Microsoft.Win32;

namespace Translumo.Utils
{
    public enum AppTheme
    {
        Light,
        Dark
    }

    public static class ThemeManager
    {
        private const string PersonalizeKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string AppsUseLightThemeValue = "AppsUseLightTheme";

        public static AppTheme GetSystemTheme()
        {
            using var personalizeKey = Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath);

            return personalizeKey?.GetValue(AppsUseLightThemeValue) is int useLightTheme && useLightTheme == 0
                ? AppTheme.Dark
                : AppTheme.Light;
        }

        public static void StartFollowingSystemTheme()
        {
            ApplySystemTheme();
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        public static void StopFollowingSystemTheme()
        {
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        }

        public static void ApplySystemTheme()
        {
            ChangeAppTheme(GetSystemTheme());
        }

        public static void ChangeAppTheme(AppTheme theme)
        {
            var resources = Application.Current?.Resources;
            if (resources == null)
            {
                return;
            }

            var source = GetThemeSource(theme);
            var currentDictionary = ResourceDictionaryHelper.FindBySource(resources, GetThemeSource(AppTheme.Light))
                                    ?? ResourceDictionaryHelper.FindBySource(resources, GetThemeSource(AppTheme.Dark));
            if (currentDictionary == null || currentDictionary.Source?.OriginalString == source)
            {
                return;
            }

            var newDictionary = new ResourceDictionary() { Source = new Uri(source, UriKind.Relative) };
            ResourceDictionaryHelper.TryReplace(resources, currentDictionary, newDictionary);
        }

        private static string GetThemeSource(AppTheme theme)
        {
            return $"/Themes/{theme}.xaml";
        }

        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category != UserPreferenceCategory.General)
            {
                return;
            }

            Application.Current?.Dispatcher.Invoke(ApplySystemTheme);
        }
    }
}
