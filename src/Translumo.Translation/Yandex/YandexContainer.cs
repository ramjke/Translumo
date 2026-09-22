using Translumo.Translation.Configuration;
using Translumo.Utils.Http;

namespace Translumo.Translation.Yandex
{
    public sealed class YandexContainer : TranslationContainer
    {
        public HttpReader Reader { get; private set; }

        public YandexContainer(Proxy proxy = null, bool isPrimary = false) : base(proxy, isPrimary)
        {
            Reader = CreateReader(proxy);
        }

        private static HttpReader CreateReader(Proxy proxy)
        {
            return new HttpReader
            {
                ThrowExceptions = false,
                ContentType = "application/json",
                Accept = "*/*",
                UserAgent = "Translumo",
                Proxy = proxy?.ToWebProxy()
            };
        }
    }
}
