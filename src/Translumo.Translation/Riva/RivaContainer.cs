using Translumo.Translation.Configuration;
using Translumo.Utils.Http;

namespace Translumo.Translation.Riva
{
    public sealed class RivaContainer : TranslationContainer
    {
        public HttpReader Reader { get; set; }

        public RivaContainer(Proxy proxy = null, bool isPrimary = false) 
            : base(proxy, isPrimary)
        {
            Reader = CreateReader(proxy);
        }

        private HttpReader CreateReader(Proxy proxy)
        {
            var httpReader = new HttpReader();
            httpReader.Proxy = proxy?.ToWebProxy();

            httpReader.ContentType = "application/json";
            httpReader.UserAgent = "Translumo-Client-Riva";
            httpReader.Accept = "application/json";

            return httpReader;
        }
    }
}
