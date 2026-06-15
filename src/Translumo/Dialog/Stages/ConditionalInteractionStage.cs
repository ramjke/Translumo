using System;
using System.Threading.Tasks;

namespace Translumo.Dialog.Stages
{
    public class ConditionalInteractionStage : InteractionStage
    {
        public InteractionStage NextFalseStage { get; private set; }

        private readonly Func<Task<bool>> _stageFunc;

        public ConditionalInteractionStage(DialogService dialogService, Func<Task<bool>> stageFunc, string stageName = null)
            : base(dialogService, stageName)
        {
            _stageFunc = stageFunc;
        }

        public InteractionStage AddNextFalse(InteractionStage nextStage)
        {
            NextFalseStage = nextStage;

            return this;
        }

        protected override async Task<InteractionStage> ExecuteInner()
        {
            bool funcResult = await _stageFunc.Invoke();

            if (funcResult)
            {
                return NextStage;
            }

            return NextFalseStage;
        }
    }
}
