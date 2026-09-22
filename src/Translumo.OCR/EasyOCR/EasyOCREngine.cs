using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Python.Runtime;
using Translumo.Infrastructure.Constants;
using Translumo.Infrastructure.Language;
using Translumo.Infrastructure.Python;

namespace Translumo.OCR.EasyOCR
{
    public class EasyOCREngine : IOCREngine, IDisposable
    {
        public byte PrimaryPriority => 2;
        public bool SecondaryPrimaryCheck => false;
        public int Confidence => 9;
        public Languages DetectionLanguage => _languageDescriptor.Language;

        private bool _objectsInitialized;
        private bool _readerIsUsed;

        private readonly object _obj = new object();
        private readonly LanguageDescriptor _languageDescriptor;
        private readonly PythonEngineWrapper _pythonEngine;
        private readonly string _modelPath = Path.Combine(Global.ModelsPath, "easyocr");
        private readonly ILogger _logger;

        #region Python objects
        private dynamic _builtinsLib;
        private dynamic _easyOcrLib;
        private dynamic _reader;
        private dynamic _bytes;
        #endregion

        public EasyOCREngine(LanguageDescriptor languageDescriptor, PythonEngineWrapper pythonEngine, ILogger logger)
        {
            _languageDescriptor = languageDescriptor;
            _pythonEngine = pythonEngine;
            _logger = logger;

            _logger.LogTrace($"Initialization EasyOCR from path: '{Global.PythonPath}'");

            Task.Factory.StartNew(EnsureInitialized);
        }

        public string[] GetTextLines(byte[] image)
        {
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            lock (_obj)
            {
                if (!_objectsInitialized)
                {
                    throw new InvalidOperationException($"EasyOCR is not initialized");
                }

                return _pythonEngine.Execute(() =>
                {
                    dynamic ocrResult = _reader.readtext(_bytes.Invoke(image.ToPython()), detail: 0, paragraph: true);
                    _readerIsUsed = true;

                    return (string[])ocrResult;
                });
            }
        }

        private void EnsureInitialized()
        {
            lock (_obj)
            {
                try
                {
                    _pythonEngine.Init();

                    if (!_objectsInitialized)
                    {
                        InitializeObjects();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"EasyOCR initialization error");
                }
            }
        }

        private void InitializeObjects() =>
            _pythonEngine.Execute(() =>
            {
                _builtinsLib = _pythonEngine.Import("builtins");
                _easyOcrLib = _pythonEngine.Import("easyocr");
                _reader = _easyOcrLib.Reader(new[] { _languageDescriptor.EasyOcrCode }, model_storage_directory: _modelPath, download_enabled: false, recog_network: _languageDescriptor.EasyOcrModel);
                _bytes = _builtinsLib.bytes;

                _objectsInitialized = true;
            });

        public void Dispose()
        {
            lock (_obj)
            {
                if (_objectsInitialized)
                {
                    _pythonEngine.Execute(() =>
                    {
                        _builtinsLib.Dispose();
                        _easyOcrLib.Dispose();
                        if (_readerIsUsed)
                        {
                            _reader.Dispose();
                        }
                    });

                    _readerIsUsed = false;
                    _objectsInitialized = false;
                }

                _pythonEngine.Dispose();
            }
        }
    }
}
