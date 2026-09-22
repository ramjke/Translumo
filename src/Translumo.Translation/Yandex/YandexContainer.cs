using Translumo.Translation.Configuration;
using Translumo.Utils.Http;

namespace Translumo.Translation.Yandex
{
    public sealed class YandexContainer : TranslationContainer
    {
        public HttpReader Reader { get; private set; }

        public YandexContainer(bool isPrimary = false) : base(isPrimary)
        {
            Reader = CreateReader();
        }

        private static HttpReader CreateReader()
        {
            return new HttpReader
            {
                ThrowExceptions = false,
                ContentType = "application/json",
                Accept = "*/*",
                UserAgent = "Translumo"
            };
        }
    }
}
