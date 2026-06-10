using System;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using Translumo.Utils;

namespace Translumo.MVVM.Common
{
    public class ProxyCardItem : BindableBase
    {
        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }

        public string Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string TestStatus
        {
            get => _testStatus;
            set => SetProperty(ref _testStatus, value);
        }

        public string TestStatusColor
        {
            get => _testStatusColor;
            set => SetProperty(ref _testStatusColor, value);
        }

        public bool IsTesting
        {
            get => _isTesting;
            set
            {
                SetProperty(ref _isTesting, value);
                OnPropertyChanged(nameof(CanTest));
            }
        }

        public bool CanTest => !_isTesting;

        public ICommand TestProxyCommand => new AsyncRelayCommand(TestProxyConnectionAsync);

        private string _ipAddress;
        private string _port;
        private string _login;
        private string _password;
        private string _testStatus = "Not tested";
        private string _testStatusColor = "Gray";
        private bool _isTesting;

        private static readonly Regex IpRegex = new Regex(
            @"^((25[0-5]|2[0-4][0-9]|[0-1]?[0-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|[0-1]?[0-9]?[0-9])$",
            RegexOptions.Compiled);

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(IpAddress) || string.IsNullOrEmpty(Port))
            {
                return false;
            }

            var cleanIp = IpAddress.Trim();
            var cleanPort = Port.Trim();

            if (!IpRegex.IsMatch(cleanIp))
            {
                return false;
            }

            if (!int.TryParse(cleanPort, out int portVal) || portVal < 1 || portVal > 65535)
            {
                return false;
            }

            return true;
        }

        private async Task TestProxyConnectionAsync()
        {
            if (IsTesting)
            {
                return;
            }

            if (!IsValid())
            {
                TestStatus = "Invalid";
                TestStatusColor = "Red";
                return;
            }

            IsTesting = true;
            TestStatus = "Testing...";
            TestStatusColor = "Orange";

            try
            {
                var cleanIp = IpAddress.Trim();
                var portVal = int.Parse(Port.Trim());

                var webProxy = new WebProxy(cleanIp, portVal);
                if (!string.IsNullOrEmpty(Login))
                {
                    webProxy.Credentials = new NetworkCredential(Login, Password);
                }

                var handler = new System.Net.Http.HttpClientHandler
                {
                    Proxy = webProxy,
                    UseProxy = true
                };

                using (var client = new System.Net.Http.HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    
                    var stopwatch = Stopwatch.StartNew();
                    var response = await client.GetAsync("https://www.google.com");
                    stopwatch.Stop();
                    
                    if (response.IsSuccessStatusCode)
                    {
                        TestStatus = $"{stopwatch.ElapsedMilliseconds} ms";
                        TestStatusColor = "Green";
                    }
                    else
                    {
                        TestStatus = $"HTTP {(int)response.StatusCode}";
                        TestStatusColor = "Red";
                    }
                }
            }
            catch (Exception)
            {
                TestStatus = "Offline";
                TestStatusColor = "Red";
            }
            finally
            {
                IsTesting = false;
            }
        }
    }
}
