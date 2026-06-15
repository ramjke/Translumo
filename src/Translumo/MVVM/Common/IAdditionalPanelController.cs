using System;

namespace Translumo.MVVM.Common
{
    public interface IAdditionalPanelController
    {
        event EventHandler<bool> PanelStateIsChanged;

        void ClosePanel();
    }
}
