using System;
using System.Threading.Tasks;
using Translumo.MVVM.Common;
using Translumo.MVVM.ViewModels;

namespace Translumo.Dialog.Stages
{
    public class ExceptionInteractionStage : InteractionStage
    {
        public Exception InputException { get; set; }

        private readonly Action<Exception> _stageAction;
        private readonly string _errorMessage;
        public ExceptionInteractionStage(DialogService dialogService, Action<Exception> stageAction, string errorMessage)
            : base(dialogService, null)
        {
            _stageAction = stageAction;
            _errorMessage = errorMessage;

        }

        protected override async Task<InteractionStage> ExecuteInner()
        {
            _stageAction?.Invoke(InputException);

            await DialogService.ShowDialogAsync(SimpleDialogViewModel.Create(string.Format(_errorMessage, InputException.Message),
                SimpleDialogTypes.Error));

            return NextStage;
        }
    }
}
