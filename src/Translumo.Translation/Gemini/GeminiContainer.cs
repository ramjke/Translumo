using System.Net;
using Translumo.Translation.Configuration;
using Translumo.Utils.Http;

namespace Translumo.Translation.Gemini
{
    public sealed class GeminiContainer : TranslationContainer
    {
        public HttpReader Reader { get; private set; }
        public string ApiKey { get; private set; }

        public GeminiContainer(string apiKey, Proxy proxy = null, bool isPrimary = false) : base(proxy, isPrimary)
        {
            ApiKey = apiKey;
            Reader = CreateReader(proxy);
        }

        public override void Reset()
        {
            base.Reset();
            Reader.Cookies = new CookieContainer();
        }

        private HttpReader CreateReader(Proxy proxy)
        {
            var reader = new HttpReader();
            reader.ContentType = "application/json";
            reader.Accept = "application/json";
            reader.Proxy = proxy?.ToWebProxy();
            return reader;
        }
    }
}
