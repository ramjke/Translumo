using System;
using SharpDX.XInput;

namespace Translumo.HotKeys
{
    public class GamepadKeyPressedEventArgs : EventArgs
    {
        public GamepadKeyCode KeyCode { get; }

        public bool Handled { get; set; }

        public GamepadKeyPressedEventArgs(GamepadKeyCode key)
        {
            this.KeyCode = key;
        }
    }
}
