using System;
using System.Net;
using Translumo.Translation.Configuration;
using Translumo.Utils.Http;

namespace Translumo.Translation.Deepl
{
    public sealed class DeeplContainer : TranslationContainer
    {
        public HttpReader Reader { get; private set; }
        public long DeeplId { get; set; }

        public DeeplContainer(Proxy proxy = null, bool isPrimary = false) : base(proxy, isPrimary)
        {
            Reader = CreateReader(proxy);
            DeeplId = GenerateDeeplId();
        }

        public override void Reset()
        {
            base.Reset();

            DeeplId = GenerateDeeplId();
            Reader.Cookies = new CookieContainer();
        }

        private HttpReader CreateReader(Proxy proxy)
        {
            var deeplReader = new HttpReader();
            deeplReader.ThrowExceptions = false;
            deeplReader.Referer = "https://www.deepl.com/translator";
            deeplReader.ContentType = "application/json";
            deeplReader.Accept = "*/*";
            deeplReader.OptionalHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
            deeplReader.OptionalHeaders.Add("DNT", "1");
            deeplReader.OptionalHeaders.Add("TE", "Trailers");

            deeplReader.Proxy = proxy?.ToWebProxy();

            return deeplReader;
        }

        private long GenerateDeeplId()
        {
            long num = 10000L;
            
            return num * (long)Math.Round((double)num * Random.Shared.NextDouble());
        }
    }
}
