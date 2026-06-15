using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Translumo.Controls
{
    /// <summary>
    /// Interaction logic for TrayControl.xaml
    /// </summary>
    public partial class TrayControl : UserControl
    {
        public static readonly DependencyProperty SettingsOpeningCommandProperty = DependencyProperty.Register("SettingsOpeningCommand", typeof(ICommand), typeof(TrayControl));
        public static readonly DependencyProperty ChatOpeningCommandProperty = DependencyProperty.Register("ChatOpeningCommand", typeof(ICommand), typeof(TrayControl));

        public ICommand SettingsOpeningCommand
        {
            get { return (ICommand)GetValue(SettingsOpeningCommandProperty); }
            set { SetValue(SettingsOpeningCommandProperty, value); }
        }

        public ICommand ChatOpeningCommand
        {
            get { return (ICommand)GetValue(ChatOpeningCommandProperty); }
            set { SetValue(ChatOpeningCommandProperty, value); }
        }


        public TrayControl()
        {
            InitializeComponent();
        }
    }
}
