using System.Net;
using Translumo.Translation.Configuration;
using Translumo.Utils.Http;

namespace Translumo.Translation.Google
{
    public sealed class GoogleContainer : TranslationContainer
    {
        public HttpReader Reader { get; set; }

        public GoogleContainer(bool isPrimary = false) : base(isPrimary)
        {
            Reader = CreateReader();
        }

        public override void Block()
        {
            base.Block();
            Reader.Cookies = new CookieContainer();
        }

        private HttpReader CreateReader()
        {
            var httpReader = new HttpReader();
            httpReader.ThrowExceptions = false;

            httpReader.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpReader.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
            httpReader.Accept = "*/*";

            httpReader.OptionalHeaders.Add("Accept-Language", "en-US;q=0.8,en;q=0.7");
            httpReader.OptionalHeaders.Add("Cache-Control", "no-cache");
            httpReader.OptionalHeaders.Add("DNT", "1");

            return httpReader;
        }
    }
}
