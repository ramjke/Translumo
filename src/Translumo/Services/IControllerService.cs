using SharpDX.XInput;
using System;
using Translumo.HotKeys;

namespace Translumo.Services
{
    public interface IControllerService
    {
        ObservablePipe<Keystroke> EventPipe { get; }

        bool IsListening { get; }

        bool TryChangeListenState(bool enabled);
    }
}
