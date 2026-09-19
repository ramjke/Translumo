using System.Windows.Input;
using SharpDX.XInput;
using Translumo.Utils;

namespace Translumo.HotKeys
{
    public class HotKeysConfiguration : BindableBase
    {
        public static HotKeysConfiguration Default => new HotKeysConfiguration()
        {
            ChatVisibilityKey = new HotKeyInfo(Key.T, KeyModifier.Alt),
            SelectAreaKey = new HotKeyInfo(Key.Q, KeyModifier.Alt),
            SettingVisibilityKey = new HotKeyInfo(Key.G, KeyModifier.Alt),
            TranslationStateKey = new HotKeyInfo(Key.OemTilde, KeyModifier.None),
            ShowSelectionAreaKey = new HotKeyInfo(Key.Y, KeyModifier.Alt),
            OnceTranslateKey = new HotKeyInfo(Key.F, KeyModifier.Shift),
            WindowStyleChangeKey = new HotKeyInfo(Key.T, KeyModifier.Ctrl),

            ChatVisibilityGamepadKey = new GamepadHotKeyInfo(GamepadKeyCode.None),
            SelectAreaGamepadKey = new GamepadHotKeyInfo(GamepadKeyCode.None),
            SettingVisibilityGamepadKey = new GamepadHotKeyInfo(GamepadKeyCode.None),
            TranslationStateGamepadKey = new GamepadHotKeyInfo(GamepadKeyCode.None),
            ShowSelectionAreaGamepadKey = new GamepadHotKeyInfo(GamepadKeyCode.None),
            OnceTranslateGamepadKey = new GamepadHotKeyInfo(GamepadKeyCode.None),
            WindowStyleChangeGamepadKey = new GamepadHotKeyInfo(GamepadKeyCode.None)
        };

        public HotKeyInfo ChatVisibilityKey
        {
            get => _chatVisibilityKey;
            set
            {
                SetProperty(ref _chatVisibilityKey, value);
            }
        }

        public HotKeyInfo TranslationStateKey
        {
            get => _translationStateKey;
            set
            {
                SetProperty(ref _translationStateKey, value);
            }
        }

        public HotKeyInfo SelectAreaKey
        {
            get => _selectAreaKey;
            set
            {
                SetProperty(ref _selectAreaKey, value);
            }
        }

        public HotKeyInfo SettingVisibilityKey
        {
            get => _settingVisibilityKey;
            set 
            {
                SetProperty(ref _settingVisibilityKey, value);
            }
        }

        public HotKeyInfo ShowSelectionAreaKey
        {
            get => _showSelectionAreaKey;
            set
            {
                SetProperty(ref _showSelectionAreaKey, value);
            }
        }

        public HotKeyInfo OnceTranslateKey
        {
            get => _onceTranslateKey;
            set
            {
                SetProperty(ref _onceTranslateKey, value);
            }
        }

        public HotKeyInfo WindowStyleChangeKey
        {
            get => _windowStyleChangeKey;
            set
            {
                SetProperty(ref _windowStyleChangeKey, value);
            }
        }



        public GamepadHotKeyInfo ChatVisibilityGamepadKey
        {
            get => _chatVisibilityGamepadKey;
            set
            {
                SetProperty(ref _chatVisibilityGamepadKey, value);
            }
        }

        public GamepadHotKeyInfo TranslationStateGamepadKey
        {
            get => _translationStateGamepadKey;
            set
            {
                SetProperty(ref _translationStateGamepadKey, value);
            }
        }

        public GamepadHotKeyInfo SelectAreaGamepadKey
        {
            get => _selectAreaGamepadKey;
            set
            {
                SetProperty(ref _selectAreaGamepadKey, value);
            }
        }

        public GamepadHotKeyInfo SettingVisibilityGamepadKey
        {
            get => _settingVisibilityGamepadKey;
            set
            {
                SetProperty(ref _settingVisibilityGamepadKey, value);
            }
        }

        public GamepadHotKeyInfo ShowSelectionAreaGamepadKey
        {
            get => _showSelectionAreaGamepadKey;
            set
            {
                SetProperty(ref _showSelectionAreaGamepadKey, value);
            }
        }

        public GamepadHotKeyInfo OnceTranslateGamepadKey
        {
            get => _onceTranslateGamepadKey;
            set
            {
                SetProperty(ref _onceTranslateGamepadKey, value);
            }
        }

        public GamepadHotKeyInfo WindowStyleChangeGamepadKey
        {
            get => _windowStyleChangeGamepadKey;
            set
            {
                SetProperty(ref _windowStyleChangeGamepadKey, value);
            }
        }

        private HotKeyInfo _chatVisibilityKey;
        private HotKeyInfo _selectAreaKey;
        private HotKeyInfo _translationStateKey;
        private HotKeyInfo _settingVisibilityKey;
        private HotKeyInfo _showSelectionAreaKey;
        private HotKeyInfo _onceTranslateKey;
        private HotKeyInfo _windowStyleChangeKey = new HotKeyInfo(Key.T, KeyModifier.Ctrl);

        private GamepadHotKeyInfo _chatVisibilityGamepadKey;
        private GamepadHotKeyInfo _selectAreaGamepadKey;
        private GamepadHotKeyInfo _translationStateGamepadKey;
        private GamepadHotKeyInfo _settingVisibilityGamepadKey;
        private GamepadHotKeyInfo _showSelectionAreaGamepadKey;
        private GamepadHotKeyInfo _onceTranslateGamepadKey;
        private GamepadHotKeyInfo _windowStyleChangeGamepadKey = new GamepadHotKeyInfo(GamepadKeyCode.None);
    }
}
