using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Translumo.Controls
{
    /// <summary>
    /// Interaction logic for ProxySettingCard.xaml
    /// </summary>
    public partial class ProxySettingCard : UserControl
    {
        public static readonly DependencyProperty DeleteCommandProperty = DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(ProxySettingCard));

        public static readonly DependencyProperty IpAddressProperty =
            DependencyProperty.Register(
                "IpAddress", typeof(string), typeof(ProxySettingCard),
                new FrameworkPropertyMetadata(
                    defaultValue: default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IpAddressCallback));

        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register(
                "Port", typeof(string), typeof(ProxySettingCard),
                new FrameworkPropertyMetadata(
                    defaultValue: default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PortCallback));

        public static readonly DependencyProperty LoginProperty =
            DependencyProperty.Register(
                "Login", typeof(string), typeof(ProxySettingCard),
                new FrameworkPropertyMetadata(
                    defaultValue: default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, LoginCallback));

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(
                "Password", typeof(string), typeof(ProxySettingCard),
                new FrameworkPropertyMetadata(
                    defaultValue: default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PasswordCallback));

        private static void IpAddressCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var targetControl = (ProxySettingCard)d;
            if (targetControl.TbIpAddress.Text != e.NewValue as string)
            {
                targetControl.TbIpAddress.Text = e.NewValue as string ?? string.Empty;
            }
        }

        private static void PortCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var targetControl = (ProxySettingCard)d;
            if (targetControl.TbPort.Text != e.NewValue as string)
            {
                targetControl.TbPort.Text = e.NewValue as string ?? string.Empty;
            }
        }

        private static void LoginCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var targetControl = (ProxySettingCard)d;
            if (targetControl.TbLogin.Text != e.NewValue as string)
            {
                targetControl.TbLogin.Text = e.NewValue as string ?? string.Empty;
            }
        }

        private static void PasswordCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var targetControl = (ProxySettingCard)d;
            if (targetControl.TbPassword.Text != e.NewValue as string)
            {
                targetControl.TbPassword.Text = e.NewValue as string ?? string.Empty;
            }
        }

        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }

        public string IpAddress
        {
            get { return (string)GetValue(IpAddressProperty); }
            set { SetValue(IpAddressProperty, value); }
        }

        public string Port
        {
            get { return (string)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }

        public string Login
        {
            get { return (string)GetValue(LoginProperty); }
            set { SetValue(LoginProperty, value); }
        }

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public ProxySettingCard()
        {
            InitializeComponent();
        }

        private void TbIpAddress_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits and dots for IP Address
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9.]+$");
        }

        private void TbPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits for Port
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void TbIpAddress_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                // Only allow pasting if it strictly contains digits and dots
                if (!Regex.IsMatch(text, "^[0-9.]+$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void TbPort_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                // Only allow pasting if it strictly contains digits
                if (!Regex.IsMatch(text, "^[0-9]+$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void TbPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newValue = (sender as TextBox)?.Text;
            if (Port != newValue)
            {
                Port = newValue;
            }
        }

        private void TbIpAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newValue = (sender as TextBox)?.Text;
            if (IpAddress != newValue)
            {
                IpAddress = newValue;
            }
        }

        private void TbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newValue = (sender as TextBox)?.Text;
            if (Login != newValue)
            {
                Login = newValue;
            }
        }

        private void TbPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            string newValue = (sender as TextBox)?.Text;
            if (Password != newValue)
            {
                Password = newValue;
            }
        }
    }
}
