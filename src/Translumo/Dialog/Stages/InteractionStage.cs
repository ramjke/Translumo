using System;
using System.Threading.Tasks;
using Translumo.MVVM.ViewModels;

namespace Translumo.Dialog.Stages
{
    public abstract class InteractionStage
    {
        public InteractionStage NextStage { get; protected set; }
        public ExceptionInteractionStage ExceptionStage { get; private set; }

        protected string StageName;
        protected readonly DialogService DialogService;

        protected InteractionStage(DialogService dialogService, string stageName)
        {
            StageName = stageName;
            DialogService = dialogService;
        }

        public InteractionStage AddNextStage(InteractionStage nextStage)
        {
            NextStage = nextStage;

            return this;
        }

        public InteractionStage AddException(ExceptionInteractionStage exceptionStage)
        {
            ExceptionStage = exceptionStage;

            return this;
        }

        private ExceptionInteractionStage TryGetExceptionStage(Exception ex)
        {
            if (ExceptionStage == null)
            {
                throw ex;
            }

            ExceptionStage.InputException = ex;

            return ExceptionStage;
        }

        public virtual async Task ExecuteAsync()
        {
            InteractionStage nextStage;
            try
            {
                var task = ExecuteInner();
                if (!string.IsNullOrEmpty(StageName))
                {
                    if (!task.IsCompleted)
                    {
                        await DialogService.ShowDialogAsync(new WaitingDialogViewModel(task, StageName));
                    }
                }

                nextStage = await task;
                if (task.Exception != null)
                {
                    nextStage = TryGetExceptionStage(task.Exception.InnerException ?? task.Exception);
                }
            }
            catch (AggregateException ex)
            {
                nextStage = TryGetExceptionStage(ex.InnerException ?? ex);
            }
            catch (Exception ex)
            {
                nextStage = TryGetExceptionStage(ex);
            }

            if (nextStage != null)
            {
                await nextStage.ExecuteAsync();
            }
        }

        protected abstract Task<InteractionStage> ExecuteInner();

    }
}
