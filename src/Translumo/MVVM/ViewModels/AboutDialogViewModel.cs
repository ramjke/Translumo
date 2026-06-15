using Translumo.Infrastructure.Constants;

namespace Translumo.MVVM.ViewModels
{
    public class AboutDialogViewModel
    {
        public string ApplicationVersion { get; set; } = "v" + Global.GetVersion();

        public bool HasUpdates { get; set; }

        public AboutDialogViewModel()
        {
        }
    }
}
