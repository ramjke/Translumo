using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Translumo.Infrastructure.Constants;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Translumo.Translation.Deepl
{
    public class DeepLRequest
    {
        public class DeepLTranslatorRequest
        {
            [JsonPropertyName("jsonrpc")]
            public string Jsonrpc { get; set; }
            [JsonPropertyName("method")]
            public string Method { get; set; }
            [JsonPropertyName("params")]
            public Parameters Params { get; set; }
            [JsonPropertyName("id")]
            public long Id { get; set; }

            public DeepLTranslatorRequest(long id, string sentence, string sourceLanguage, string targetLanguage, string regionalVariantCode)
            {
                this.Id = id;
                this.Jsonrpc = "2.0";
                this.Method = "LMT_handle_jobs";
                
                var regexResult = RegexStorage.DeeplSentenceRegex.Matches(sentence);
                var jobs = new List<Job>(regexResult.Count);
                for (int i = 0; i < regexResult.Count; i++)
                {
                    string prevValue = i > 0 ? regexResult[i - 1].Value : null;
                    string nextValue = i < regexResult.Count - 1 ? regexResult[i + 1].Value : null;

                    jobs.Add(new Job(regexResult[i].Value, prevValue, nextValue));
                }

                Params = new Parameters(jobs, new Lang(sourceLanguage, targetLanguage), regionalVariantCode);
            }

            public sealed class Parameters
            {
                private static Regex sentenceRegex = new Regex("[i]", RegexOptions.Compiled);

                [JsonPropertyName("jobs")]
                public List<Job> Jobs { get; set; }

                [JsonPropertyName("lang")]
                public Lang Lang { get; set; }

                [JsonPropertyName("priority")]
                public long Priority { get; set; }

                [JsonPropertyName("commonJobParams")]
                public CommonJobParams CommonJobParams { get; set; }

                [JsonPropertyName("timestamp")]
                public long Timestamp { get; set; }

                public Parameters(List<Job> jobs, Lang lang, string regionalVariantCode)
                {
                    Priority = 1L;
                    Lang = lang;
                    Jobs = jobs;
                    long num = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                    long num2 = 1L;
                    foreach (Job job in Jobs)
                    {
                        int count = sentenceRegex.Matches(job.RawEnSentence).Count;
                        num2 += count;
                    }
                    long num3 = num;
                    Timestamp = num3 + (num2 - num3 % num2);
                    if (!string.IsNullOrEmpty(regionalVariantCode))
                    {
                        CommonJobParams = new CommonJobParams() { RegionalVariant = regionalVariantCode };
                    }
                }
            }

            public sealed class CommonJobParams
            {
                [JsonPropertyName("formality")]
                public object Formality { get; set; }

                [JsonPropertyName("regionalVariant")]
                public string RegionalVariant { get; set; }
            }

            public sealed class Job
            {
                [JsonPropertyName("kind")]
                public string Kind { get; set; }

                [JsonPropertyName("raw_en_sentence")]
                public string RawEnSentence { get; set; }

                [JsonPropertyName("raw_en_context_before")]
                public List<string> RawEnContextBefore { get; set; }

                [JsonPropertyName("raw_en_context_after")]
                public List<string> RawEnContextAfter { get; set; }

                [JsonPropertyName("preferred_num_beams")]
                public long PreferredNumBeams { get; set; }

                [JsonIgnore] 
                public bool NewLineFollows { get; set; }

                public Job(string sentence, string contextBefore, string contextAfter)
                {
                    Kind = "default";
                    NewLineFollows = sentence.EndsWith("\r\n");
                    RawEnSentence = sentence;
                    RawEnContextBefore = new List<string>();
                    if (contextBefore != null)
                    {
                        RawEnContextBefore.Add(contextBefore);
                    }

                    RawEnContextAfter = new List<string>();
                    if (contextAfter != null)
                    {
                        RawEnContextAfter.Add(contextAfter);
                    }
                }
            }

            public sealed class Lang
            {
                [JsonPropertyName("user_preferred_langs")]
                public string[] UserPreferredLangs { get; set; }

                [JsonPropertyName("source_lang_computed")]
                public string SourceLangComputed { get; set; }

                [JsonPropertyName("target_lang")]
                public string TargetLang { get; set; }

                public Lang(string sourceLanguage, string targetLanguage)
                {
                    SourceLangComputed = sourceLanguage;
                    TargetLang = targetLanguage;
                    UserPreferredLangs = new[] {sourceLanguage, targetLanguage};
                }
            }

            public string ToJsonString()
            {
                return JsonSerializer.Serialize(this).Replace("hod\":\"",
                    ((Id + 3) % 13 == 0L || (Id + 5) % 29 == 0L) ? "hod\" : \"" : "hod\": \"");
            }
        }
    }
}
