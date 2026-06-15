using System;
using System.Threading.Tasks;

namespace Translumo.Dialog.Stages
{
    public class ActionInteractionStage : InteractionStage
    {
        private readonly Func<Task> _actionStage;

        public ActionInteractionStage(DialogService dialogService, Func<Task> actionStage, string stageName = null) : base(dialogService, stageName)
        {
            _actionStage = actionStage;
        }

        protected override async Task<InteractionStage> ExecuteInner()
        {
            await _actionStage.Invoke();

            return NextStage;
        }
    }
}
