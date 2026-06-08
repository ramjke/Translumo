using System.Net;
using Translumo.Translation.Configuration;
using Translumo.Utils.Http;

namespace Translumo.Translation.Openrouter
{
    public sealed class OpenrouterContainer : TranslationContainer
    {
        public HttpReader Reader { get; private set; }
        public string ApiKey { get; private set; }

        public OpenrouterContainer(string apiKey, Proxy proxy = null, bool isPrimary = false) : base(proxy, isPrimary)
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
            if (!string.IsNullOrEmpty(ApiKey))
            {
                reader.OptionalHeaders.Add("Authorization", $"Bearer {ApiKey}");
            }
            reader.OptionalHeaders.Add("HTTP-Referer", "https://github.com/Translumo");
            reader.OptionalHeaders.Add("X-Title", "Translumo");
            reader.Proxy = proxy?.ToWebProxy();
            return reader;
        }
    }
}
