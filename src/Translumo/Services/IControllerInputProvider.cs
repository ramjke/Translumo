using SharpDX.XInput;
using System;
using Translumo.HotKeys;

namespace Translumo.Services
{
    public interface IControllerInputProvider
    {
        event EventHandler<GamepadKeyPressedEventArgs> KeyDown;
        event EventHandler<GamepadKeyPressedEventArgs> KeyUp;

        void RegisterHotKey(GamepadHotKey hotKey);
        void UnregisterHotKey(GamepadHotKey hotKey);
        void ReassignHotKey(GamepadHotKey hotKey);
    }
}
