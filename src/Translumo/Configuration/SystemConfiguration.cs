using System.Globalization;
using Translumo.Utils;

namespace Translumo.Configuration
{
    public class SystemConfiguration : BindableBase
    {
        public static SystemConfiguration Default => new SystemConfiguration()
        {
            ApplicationCulture = LocalizationManager.GetSystemLocalization().Name
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
