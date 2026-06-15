using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Tokenizers.DotNet;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;

namespace Translumo.Translation.Onnx
{
    public class OnnxContainer : TranslationContainer
    {
        public OnnxContainer() : base(null, true)
        {
        }
    }

    public class OnnxTranslator : BaseTranslator<OnnxContainer>
    {
        private readonly string _modelBasePath;
        private InferenceSession _encoderSession;
        private InferenceSession _decoderSession;
        private Tokenizer _tokenizer;
        private bool _isDownloading;
        private string _loadedModelPair; // Track which language pair is currently loaded

        private const int PadTokenId = 54795;
        private const int DecoderStartTokenId = 54795;
        private const int EosTokenId = 0;
        private const int MaxLength = 512;

        // Bad word IDs that should never be generated (from model config)
        private static readonly HashSet<int> BadWordIds = new HashSet<int> { PadTokenId };

        public OnnxTranslator(TranslationConfiguration translationConfiguration, LanguageService languageService, ILogger logger)
            : base(translationConfiguration, languageService, logger)
        {
            _modelBasePath = string.IsNullOrWhiteSpace(translationConfiguration.OnnxModelPath) 
                ? "Models" 
                : translationConfiguration.OnnxModelPath;
        }

        protected override IList<OnnxContainer> CreateContainers(TranslationConfiguration configuration)
        {
            return new List<OnnxContainer> { new OnnxContainer() };
        }

        private async Task<bool> EnsureModelLoadedAsync(string sourceLang, string targetLang)
        {
            string pair = $"{sourceLang}-{targetLang}";

            // If sessions are loaded but for a different language pair, dispose and reload
            if (_loadedModelPair != null && _loadedModelPair != pair)
            {
                Logger.LogInformation($"Language pair changed from {_loadedModelPair} to {pair}, reloading model...");
                DisposeModelSessions();
            }

            if (_encoderSession != null && _decoderSession != null && _tokenizer != null)
                return true;

            string modelDir = Path.Combine(_modelBasePath, pair);

            if (!Directory.Exists(modelDir) || 
                !File.Exists(Path.Combine(modelDir, "encoder_model.onnx")) ||
                !File.Exists(Path.Combine(modelDir, "decoder_model.onnx")) ||
                !File.Exists(Path.Combine(modelDir, "tokenizer.json")))
            {
                if (!_isDownloading)
                {
                    _isDownloading = true;
                    Logger.LogInformation($"Downloading ONNX model for {pair}...");
                    _ = Task.Run(() => DownloadModelAsync(pair, modelDir));
                }
                return false;
            }

            try
            {
                var options = new SessionOptions
                {
                    IntraOpNumThreads = 1,
                    InterOpNumThreads = 1
                };
                
                _encoderSession = new InferenceSession(Path.Combine(modelDir, "encoder_model.onnx"), options);
                _decoderSession = new InferenceSession(Path.Combine(modelDir, "decoder_model.onnx"), options);

                _tokenizer = new Tokenizer(Path.Combine(modelDir, "tokenizer.json"));
                _loadedModelPair = pair;
                
                return true;
            }
            catch (Exception ex)
            {
                DisposeModelSessions();
                throw new TranslationException($"Failed to load ONNX model from {modelDir}", ex);
            }
        }

        private void DisposeModelSessions()
        {
            _encoderSession?.Dispose();
            _encoderSession = null;
            _decoderSession?.Dispose();
            _decoderSession = null;
            _tokenizer = null;
            _loadedModelPair = null;
        }

