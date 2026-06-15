using System;
using System.Globalization;
using Translumo.Utils;
using MaterialDesignThemes.Wpf;

namespace Translumo.Configuration
{
    public class SystemConfiguration : BindableBase
    {
        public static SystemConfiguration Default => new SystemConfiguration()
        {
            ApplicationCulture = "en-US"
        };

        public string ApplicationCulture
        {
            get => _applicationCulture;
            set
            {
                SetProperty(ref _applicationCulture, value);
                UpdateSelectedLanguage();
            }
        }

        private string _applicationCulture;

        private void UpdateSelectedLanguage()
        {
            LocalizationManager.ChangeAppCulture(new CultureInfo(ApplicationCulture));
        }
    }
}
