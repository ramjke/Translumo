using System;
using System.Globalization;
using System.Windows.Data;
using SharpDX.XInput;
using Translumo.HotKeys;

namespace Translumo.MVVM.Common
{
    public class GamepadKeyCombinationValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hotKeyInfo = value as GamepadHotKeyInfo;
            if (hotKeyInfo == null)
            {
                return new GamepadKeyCombination() { Key = GamepadKeyCode.None };
            }

            return new GamepadKeyCombination() { Key = hotKeyInfo.Key };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var keyCombination = (GamepadKeyCombination)value;

            return new GamepadHotKeyInfo(keyCombination.Key);
        }
    }
}
