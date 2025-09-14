using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using OpenCvSharp;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Translumo.Dialog;
using Translumo.Dialog.Stages;
using Translumo.Infrastructure.Language;
using Translumo.MVVM.Common;
using Translumo.MVVM.Models;
using Translumo.OCR.Configuration;
using Translumo.OCR.WindowsOCR;
using Translumo.Translation.Configuration;
using Translumo.TTS;
using Translumo.Utils;
using Translumo.Utils.Extensions;
using RelayCommand = Microsoft.Toolkit.Mvvm.Input.RelayCommand;

namespace Translumo.MVVM.ViewModels
{
    public sealed class LanguagesSettingsViewModel : BindableBase, IAdditionalPanelController, IDisposable
    {
        public event EventHandler<bool> PanelStateIsChanged;


        public IList<DisplayLanguage> AvailableLanguages { get; set; }
        public IList<DisplayLanguage> AvailableTranslationLanguages { get; set; }

        public TranslationConfiguration Model { get; set; }

        public TtsConfiguration TtsSettings { get; set; }


        public ObservableCollection<ProxyCardItem> ProxyCollection
        {
            get => _proxyCollection;
            set
            {
                SetProperty(ref _proxyCollection, value);
            }
        }
        public bool ProxySettingsIsOpened
        {
            get => _proxySettingsIsOpened;
            set
            {
                SetProperty(ref _proxySettingsIsOpened, value);
                PanelStateIsChanged?.Invoke(this, value);
            }
        }

        public Languages TranslateFromLang
        {
            get => Model.TranslateFromLang;
            set
            {
                ChangeSourceLanguage(value);
            }
        }

        public Languages TranslateToLang
        {
            get => Model.TranslateToLang;
            set
            {
                ChangeTargetLanguage(value);
            }
        }

        public TTSEngines TtsSystem
        {
            get => TtsSettings.TtsSystem;
            set
            {
                ChangeTtsSystem(value);
            }
        }

        public ICommand ProxySettingsClickedCommand => new RelayCommand(OnProxySettingsClicked);
        public ICommand ProxyItemDeletedCommand => new RelayCommand<ProxyCardItem>(OnProxyItemDeletedCommand);
        public ICommand ProxyItemAddCommand => new RelayCommand(OnProxyItemAddCommand);
        public ICommand ProxySettingsSubmitCommand => new RelayCommand<bool>(OnProxySettingsSubmit);

        private ObservableCollection<ProxyCardItem> _proxyCollection;
        private bool _proxySettingsIsOpened;

        private readonly DialogService _dialogService;
        private readonly OcrGeneralConfiguration _ocrConfiguration;
        private readonly LanguageService _languageService;
        private readonly ILogger _logger;

        public LanguagesSettingsViewModel(LanguageService languageService, TranslationConfiguration translationConfiguration,
            OcrGeneralConfiguration ocrConfiguration, TtsConfiguration ttsConfiguration, DialogService dialogService,
            ILogger<LanguagesSettingsViewModel> logger)
        {
            var languages = languageService.GetAll(true)
                .Select(lang => (lang.TranslationOnly, new DisplayLanguage(lang, GetLanguageDisplayName(lang))))
                .ToArray();
            this.AvailableLanguages = languages
                .Where(lang => !lang.TranslationOnly)
                .OrderBy(lang => lang.Item2.DisplayName)
                .Select(lang => lang.Item2)
                .ToList();

            this.AvailableTranslationLanguages = languages
                .OrderBy(lang => lang.Item2.DisplayName)
                .Select(lang => lang.Item2)
                .ToList();

            this.Model = translationConfiguration;
            this.TtsSettings = ttsConfiguration;
            this.TtsSettings.TtsLanguage = this.Model.TranslateToLang;


            this._languageService = languageService;
            this._dialogService = dialogService;
            this._ocrConfiguration = ocrConfiguration;
            this._logger = logger;
        }

        private void OnProxySettingsClicked()
        {
            InitializeProxyCollection();
            ProxySettingsIsOpened = true;
        }

        private void OnProxyItemDeletedCommand(ProxyCardItem itemToDelete)
        {
            _proxyCollection.Remove(itemToDelete);
        }

