using System;

namespace Translumo.MVVM.ViewModels
{
    public interface INonInteractionDialogViewModel
    {
        bool IsClosed { get; }

        event EventHandler DialogIsClosed;
    }
}
