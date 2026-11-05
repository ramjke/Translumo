using System.Net;
using Translumo.Translation.Configuration;
using Translumo.Utils.Http;

namespace Translumo.Translation.DeepSeek
{
    public sealed class DeepSeekContainer : TranslationContainer
    {
        public HttpReader Reader { get; private set; }

        public DeepSeekContainer(bool isPrimary = false) : base(isPrimary)
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
            var deepSeekReader = new HttpReader();
            deepSeekReader.ThrowExceptions = false;
            deepSeekReader.ContentType = "application/json";
            deepSeekReader.Accept = "application/json";
            deepSeekReader.UserAgent = "Translumo";

            return deepSeekReader;
        }
    }
}
