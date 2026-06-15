using System.Windows.Input;

namespace Translumo.HotKeys
{
    public class HotKeyInfo
    {
        public Key Key { get; set; }

        public KeyModifier KeyModifier { get; set; }

        public HotKeyInfo(Key key, KeyModifier keyModifier)
        {
            Key = key;
            KeyModifier = keyModifier;
        }

        public HotKeyInfo()
        {
        }

        public override bool Equals(object obj)
        {
            var anotherHotKey = obj as HotKeyInfo;
            if (anotherHotKey == null)
            {
                return false;
            }

            return this.Key == anotherHotKey.Key && this.KeyModifier == anotherHotKey.KeyModifier;
        }

        public override string ToString()
        {
            var keyStr = Key == Key.Oem3 ? "~" : Key.ToString();
            if (KeyModifier == KeyModifier.None)
            {
                return keyStr;
            }

            return KeyModifier.ToString() + "+" + keyStr;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Key.GetHashCode();
                hash = hash * 23 + KeyModifier.GetHashCode();
                return hash;
            }
        }
    }
}
