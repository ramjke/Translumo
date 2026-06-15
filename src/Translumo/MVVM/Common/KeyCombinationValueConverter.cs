using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using Translumo.HotKeys;

namespace Translumo.MVVM.Common
{
    public class KeyCombinationValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hotKeyInfo = value as HotKeyInfo;

            return new KeyCombination { Key = hotKeyInfo.Key, Modifier = (ModifierKeys)hotKeyInfo.KeyModifier };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var keyCombination = (KeyCombination)value;

            return new HotKeyInfo(keyCombination.Key, (KeyModifier)keyCombination.Modifier);
        }
    }
}
