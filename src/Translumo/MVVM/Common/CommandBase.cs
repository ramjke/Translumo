using System;
using System.Windows.Input;
using System.Windows.Markup;

namespace Translumo.MVVM.Common
{
    public abstract class CommandBase<TCommand> : MarkupExtension, ICommand
        where TCommand : class, ICommand, new()
    {
        protected static TCommand Command;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Command == null)
            {
                Command = new TCommand();
            }

            return Command;
        }

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);

    }
}
