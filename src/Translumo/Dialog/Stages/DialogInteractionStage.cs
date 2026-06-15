using Translumo.MVVM.Common;
using Translumo.MVVM.ViewModels;

namespace Translumo.Dialog.Stages
{
    public class DialogInteractionStage : ActionInteractionStage
    {
        public DialogInteractionStage(DialogService dialogService, string dialogMessage) : base(dialogService,
            () => dialogService.ShowDialogAsync(SimpleDialogViewModel.Create(dialogMessage, SimpleDialogTypes.Info)))
        {
        }
    }
}
