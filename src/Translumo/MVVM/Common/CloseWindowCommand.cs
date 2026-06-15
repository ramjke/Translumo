using System.Windows;

namespace Translumo.MVVM.Common
{
    public class CloseWindowCommand : CommandBase<CloseWindowCommand>
    {
        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }
    }
}
