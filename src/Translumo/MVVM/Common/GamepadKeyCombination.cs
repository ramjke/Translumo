using SharpDX.XInput;

namespace Translumo.MVVM.Common
{
    public struct GamepadKeyCombination
    {
        public GamepadKeyCode Key;

        public override string ToString()
        {
            return Key.ToString();
        }
    }
}
