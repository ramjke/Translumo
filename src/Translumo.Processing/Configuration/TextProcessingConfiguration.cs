using Translumo.Utils;

namespace Translumo.Processing.Configuration
{
    public class TextProcessingConfiguration : BindableBase
    {
        public static TextProcessingConfiguration Default => new TextProcessingConfiguration()
        {
            KeepFormatting = false,
            AutoClearTexts = false,
            AutoClearTextsDelayMs = 1000
        };

        public bool KeepFormatting
        {
            get => _keepFormatting;
            set
            {
                SetProperty(ref _keepFormatting, value);
            }
        }

        public bool AutoClearTexts
        {
            get => _autoClearTexts;
            set
            {
                SetProperty(ref _autoClearTexts, value);
            }
        }

        public uint AutoClearTextsDelayMs
        {
            get => _autoClearTextsDelayMs;
            set
            {
                SetProperty(ref _autoClearTextsDelayMs, value);
            }
        }

        private bool _keepFormatting;
        private bool _autoClearTexts;
        private uint _autoClearTextsDelayMs = 1000;
    }
}
