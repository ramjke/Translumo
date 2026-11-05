using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;
using Translumo.Utils.Http;

namespace Translumo.Translation.DeepSeek
{
    public sealed class DeepSeekTranslator : BaseTranslator<DeepSeekContainer>
    {
        private const string BASE_URL = "https://api.deepseek.com";
        private const string CHAT_COMPLETIONS_URL = BASE_URL + "/chat/completions";
        private const string MODEL_NAME = "deepseek-chat";

        // 'deepseek-chat' is a reasoning model: 'reasoning_effort' must not be sent,
        // and temperature has to stay within [0, 2] (never negative).
        private const double TEMPERATURE = 1.0;

        private const string SYSTEM_PROMPT =
            "You are a translation engine. Translate the user's message from {0} to {1}. " +
            "Output only the translated text: no explanations, no notes, no quotation marks, no extra content.";

        public DeepSeekTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger)
            : base(translationConfiguration, languageService, logger)
        {
        }

        public override Task<string> TranslateTextAsync(string sourceText)
        {
            if (string.IsNullOrWhiteSpace(TranslationConfiguration.DeepSeekApiKey))
            {
                const string emptyKeyMessage =
                    "未设置 DeepSeek API 密钥，请在「语言」设置页的「翻译源」中选择 DeepSeek，并在「DeepSeek API 密钥」输入框中填写密钥后重试";

                Fail(emptyKeyMessage);

                throw new TranslationException(emptyKeyMessage);
            }

            return base.TranslateTextAsync(sourceText);
        }

        protected override async Task<string> TranslateTextInternal(DeepSeekContainer container, string sourceText)
        {
            var apiKey = TranslationConfiguration.DeepSeekApiKey.Trim();
            var request = CreateRequest(sourceText);
            container.Reader.OptionalHeaders["Authorization"] = $"Bearer {apiKey}";

            var response = await container.Reader
                .RequestWebDataAsync(CHAT_COMPLETIONS_URL, HttpMethods.POST, JsonSerializer.Serialize(request))
                .ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                var failureMessage = $"DeepSeek 翻译失败：{DescribeFailure(response)}";

                Fail(failureMessage, response.InnerException);

                throw new TranslationException(failureMessage, response.InnerException);
            }

            try
            {
                var parsedResponse = JsonSerializer.Deserialize<DeepSeekResponse>(response.Body);
                if (parsedResponse?.Error != null)
                {
                    var apiError = $"DeepSeek 返回错误：{parsedResponse.Error.Message ?? parsedResponse.Error.Code}";

                    Fail(apiError);

                    throw new TranslationException(apiError);
                }

                var translated = parsedResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
                if (string.IsNullOrEmpty(translated))
                {
                    Fail($"DeepSeek 返回了意外的响应：'{response.Body}'");

                    throw new TranslationException($"DeepSeek 返回了意外的响应：'{response.Body}'");
                }

                return translated;
            }
            catch (JsonException ex)
            {
                Fail($"无法解析 DeepSeek 响应：'{response.Body}'", ex);

                throw new TranslationException($"无法解析 DeepSeek 响应：'{response.Body}'", ex);
            }
        }

        protected override IList<DeepSeekContainer> CreateContainers(TranslationConfiguration configuration)
        {
            return new List<DeepSeekContainer> { new DeepSeekContainer(isPrimary: true) };
        }

        private DeepSeekRequest CreateRequest(string sourceText)
        {
            var systemPrompt = string.Format(SYSTEM_PROMPT, GetLanguageName(SourceLangDescriptor), GetLanguageName(TargetLangDescriptor));

            var messages = new List<DeepSeekChatMessage>
            {
                new DeepSeekChatMessage("system", systemPrompt),
                new DeepSeekChatMessage("user", sourceText)
            };

            return new DeepSeekRequest(messages, MODEL_NAME, TEMPERATURE);
        }

        private static string GetLanguageName(LanguageDescriptor languageDescriptor)
        {
            return $"{languageDescriptor.Language} ({languageDescriptor.IsoCode})";
        }

        private void Fail(string message, Exception exception = null)
        {
            if (exception == null)
            {
                Logger.LogError(message);
            }
            else
            {
                Logger.LogError(exception, message);
            }

            try
            {
                Console.WriteLine($"[Translumo][DeepSeek] {message}");
            }
            catch (Exception)
            {
                // console output is optional
            }
        }

        private static string DescribeFailure(HttpResponse response)
        {
            if (response.InnerException is WebException webException && webException.Response is HttpWebResponse httpResponse)
            {
                var apiMessage = TryExtractApiMessage(ReadErrorBody(httpResponse));

                switch ((int)httpResponse.StatusCode)
                {
                    case 401:
                        return $"API 密钥无效或被拒绝（HTTP 401，请检查 DeepSeek API 密钥是否正确）{Append(apiMessage)}";
                    case 402:
                        return $"DeepSeek 账户余额不足（HTTP 402，请充值后重试）{Append(apiMessage)}";
                    case 404:
                        return $"模型 {MODEL_NAME} 不可用或接口地址不存在（HTTP 404）{Append(apiMessage)}";
                    case 429:
                        return $"请求过于频繁，已被限流（HTTP 429，请稍后再试）{Append(apiMessage)}";
                    default:
                        return $"服务返回 HTTP {(int)httpResponse.StatusCode}{Append(apiMessage)}";
                }
            }

            return response.InnerException?.Message ?? response.Body ?? "空响应";
        }

        private static string Append(string apiMessage)
        {
            return string.IsNullOrWhiteSpace(apiMessage) ? string.Empty : $"，接口返回：{apiMessage}";
        }

        private static string ReadErrorBody(WebResponse webResponse)
        {
            try
            {
                using var stream = webResponse.GetResponseStream();
                if (stream == null)
                {
                    return null;
                }

                using var reader = new StreamReader(stream, Encoding.UTF8);

                return reader.ReadToEnd();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string TryExtractApiMessage(string errorBody)
        {
            if (string.IsNullOrWhiteSpace(errorBody))
            {
                return null;
            }

            try
            {
                using var jsonDocument = JsonDocument.Parse(errorBody);
                if (jsonDocument.RootElement.TryGetProperty("error", out var errorElement)
                    && errorElement.TryGetProperty("message", out var messageElement))
                {
                    return messageElement.GetString();
                }

                if (jsonDocument.RootElement.TryGetProperty("message", out var rootMessageElement))
                {
                    return rootMessageElement.GetString();
                }

                return errorBody.Length > 200 ? errorBody.Substring(0, 200) : errorBody;
            }
            catch (JsonException)
            {
                return errorBody.Length > 200 ? errorBody.Substring(0, 200) : errorBody;
            }
        }
    }
}
