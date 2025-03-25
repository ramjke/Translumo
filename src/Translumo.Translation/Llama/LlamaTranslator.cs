using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Constants;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;
using Translumo.Utils.Http;

namespace Translumo.Translation.Llama
{
    public class LlamaTranslator : BaseTranslator<LlamaContainer>
    {
        private const string LLAMA_API_URL = "http://localhost:11434/api/generate";
        private readonly HttpClient _httpClient;
        private string _modelName;

        public LlamaTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger)
            : base(translationConfiguration, languageService, logger)
        {
            _httpClient = new HttpClient();
            _modelName = translationConfiguration.OllamaModel;
        }

        public override Task<string> TranslateTextAsync(string sourceText)
        {
            //TODO: Temp implementation for specific lang
            if (TargetLangDescriptor.Language == Languages.PortugueseBrazil)
            {
                throw new TransactionException("Llama translate is unavailable for this language");
            }

            return base.TranslateTextAsync(sourceText);
        }



        protected override async Task<string> TranslateTextInternal(LlamaContainer container, string sourceText)
        {
            var requestBody = new
            {
                model = _modelName, // 你可以改成本地安装的模型，如 "llama2"、"mistral"
                prompt = $"Translate this text to {TargetLangDescriptor.Language.ToString()}: {sourceText}",
                stream = false // 设置 stream=false 以同步返回完整翻译结果
            };

            // 序列化 JSON
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            // 发送 HTTP 请求到本地 Llama 服务器
            HttpResponseMessage response = await _httpClient.PostAsync(LLAMA_API_URL, content);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                // 解析 JSON 响应
                using JsonDocument jsonDoc = JsonDocument.Parse(responseBody);
                string translatedText = jsonDoc.RootElement.GetProperty("response").GetString();

                return translatedText;
            }

            throw new TranslationException($"Llama translation failed. Status Code: {response.StatusCode}");
        }

        protected override IList<LlamaContainer> CreateContainers(TranslationConfiguration configuration)
        {
            var result = configuration.ProxySettings.Select(proxy => new LlamaContainer(proxy)).ToList();
            result.Add(new LlamaContainer(isPrimary: true));

            return result;
        }
    }
}
