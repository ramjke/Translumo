using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;
using Translumo.Utils;
using Translumo.Utils.Http;

namespace Translumo.Translation.Papago
{
    public class PapagoReaderProxy
    {
        public HttpReader HttpReader { get; protected set; }

        private const string TRANSLATE_URL = "https://papago.naver.com/apis/n2mt/translate";
        private const string PAPAGO_HOME_URL = "https://papago.naver.com/";

        public PapagoReaderProxy(Proxy proxy = null)
        {
            HttpReader = CreateReader(proxy);
        }

        public async Task<string> RequestTranslationAsync(PapagoRequest request)
        {
            HttpReader.OptionalHeaders["authorization"] = request.AuthorizationHeader;
            HttpReader.OptionalHeaders["timestamp"] = request.TimestampHeader;
            var response = await HttpReader
                .RequestWebDataAsync(TRANSLATE_URL, HttpMethods.POST, HttpHelper.BuildFormData(request.Body), true)
                .ConfigureAwait(false);

            if (response.IsSuccessful)
            {
                var papagoResponse = JsonSerializer.Deserialize<PapagoResponse>(response.Body);
                if (papagoResponse == null)
                {
                    throw new TranslationException($"Unexpected response: '{response.Body}'");
                }

                return papagoResponse.TranslatedText;
            }
            else
            {
                throw new TranslationException($"Bad response by translator: '{response.Body}'");
            }
        }

        public async Task<string> RequestAuthKeyAsync()
        {
            HttpResponse response = await HttpReader.RequestWebDataAsync(PAPAGO_HOME_URL, HttpMethods.GET).ConfigureAwait(true);
            if (!response.IsSuccessful)
            {
                throw new HttpRequestException($"Invalid http response: '{response.Body}'");
            }

            var jsChunkName = ExtractHomeJsChunk(response.Body);
            if (string.IsNullOrEmpty(jsChunkName))
            {
                return jsChunkName;
            }

            response = await HttpReader.RequestWebDataAsync($"{PAPAGO_HOME_URL}/{jsChunkName}", HttpMethods.GET)
                .ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                throw new HttpRequestException($"Invalid http response: '{response.Body}'");
            }

            return ExtractAuthKey(response.Body);
        }


        private string ExtractHomeJsChunk(string textBody)
        {
            const string HOME_JS_REGEX = @"home\.\w+\.chunk\.js";

            return RegexHelper.Match(textBody, HOME_JS_REGEX);
        }

        private string ExtractAuthKey(string textBody)
        {
            var keyGroupName = "Key";
            var authKeyRegex = @$"AUTH_KEY:\s*""(?<{keyGroupName}>[^"" ]+)""";

            return RegexHelper.Match(textBody, authKeyRegex, keyGroupName);
        }

        private HttpReader CreateReader(Proxy proxy)
        {
            var httpReader = new HttpReader();
            httpReader.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:97.0) Gecko/20100101 Firefox/97.0";
            //httpReader.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            //httpReader.Accept = "application/json";
            httpReader.Accept = "*/*";
            httpReader.Proxy = proxy?.ToWebProxy();

            httpReader.OptionalHeaders.Add("Sec-Fetch-Site", "same-origin");
            httpReader.OptionalHeaders.Add("Sec-Fetch-Mode", "cors");
            httpReader.OptionalHeaders.Add("Sec-Fetch-Dest", "empty");
            httpReader.OptionalHeaders.Add("x-apigw-partnerid", "papago");
            httpReader.OptionalHeaders.Add("device-type", "pc");
            httpReader.OptionalHeaders.Add("origin", "https://papago.naver.com");
            httpReader.OptionalHeaders.Add("referer", "https://papago.naver.com/");

            return httpReader;
        }
    }
}