        private void OnProxyItemAddCommand()
        {
            _proxyCollection.Add(new ProxyCardItem());
        }

        private void OnProxySettingsSubmit(bool applyProxy)
        {
            if (applyProxy)
            {
                Model.ProxySettings = ProxyCollection.Where(pr => pr.IsValid())
                    .Select(pr => pr.MapTo<ProxyCardItem, Proxy>())
                    .ToList();
            }

            ProxySettingsIsOpened = false;
        }

        private async Task ChangeSourceLanguage(Languages language)
        {
            try
            {
                var changeLangStage = StagesFactory.CreateLanguageChangeStages(_dialogService, () => Model.TranslateFromLang = language,
                    _logger);

                if (_ocrConfiguration.GetConfiguration<WindowsOCRConfiguration>().Enabled)
                {
                    var langCode = _languageService.GetLanguageDescriptor(language).Code;
                    changeLangStage = StagesFactory.CreateWindowsOcrCheckingStages(_dialogService, langCode, changeLangStage, _logger);
                }

                await changeLangStage.ExecuteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error during source language change");
            }

            OnPropertyChanged(nameof(TranslateFromLang));
        }

        private async Task ChangeTargetLanguage(Languages language)
        {
            var changeLanguageAction = () =>
            {
                this.TtsSettings.TtsLanguage = language;
                this.Model.TranslateToLang = language;
            };

            await this.ReconfigureTts(language, TtsSettings.TtsSystem, changeLanguageAction);
            OnPropertyChanged(nameof(TranslateToLang));
        }

        private async Task ChangeTtsSystem(TTSEngines engine)
        {
            Action changeTtsEngineAction = () => this.TtsSettings.TtsSystem = engine;
            await this.ReconfigureTts(TtsSettings.TtsLanguage, engine, changeTtsEngineAction);
            OnPropertyChanged(nameof(TtsSystem));
        }

        private async Task ReconfigureTts(Languages language, TTSEngines engine, Action changeParameter)
        {
            try
            {
                var changeLangStage = StagesFactory.CreateLanguageChangeStages(
                    _dialogService,
                    changeParameter,
                    _logger);

                if (engine == TTSEngines.WindowsTTS
                    && !TtsSettings.InstalledWinTtsLanguages.Contains(language))
                {
                    var langCode = _languageService.GetLanguageDescriptor(language).Code;
                    changeLangStage.AddNextStage(new ActionInteractionStage(_dialogService, () =>
                    {
                        this.TtsSettings.InstalledWinTtsLanguages.Add(language);
                        return Task.CompletedTask;
                    }));
                    changeLangStage = StagesFactory.CreateWindowsTtsCheckingStages(_dialogService, langCode, changeLangStage, _logger);
                }
                //else if (engine == TTSEngines.SileroTTS)
                //{
                //    var languageDescriptor = _languageService.GetLanguageDescriptor(language);
                //    changeLangStage = StagesFactory.CreateSileroTtsCheckingStages(languageDescriptor, _dialogService, changeLangStage, _logger);
                //}

                await changeLangStage.ExecuteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error during source language change");
            }
        }

        private string GetLanguageDisplayName(LanguageDescriptor languageDescriptor)
        {
            return LocalizationManager.GetValue($"Str.Languages.{languageDescriptor.Language}", false,
               OnLocalizedValueChanged, this);
        }

        private void OnLocalizedValueChanged(string key, string oldValue)
        {
            var availableLang = AvailableTranslationLanguages.First(lang => lang.DisplayName == oldValue);
            availableLang.DisplayName = LocalizationManager.GetValue(key, false, OnLocalizedValueChanged, this);
        }

        private void InitializeProxyCollection()
        {
            ProxyCollection = new ObservableCollection<ProxyCardItem>(Model.ProxySettings.Select(st => st.MapTo<Proxy, ProxyCardItem>()));
        }

        public void ClosePanel()
        {
            ProxySettingsIsOpened = false;
        }

        public void Dispose()
        {
            LocalizationManager.ReleaseChangedValuesCallbacks(this);
        }
    }
}
