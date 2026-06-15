using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Translumo.MVVM.Common;
using Translumo.Translation.Configuration;
using Translumo.Utils;

namespace Translumo.MVVM.Views
{
    /// <summary>
    /// Interaction logic for BrowserView.xaml
    /// </summary>
    public partial class BrowserView : Window
    {
        public static readonly DependencyProperty NotificationCaptionProperty =
            DependencyProperty.Register(
                "NotificationCaption", typeof(string), typeof(BrowserView),
                new FrameworkPropertyMetadata(
                    defaultValue: default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, NotificationCaptionCallback));

        private static void NotificationCaptionCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((string)e.NewValue == (string)e.OldValue)
            {
                return;
            }

            var targetControl = (BrowserView)d;
            targetControl.TbCaption.Text = (string)e.NewValue;
        }

        public string NotificationCaption
        {
            get { return (string)GetValue(NotificationCaptionProperty); }
            set { SetValue(NotificationCaptionProperty, value); }
        }

        public Guid SessionId { get; set; }

        public string TargetPageUrl { get; set; }

        public string SourcePageUrl { get; set; }

        public WebPageInfo TargetPageInfo { get; set; }

        public WebProxy Proxy { get; set; }

        public BrowserView(Guid sessionId)
        {
            InitializeComponent();

            this.SessionId = sessionId;
            Browser.NavigationCompleted += BrowserOnNavigationCompleted;
            Browser.CoreWebView2InitializationCompleted += BrowserOnCoreWebView2InitializationCompleted;
            this.Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Proxy != null)
            {
                var credential = Proxy.Credentials as NetworkCredential;
                var options = new CoreWebView2EnvironmentOptions(additionalBrowserArguments: $"--proxy-server=\"{Proxy.Address.Host}:{Proxy.Address.Port}\"");
                var env = await CoreWebView2Environment.CreateAsync(options: options);
                await Browser.EnsureCoreWebView2Async(env);

                Browser.CoreWebView2.BasicAuthenticationRequested += (sender, args) =>
                {
                    args.Response.UserName = credential.UserName;
                    args.Response.Password = credential.Password;
                };
            }

            Browser.Source = new Uri(SourcePageUrl);
        }

        private void BrowserOnCoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            Browser.CoreWebView2.CookieManager.DeleteAllCookies();
        }

        private async void BrowserOnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var content = await GetInnerHtmlPageAsync();
            if (!string.IsNullOrEmpty(TargetPageUrl) && Browser.Source == new Uri(TargetPageUrl))
            {
                await this.ProcessClose(content);
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.Topmost = true;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Topmost = true;
            this.Activate();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            WindowHelper.RemoveIcon(this);
            if (string.IsNullOrEmpty(NotificationCaption))
            {
                OverlayNotification.Visibility = Visibility.Hidden;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            OverlayNotification.Dispose();
            Browser.Dispose();
        }

        private async Task ProcessClose(string htmlContent)
        {
            TargetPageInfo = new WebPageInfo()
            {
                Body = JsonConvert.DeserializeObject(htmlContent)?.ToString(),
                Cookies = (await Browser.CoreWebView2.CookieManager.GetCookiesAsync(TargetPageUrl)).Select(c => c.ToSystemNetCookie()).ToArray()
            };

            this.Close();
        }

        private async Task<string> GetInnerHtmlPageAsync()
        {
            const int LOAD_DELAY_MS = 400;
            const int LOAD_ATTEMPTS = 7;

            var htmlContent = string.Empty;
            var curAttempt = 1;
            while (htmlContent.Length < 5 && curAttempt <= LOAD_ATTEMPTS)
            {
                htmlContent = await Browser.CoreWebView2.ExecuteScriptAsync("document.body.innerHTML");
                curAttempt++;
                await Task.Delay(LOAD_DELAY_MS);
            }

            return htmlContent;
        }
    }
}
