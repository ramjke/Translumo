using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using Translumo.HotKeys;

namespace Translumo.Services
{
    public class ControllerInputProvider : IControllerInputProvider
    {
        public event EventHandler<GamepadKeyPressedEventArgs> KeyDown;
        public event EventHandler<GamepadKeyPressedEventArgs> KeyUp;

        private readonly IDictionary<int, GamepadHotKey> _registeredHotKeys;

        public ControllerInputProvider(ObservablePipe<Keystroke> inputPipe)
        {
            _registeredHotKeys = new Dictionary<int, GamepadHotKey>();
            inputPipe.ItemHasArrived += InputPipeOnItemHasArrived;
        }

        private void InputPipeOnItemHasArrived(object sender, Keystroke e)
        {
            if (e.Flags == KeyStrokeFlags.KeyDown)
            {
                var args = new GamepadKeyPressedEventArgs(e.VirtualKey);
                KeyDown?.Invoke(this, args);
                if (args.Handled)
                {
                    return;
                }

                var id = (int)e.VirtualKey;
                if (_registeredHotKeys.ContainsKey(id))
                {
                    _registeredHotKeys[id].Action.Invoke();
                }
            }

            if (e.Flags == KeyStrokeFlags.KeyUp)
            {
                var args = new GamepadKeyPressedEventArgs(e.VirtualKey);
                KeyUp?.Invoke(this, args);
                if (args.Handled)
                {
                    return;
                }
            }
        }

        public void RegisterHotKey(GamepadHotKey hotKey)
        {
            if (hotKey.KeyCode != GamepadKeyCode.None)
            {
                if (_registeredHotKeys.ContainsKey(hotKey.Id))
                {
                    if (object.ReferenceEquals(_registeredHotKeys[hotKey.Id], hotKey))
                    {
                        return;
                    }

                    throw new ArgumentException($"Gamepad hotkey with id={hotKey.Id} already has been registered");
                }

                _registeredHotKeys.Add(hotKey.Id, hotKey);
            }
        }

        public void UnregisterHotKey(GamepadHotKey hotKey)
        {
            if (_registeredHotKeys.ContainsKey(hotKey.Id))
            {
                _registeredHotKeys.Remove(hotKey.Id);
            }
        }

        public void ReassignHotKey(GamepadHotKey hotKey)
        {
            var registered = _registeredHotKeys.FirstOrDefault(k => object.ReferenceEquals(k.Value, hotKey));
            if (!registered.Equals(default))
            {
                _registeredHotKeys.Remove(registered);
            }

            RegisterHotKey(hotKey);
        }
    }
}
