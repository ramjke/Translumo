using System.Net;

namespace Translumo.Translation.Configuration
{
    public class Proxy
    {
        public string IpAddress { get; set; }

        public int Port { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public WebProxy ToWebProxy()
        {
            var proxy = new WebProxy(IpAddress, Port);
            if (!string.IsNullOrEmpty(IpAddress) && !string.IsNullOrEmpty(Login))
            {
                proxy.Credentials = new NetworkCredential(Login, Password);
            }

            return proxy;
        }

        public override string ToString()
        {
            return $"{IpAddress}:{Port}";
        }
    }
}
