using System;
using System.Threading.Tasks;
using Translumo.Utils;

namespace Translumo.MVVM.ViewModels
{
    public sealed class WaitingDialogViewModel : BindableBase, INonInteractionDialogViewModel
    {
        public string TextContent
        {
            get => _textContent;
            set => SetProperty(ref _textContent, value);
        }

        public bool IsClosed { get; private set; } = false;

        public event EventHandler DialogIsClosed;

        private string _textContent;

        public WaitingDialogViewModel(Task innerTask, string taskText)
        {
            this.TextContent = taskText;

            innerTask.ContinueWith(t => Close());
        }

        private void Close()
        {
            IsClosed = true;
            DialogIsClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}