        private async Task DownloadModelAsync(string pair, string dir)
        {
            try
            {
                Directory.CreateDirectory(dir);
                string baseUrl = $"https://huggingface.co/Xenova/opus-mt-{pair}/resolve/main";
                using var client = new HttpClient();
                client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
                
                var files = new[]
                {
                    "onnx/encoder_model.onnx",
                    "onnx/decoder_model.onnx",
                    "tokenizer.json"
                };

                foreach (var file in files)
                {
                    string localFile = Path.Combine(dir, Path.GetFileName(file));
                    string tempFile = localFile + ".tmp";
                    if (!File.Exists(localFile))
                    {
                        Logger.LogInformation($"Downloading {file}...");
                        var response = await client.GetAsync($"{baseUrl}/{file}").ConfigureAwait(false);
                        response.EnsureSuccessStatusCode();

                        // Download to a temp file first, then rename to avoid partial files
                        using (var fs = new FileStream(tempFile, FileMode.Create))
                        {
                            await response.Content.CopyToAsync(fs).ConfigureAwait(false);
                        }
                        File.Move(tempFile, localFile);

                        if (file == "tokenizer.json")
                        {
                            try 
                            {
                                var text = await File.ReadAllTextAsync(localFile);
                                int start = text.IndexOf("\"normalizer\":");
                                int end = text.IndexOf("\"pre_tokenizer\":");
                                if (start != -1 && end != -1)
                                {
                                    // Substring(0, start) contains the comma from the previous property (e.g. `],\n  `)
                                    // So we just concatenate them directly without removing commas!
                                    text = text.Substring(0, start) + text.Substring(end);
                                    await File.WriteAllTextAsync(localFile, text);
                                    Logger.LogInformation("Patched tokenizer.json for Rust compatibility (removed null normalizer).");
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, "Failed to patch tokenizer.json");
                            }
                        }
                    }
                }
                Logger.LogInformation($"ONNX model download complete for {pair}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to download ONNX model for {pair}");
                // Clean up partial downloads so next attempt starts fresh
                try
                {
                    foreach (var tmpFile in Directory.GetFiles(dir, "*.tmp"))
                    {
                        File.Delete(tmpFile);
                    }
                }
                catch { /* best effort cleanup */ }
            }
            finally
            {
                _isDownloading = false;
            }
        }

        protected override async Task<string> TranslateTextInternal(OnnxContainer container, string sourceText)
        {
            string srcLang = SourceLangDescriptor.Code.Substring(0, 2).ToLower();
            string tgtLang = TargetLangDescriptor.Code.Substring(0, 2).ToLower();

            bool isReady = await EnsureModelLoadedAsync(srcLang, tgtLang);
            if (!isReady)
            {
                return "Sedang mengunduh model ONNX (sekitar 70MB)... Mohon tunggu beberapa menit dan coba translate lagi.";
            }

            // 1. Tokenize Input
            var encodeResult = _tokenizer.Encode(sourceText);
            var inputIds = encodeResult.Select(id => (long)id).ToList();
            inputIds.Add(EosTokenId); // MarianNMT requires EOS at the end

            var inputTensor = new DenseTensor<long>(inputIds.ToArray(), new[] { 1, inputIds.Count });
            var attentionMaskTensor = new DenseTensor<long>(Enumerable.Repeat(1L, inputIds.Count).ToArray(), new[] { 1, inputIds.Count });

            // 2. Encoder Forward
            var encoderInputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
                NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
            };

            using var encoderResults = _encoderSession.Run(encoderInputs);
            var encoderHiddenStates = encoderResults.First(v => v.Name == "last_hidden_state").AsTensor<float>();

            // 3. Decoder Loop (Greedy Search)
            var decoderInputIds = new List<long> { DecoderStartTokenId };
            
            for (int i = 0; i < MaxLength; i++)
            {
                var decoderInputTensor = new DenseTensor<long>(decoderInputIds.ToArray(), new[] { 1, decoderInputIds.Count });

                var decoderInputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", decoderInputTensor),
                    NamedOnnxValue.CreateFromTensor("encoder_hidden_states", encoderHiddenStates),
                    NamedOnnxValue.CreateFromTensor("encoder_attention_mask", attentionMaskTensor)
                };

                using var decoderResults = _decoderSession.Run(decoderInputs);
                var logits = decoderResults.First(v => v.Name == "logits").AsTensor<float>();

                // Get argmax of the last token, suppressing bad word IDs
                int vocabSize = logits.Dimensions[2];
                long nextToken = EosTokenId; // Default to EOS if nothing valid found
                float maxLogit = float.MinValue;
                int seqLen = logits.Dimensions[1];
                
                // We want the logits for the *last* token in the sequence
                for (int v = 0; v < vocabSize; v++)
                {
                    // Suppress bad word IDs (e.g., pad token should never be generated)
                    if (BadWordIds.Contains(v))
                        continue;

                    float val = logits[0, seqLen - 1, v];
                    if (val > maxLogit)
                    {
                        maxLogit = val;
                        nextToken = v;
                    }
                }

                if (nextToken == EosTokenId)
                {
                    break;
                }

                decoderInputIds.Add(nextToken);
            }

            // Remove DecoderStartTokenId
            var outputTokens = decoderInputIds.Skip(1).Select(id => (uint)id).ToArray();
            var decodedText = _tokenizer.Decode(outputTokens);

            return decodedText.Trim();
        }
    }
}
