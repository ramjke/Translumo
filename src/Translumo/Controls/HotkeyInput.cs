using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SharpDX.XInput;
using Translumo.HotKeys;
using Translumo.MVVM.Common;
using Translumo.Utils;

namespace Translumo.Controls
{
    internal class HotkeyInput : TextBox
    {
        public static readonly DependencyProperty HotKeyProperty = DependencyProperty.Register(nameof(HotKey), typeof(KeyCombination), typeof(HotkeyInput), new PropertyMetadata(OnHotKeyChanged));
        public static readonly DependencyProperty GamepadHotKeyProperty = DependencyProperty.Register(nameof(GamepadHotKey), typeof(GamepadKeyCombination), typeof(HotkeyInput), new PropertyMetadata(OnGamepadHotKeyChanged));

        public KeyCombination HotKey
        {
            get => (KeyCombination)GetValue(HotKeyProperty);
            set => SetCurrentValue(HotKeyProperty, value);
        }

        public GamepadKeyCombination GamepadHotKey
        {
            get => (GamepadKeyCombination)GetValue(GamepadHotKeyProperty);
            set => SetCurrentValue(GamepadHotKeyProperty, value);
        }

        private Key? _pressedKey;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PreviewKeyUp += OnPreviewKeyUp;
            PreviewKeyDown += OnPreviewKeyDown;
            GotFocus += OnGotFocus;
            LostFocus += OnLostFocus;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat)
            {
                return;
            }

            var key = _pressedKey ?? e.GetActualKey();
            if (GamepadHotKey.Key != GamepadKeyCode.None)
            {
                GamepadHotKey = new GamepadKeyCombination() { Key = GamepadKeyCode.None };
            }

            HotKey = new KeyCombination { Key = key, Modifier = e.KeyboardDevice.Modifiers };
            e.Handled = true;
            _pressedKey = (int)e.Key > 115 ? null : e.GetActualKey();
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            _pressedKey = null;
            this.Text = GamepadHotKey.Key != GamepadKeyCode.None ? GamepadHotKey.ToString() : HotKey.ToString();

            this.UnsubscribeControllerKeyDown(OnControllerKeyDown);
            this.UnsubscribeControllerKeyUp(OnControllerKeyUp);
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            this.Text = "...";

            this.SubscribeControllerKeyDown(OnControllerKeyDown);
            this.SubscribeControllerKeyUp(OnControllerKeyUp);
        }

        private void OnControllerKeyDown(object arg1, GamepadKeyPressedEventArgs arg)
        {
            GamepadHotKey = new GamepadKeyCombination() { Key = arg.KeyCode };
            arg.Handled = true;
        }

        private void OnControllerKeyUp(object arg1, GamepadKeyPressedEventArgs arg)
        {
            LoseFocus();
            arg.Handled = true;
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            LoseFocus();
        }

        private static void OnHotKeyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is HotkeyInput input)
            {
                input.Text = input.GamepadHotKey.Key != GamepadKeyCode.None
                    ? input.GamepadHotKey.ToString()
                    : ((KeyCombination)args.NewValue).ToString();
            }
        }

        private static void OnGamepadHotKeyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is HotkeyInput input)
            {
                var key = ((GamepadKeyCombination)args.NewValue);
                if (key.Key != GamepadKeyCode.None)
                {
                    input.Text = ((GamepadKeyCombination)args.NewValue).ToString();
                }
            }
        }

        private void LoseFocus()
        {
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this), null);
            Keyboard.ClearFocus();
        }
    }
}
