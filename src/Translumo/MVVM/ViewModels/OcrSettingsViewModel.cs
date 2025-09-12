using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Translumo.Dialog;
using Translumo.Dialog.Stages;
using Translumo.Infrastructure.Language;
using Translumo.OCR.Configuration;
using Translumo.OCR.EasyOCR;
using Translumo.OCR.Tesseract;
using Translumo.OCR.WindowsOCR;
using Translumo.Translation.Configuration;
using Translumo.Utils;

namespace Translumo.MVVM.ViewModels
{
    public sealed class OcrSettingsViewModel : BindableBase
    {
        public bool WindowsOcrEnabled
        {
            get => _ocrConfiguration.GetConfiguration<WindowsOCRConfiguration>().Enabled;
            set
            {
                _ocrConfiguration.GetConfiguration<WindowsOCRConfiguration>().Enabled = value;
                OnPropertyChanged(nameof(WindowsOcrEnabled));
                if (value)
                {
                    Task.Run(CheckWindowsOcrAvailabilityAsync);
                }
            }
        }

        public bool EasyOcrEnabled
        {
            get => _ocrConfiguration.GetConfiguration<EasyOCRConfiguration>().Enabled;
            set
            {
                if (value)
                {
                    EnableEasyOcrAsync();
                }
                else
                {
                    _ocrConfiguration.GetConfiguration<EasyOCRConfiguration>().Enabled = false;
                    OnPropertyChanged();
                }
            }
        }

        public bool TesseractOcrEnabled
        {
            get => _ocrConfiguration.GetConfiguration<TesseractOCRConfiguration>().Enabled;
            set
            {
                _ocrConfiguration.GetConfiguration<TesseractOCRConfiguration>().Enabled = value;
                OnPropertyChanged(nameof(TesseractOcrEnabled));
            }
        }

        private OcrGeneralConfiguration _ocrConfiguration;
        private TranslationConfiguration _translationConfiguration;

        private readonly LanguageService _languageService;
        private readonly DialogService _dialogService;
        private readonly ILogger _logger;

        public OcrSettingsViewModel(OcrGeneralConfiguration ocrConfiguration, TranslationConfiguration translationConfiguration, 
            DialogService dialogService, LanguageService languageService, ILogger<OcrSettingsViewModel> logger)
        {
            this._ocrConfiguration = ocrConfiguration;
            this._languageService = languageService;
            this._translationConfiguration = translationConfiguration;
            this._dialogService = dialogService;
            this._logger = logger;
        }

        private async Task CheckWindowsOcrAvailabilityAsync()
        {
            var toEnableWindowsOcr = false;
            var langCode = _languageService.GetLanguageDescriptor(_translationConfiguration.TranslateFromLang).Code;

            if (WindowsOCRHelper.IsLanguageOcrCapabilityInstalled(langCode, exactMatching: false))
            {
                return;
            }
            
            ActionInteractionStage stageToEnableFlag = new ActionInteractionStage(_dialogService, () =>
            {
                toEnableWindowsOcr = true;
                return Task.CompletedTask;
            });

            try
            {
                var initialStage = StagesFactory.CreateWindowsOcrCheckingStages(_dialogService, langCode, stageToEnableFlag, _logger);
                await initialStage.ExecuteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error during WindowsOCR switching");
            }
            finally
            {
                if (!toEnableWindowsOcr)
                {
                    WindowsOcrEnabled = false;
                }
            }
        }

        private async Task EnableEasyOcrAsync()
        {
            ActionInteractionStage stageToEnableOcr = new ActionInteractionStage(_dialogService, () =>
            {
                _ocrConfiguration.GetConfiguration<EasyOCRConfiguration>().Enabled = true;
                OnPropertyChanged(nameof(EasyOcrEnabled));
                return Task.CompletedTask;
            });

            try
            {
                var initialStage = StagesFactory.CreateEasyOcrCheckingStages(_dialogService, stageToEnableOcr, _logger);

                await initialStage.ExecuteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error during EasyOCR switching");
                EasyOcrEnabled = false;
            }
        }
    }
}
