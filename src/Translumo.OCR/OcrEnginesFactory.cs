using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Language;
using Translumo.Infrastructure.Python;
using Translumo.OCR.Configuration;
using Translumo.OCR.EasyOCR;
using Translumo.OCR.Tesseract;
using Translumo.OCR.WindowsOCR;

namespace Translumo.OCR
{
    public class OcrEnginesFactory
    {
        private readonly List<IOCREngine> _cachedEngines;

        private readonly LanguageService _languageService;
        private readonly PythonEngineWrapper _pythonEngine;
        private readonly ILogger _logger;

        public OcrEnginesFactory(LanguageService languageService, PythonEngineWrapper pythonEngine, ILogger<OcrEnginesFactory> logger)
        {
            _languageService = languageService;
            _pythonEngine = pythonEngine;
            _logger = logger;
            _cachedEngines = new List<IOCREngine>();
        }

        public IEnumerable<IOCREngine> GetEngines(IEnumerable<OcrConfiguration> ocrConfigurations,
            Languages detectionLanguage)
        {
            var langDescriptor = _languageService.GetLanguageDescriptor(detectionLanguage);

            foreach (var ocrConfiguration in ocrConfigurations)
            {
                var confType = ocrConfiguration.GetType();

                if (confType == typeof(WindowsOCRConfiguration))
                {
                    if (!TryRemoveIfDisabled<WindowsOCREngine>(ocrConfiguration))
                    {
                        var engine = TryGetEngine(() => new WindowsOCREngine(langDescriptor), detectionLanguage);
                        if (engine != null)
                            yield return engine;
                    }

                    if (!TryRemoveIfDisabled<WinOCREngineWithPreprocess>(ocrConfiguration))
                    {
                        var engine = TryGetEngine(() => new WinOCREngineWithPreprocess(langDescriptor), detectionLanguage);
                        if (engine != null)
                            yield return engine;
                    }
                }

                if (confType == typeof(TesseractOCRConfiguration))
                {
                    if (!TryRemoveIfDisabled<TesseractOCREngine>(ocrConfiguration))
                    {
                        var engine = TryGetEngine(() => new TesseractOCREngine(langDescriptor), detectionLanguage);
                        if (engine != null)
                            yield return engine;
                    }

                    if (!TryRemoveIfDisabled<TesseractOCREngineWIthPreprocess>(ocrConfiguration))
                    {
                        var engine = TryGetEngine(() => new TesseractOCREngineWIthPreprocess(langDescriptor), detectionLanguage);
                        if (engine != null)
                            yield return engine;
                    }
                }

                if (confType == typeof(EasyOCRConfiguration))
                {
                    if (!TryRemoveIfDisabled<EasyOCREngine>(ocrConfiguration))
                    {
                        var engine = TryGetEngine(() => new EasyOCREngine(langDescriptor, _pythonEngine, _logger), detectionLanguage);
                        if (engine != null)
                            yield return engine;
                    }
                }
            }

            bool TryRemoveIfDisabled<TEngine>(OcrConfiguration configuration)
                where TEngine : IOCREngine
            {
                if (configuration.Enabled)
                {
                    return false;
                }

                RemoveCachedEngine<TEngine>();
                return true;
            }
        }

        private IOCREngine? TryGetEngine<TEngine>(Func<TEngine> ocrFactoryFunc, Languages detectionLanguage)
            where TEngine : IOCREngine
        {
            try
            {
                return GetEngine(ocrFactoryFunc, detectionLanguage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create OCR engine {EngineType} for {Lang}", typeof(TEngine).Name, detectionLanguage);
                return null;
            }
        }

        private IOCREngine GetEngine<TEngine>(Func<TEngine> ocrFactoryFunc, Languages detectionLanguage)
            where TEngine : IOCREngine
        {
            var cachedEngine = _cachedEngines.FirstOrDefault(engine => engine.GetType() == typeof(TEngine));
            if (cachedEngine == null)
            {
                cachedEngine = ocrFactoryFunc.Invoke();
                _cachedEngines.Add(cachedEngine);
                return cachedEngine;
            }

            if (cachedEngine.DetectionLanguage == detectionLanguage)
            {
                return cachedEngine;
            }

            // cached engine used another detection language
            RemoveCachedEngine<TEngine>();

            cachedEngine = ocrFactoryFunc.Invoke();
            _cachedEngines.Add(cachedEngine);

            return cachedEngine;
        }

        private void RemoveCachedEngine<TEngine>()
            where TEngine : IOCREngine
        {
            var cachedEngine = _cachedEngines.FirstOrDefault(engine => engine.GetType() == typeof(TEngine));
            if (cachedEngine == null)
            {
                return;
            }

            if (cachedEngine is IDisposable disposableEngine)
            {
                disposableEngine.Dispose();
            }

            _cachedEngines.Remove(cachedEngine);
        }
    }
}