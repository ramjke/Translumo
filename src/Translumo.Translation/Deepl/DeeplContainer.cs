using System.Net;
using Translumo.Translation.Configuration;
using Translumo.Utils.Http;

namespace Translumo.Translation.Deepl
{
    public sealed class DeeplContainer : TranslationContainer
    {
        public HttpReader Reader { get; private set; }

        public DeeplContainer(Proxy proxy = null, bool isPrimary = false) : base(proxy, isPrimary)
        {
            Reader = CreateReader(proxy);
        }

        public override void Block()
        {
            base.Block();
            Reader.Cookies = new CookieContainer();
        }

        private HttpReader CreateReader(Proxy proxy)
        {
            var deeplReader = new HttpReader();
            deeplReader.ThrowExceptions = false;
            deeplReader.ContentType = "application/json";
            deeplReader.Accept = "*/*";
            deeplReader.UserAgent = "Translumo";
            deeplReader.Proxy = proxy?.ToWebProxy();

            return deeplReader;
        }
    }
}
