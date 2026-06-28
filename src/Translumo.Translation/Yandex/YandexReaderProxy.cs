using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Translumo.Infrastructure.Constants;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;
using Translumo.Utils.Http;

namespace Translumo.Translation.Yandex
{
    public class YandexReaderProxy
    {
        public readonly string YandexRuUrl = "https://translate.yandex.ru";
        public readonly string YandexApiUrl = "https://translate.yandex.net/api/v1/tr.json/translate";

        public HttpReader HttpReader { get; protected set; }
        public Uri YandexRuUri { get; }


        public YandexReaderProxy(Proxy proxy = null)
        {
            HttpReader = CreateReader(proxy);
            YandexRuUri = new Uri(YandexRuUrl);
        }

        public async Task<string> RequestSidAsync()
        {
            HttpResponse response = await HttpReader.RequestWebDataAsync(YandexRuUrl, HttpMethods.GET, true).ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                throw new HttpRequestException($"Invalid http response: '{response.Body}'");
            }

            return ExtractSid(response.Body);
        }

        public async Task<string> RequestTranslationAsync(YandexRequest request)
        {
            string uri = YandexApiUrl + HttpHelper.BuildQueryString(request.QueryParams);
            HttpResponse response = await HttpReader.RequestWebDataAsync(uri, HttpMethods.POST,
                    HttpHelper.BuildFormData(request.Body), true)
                .ConfigureAwait(false);
            if (response.IsSuccessful)
            {
                var deserialized = JsonSerializer.Deserialize<YandexResponse>(response.Body);
                if (deserialized != null && (deserialized.Text?.Any() ?? false))
                {
                    return Uri.UnescapeDataString(string.Join(" ", deserialized.Text));
                }
            }

            throw new TranslationException($"Failed to translate text: '{response.Body}'");
        }

        public string ExtractSid(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return string.Empty;
            }

            var match = RegexStorage.YandexSidRegex.Match(body);
            if (match.Success)
            {
                return match.Value;
            }

            return string.Empty;
        }

        private HttpReader CreateReader(Proxy proxy)
        {
            var reader = new HttpReader
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36",
                Accept = "*/*",
                Referer = YandexRuUrl,
                Proxy = proxy?.ToWebProxy()
            };

            reader.OptionalHeaders.Add("origin", YandexRuUrl);
            reader.OptionalHeaders.Add("accept-language", "en-US;q=0.8,en;q=0.7");
            reader.OptionalHeaders.Add("DNT", "1");
            reader.OptionalHeaders.Add("Cache-Control", "no-cache");
            reader.OptionalHeaders.Add("Sec-Fetch-Site", "none");
            reader.OptionalHeaders.Add("Sec-Fetch-Mode", "navigate");
            reader.OptionalHeaders.Add("Sec-Fetch-Dest", "document");
            reader.OptionalHeaders.Add("Sec-Fetch-User", "?1");
            reader.OptionalHeaders.Add("Upgrade-Insecure-Requests", "1");
            reader.OptionalHeaders.Add("Sec-Ch-Ua-Platform", "Windows");
            reader.OptionalHeaders.Add("Sec-Ch-Ua", "\"Not/A)Brand\";v=\"99\", \"Microsoft Edge\";v=\"115\", \"Chromium\";v=\"115\"");
            reader.OptionalHeaders.Add("X-Retpath-Y", "https://translate.yandex.ru");

            return reader;
        }
    }
}
