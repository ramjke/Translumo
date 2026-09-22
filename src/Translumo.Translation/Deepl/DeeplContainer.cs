using System.Net;
using Translumo.Translation.Configuration;
using Translumo.Utils.Http;

namespace Translumo.Translation.Deepl
{
    public sealed class DeeplContainer : TranslationContainer
    {
        public HttpReader Reader { get; private set; }

        public DeeplContainer(bool isPrimary = false) : base(isPrimary)
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
            var deeplReader = new HttpReader();
            deeplReader.ThrowExceptions = false;
            deeplReader.ContentType = "application/json";
            deeplReader.Accept = "*/*";
            deeplReader.UserAgent = "Translumo";

            return deeplReader;
        }
    }
}
