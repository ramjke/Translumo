using SharpDX.XInput;

namespace Translumo.HotKeys
{
    public class GamepadHotKeyInfo
    {
        public GamepadKeyCode Key { get; set; }

        public GamepadHotKeyInfo(GamepadKeyCode key)
        {
            this.Key = key;
        }

        public GamepadHotKeyInfo()
        {
        }

        public override bool Equals(object obj)
        {
            var anotherKeyInfo = obj as GamepadHotKeyInfo;
            if (anotherKeyInfo == null)
            {
                return false;
            }

            return this.Key.Equals(anotherKeyInfo.Key);
        }

        public override string ToString()
        {
            return Key.ToString();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Key.GetHashCode();
                return hash;
            }
        }
    }
}
