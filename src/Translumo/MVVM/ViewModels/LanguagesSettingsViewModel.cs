using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using OpenCvSharp;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Translumo.Dialog;
using Translumo.Dialog.Stages;
using Translumo.Infrastructure.Language;
using Translumo.MVVM.Common;
using Translumo.MVVM.Models;
using Translumo.OCR.Configuration;
using Translumo.OCR.WindowsOCR;
using Translumo.Translation;
using Translumo.Translation.Configuration;
using Translumo.TTS;
using Translumo.Utils;
using Translumo.Utils.Extensions;
using Translumo.Utils.Http;
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

        public bool IsApiKeyRequired => SelectedTranslator == Translators.Deepseek || SelectedTranslator == Translators.Gemini || SelectedTranslator == Translators.Openrouter;

        public bool IsOpenrouterSelected => SelectedTranslator == Translators.Openrouter;

        public string OpenrouterModel
        {
            get => Model.OpenrouterModel;
            set
            {
                Model.OpenrouterModel = value;
                OnPropertyChanged(nameof(OpenrouterModel));
            }
        }

        public Translators SelectedTranslator
        {
            get => Model.Translator;
            set
            {
                Model.Translator = value;
                OnPropertyChanged(nameof(SelectedTranslator));
                OnPropertyChanged(nameof(SelectedTranslatorIndex));
                OnPropertyChanged(nameof(IsApiKeyRequired));
                OnPropertyChanged(nameof(IsOpenrouterSelected));
                OnPropertyChanged(nameof(CurrentApiKey));
                OnPropertyChanged(nameof(CurrentApiKeyName));
            }
        }

        public int SelectedTranslatorIndex
        {
            get => (int)SelectedTranslator;
            set => SelectedTranslator = (Translators)value;
        }

        public string CurrentApiKeyName
        {
            get => SelectedTranslator switch
            {
                Translators.Deepseek => "Deepseek API Key",
                Translators.Gemini => "Gemini API Key",
                Translators.Openrouter => "OpenRouter API Key",
                _ => string.Empty
            };
        }

        public string CurrentApiKey
        {
            get
            {
                return SelectedTranslator switch
                {
                    Translators.Deepseek => Model.DeepseekApiKey,
                    Translators.Gemini => Model.GeminiApiKey,
                    Translators.Openrouter => Model.OpenrouterApiKey,
                    _ => string.Empty
                };
            }
            set
            {
                switch (SelectedTranslator)
                {
                    case Translators.Deepseek: Model.DeepseekApiKey = value; break;
                    case Translators.Gemini: Model.GeminiApiKey = value; break;
                    case Translators.Openrouter: Model.OpenrouterApiKey = value; break;
                }
                OnPropertyChanged(nameof(CurrentApiKey));
            }
        }

        public string ApiKeyValidationStatus
        {
            get => _apiKeyValidationStatus;
            set => SetProperty(ref _apiKeyValidationStatus, value);
        }

        public bool IsValidating
        {
            get => _isValidating;
            set => SetProperty(ref _isValidating, value);
        }

        public string OpenrouterModelValidationStatus
        {
            get => _openrouterModelValidationStatus;
            set => SetProperty(ref _openrouterModelValidationStatus, value);
        }

        public bool IsValidatingModel
        {
            get => _isValidatingModel;
            set => SetProperty(ref _isValidatingModel, value);
        }


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
        public ICommand ValidateApiKeyCommand => new AsyncRelayCommand(OnValidateApiKeyAsync);
        public ICommand ValidateOpenrouterModelCommand => new AsyncRelayCommand(OnValidateOpenrouterModelAsync);

        private ObservableCollection<ProxyCardItem> _proxyCollection;
        private bool _proxySettingsIsOpened;
        private string _apiKeyValidationStatus;
        private bool _isValidating;
        private string _openrouterModelValidationStatus;
        private bool _isValidatingModel;

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

        private async Task OnValidateApiKeyAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentApiKey))
            {
                ApiKeyValidationStatus = "✗ API Key is empty.";
                await _dialogService.ShowDialogAsync(SimpleDialogViewModel.Create(
                    "API Key cannot be empty. Please enter a valid key.",
                    SimpleDialogTypes.Error, "Validation Failed"));
                return;
            }

            IsValidating = true;
            ApiKeyValidationStatus = "⏳ Validating...";

            try
            {
                bool isValid = false;
                string errorMessage = null;

                switch (SelectedTranslator)
                {
                    case Translators.Deepseek:
                        (isValid, errorMessage) = await ValidateDeepseekKeyAsync(CurrentApiKey);
                        break;
                    case Translators.Gemini:
                        (isValid, errorMessage) = await ValidateGeminiKeyAsync(CurrentApiKey);
                        break;
                    case Translators.Openrouter:
                        (isValid, errorMessage) = await ValidateOpenrouterKeyAsync(CurrentApiKey);
                        break;
                }

                if (isValid)
                {
                    ApiKeyValidationStatus = "✓ Valid";
                    await _dialogService.ShowDialogAsync(SimpleDialogViewModel.Create(
                        $"{CurrentApiKeyName} is valid and ready to use!",
                        SimpleDialogTypes.Info, "Validation Successful"));
                }
                else
                {
                    ApiKeyValidationStatus = "✗ Invalid";
                    await _dialogService.ShowDialogAsync(SimpleDialogViewModel.Create(
                        $"{CurrentApiKeyName} validation failed: {errorMessage}",
                        SimpleDialogTypes.Error, "Validation Failed"));
                }
            }
            catch (Exception ex)
            {
                ApiKeyValidationStatus = "✗ Error";
                _logger.LogError(ex, "API Key validation failed");
                await _dialogService.ShowDialogAsync(SimpleDialogViewModel.Create(
                    $"Validation error: {ex.Message}",
                    SimpleDialogTypes.Error, "Validation Error"));
            }
            finally
            {
                IsValidating = false;
            }
        }

        private async Task<(bool isValid, string error)> ValidateDeepseekKeyAsync(string apiKey)
        {
            var reader = new HttpReader();
            reader.ContentType = "application/json";
            reader.Accept = "application/json";
            reader.ThrowExceptions = false;
            reader.OptionalHeaders.Add("Authorization", $"Bearer {apiKey}");

            var payload = new
            {
                model = "deepseek-chat",
                messages = new[] { new { role = "user", content = "Hi" } },
                max_tokens = 5
            };

            var response = await reader.RequestWebDataAsync(
                "https://api.deepseek.com/chat/completions",
                HttpMethods.POST, JsonSerializer.Serialize(payload));

            if (response.IsSuccessful)
                return (true, null);

            return (false, response.Body ?? "Unable to reach Deepseek API. Check your key and network.");
        }

        private async Task<(bool isValid, string error)> ValidateGeminiKeyAsync(string apiKey)
        {
            var reader = new HttpReader();
            reader.ContentType = "application/json";
            reader.Accept = "application/json";
            reader.ThrowExceptions = false;

            // Use the models list endpoint — lightweight and proves the key works
            var response = await reader.RequestWebDataAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}",
                HttpMethods.GET);

            if (response.IsSuccessful)
                return (true, null);

            return (false, response.Body ?? "Unable to reach Gemini API. Check your key and network.");
        }

        private async Task<(bool isValid, string error)> ValidateOpenrouterKeyAsync(string apiKey)
        {
            var reader = new HttpReader();
            reader.ContentType = "application/json";
            reader.Accept = "application/json";
            reader.ThrowExceptions = false;
            reader.OptionalHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Use the auth/key endpoint to check credits/validity
            var response = await reader.RequestWebDataAsync(
                "https://openrouter.ai/api/v1/auth/key",
                HttpMethods.GET);

            if (response.IsSuccessful)
                return (true, null);

            return (false, response.Body ?? "Unable to reach OpenRouter API. Check your key and network.");
        }

        private async Task OnValidateOpenrouterModelAsync()
        {
            var modelName = OpenrouterModel;
            if (string.IsNullOrWhiteSpace(modelName))
            {
                OpenrouterModelValidationStatus = "✗ Model name is empty.";
                await _dialogService.ShowDialogAsync(SimpleDialogViewModel.Create(
                    "Model name cannot be empty. Please enter a valid OpenRouter model identifier.",
                    SimpleDialogTypes.Error, "Validation Failed"));
                return;
            }

            var apiKey = Model.OpenrouterApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                OpenrouterModelValidationStatus = "✗ API Key is required.";
                await _dialogService.ShowDialogAsync(SimpleDialogViewModel.Create(
                    "OpenRouter API Key is required to validate the model. Please enter your API Key first.",
                    SimpleDialogTypes.Error, "Validation Failed"));
                return;
            }

            IsValidatingModel = true;
            OpenrouterModelValidationStatus = "⏳ Validating model...";

            try
            {
                var (isValid, errorMessage) = await ValidateOpenrouterModelRequestAsync(apiKey, modelName);

                if (isValid)
                {
                    OpenrouterModelValidationStatus = "✓ Model is valid";
                    await _dialogService.ShowDialogAsync(SimpleDialogViewModel.Create(
                        $"Model '{modelName}' is valid and accessible with your API key!",
                        SimpleDialogTypes.Info, "Model Validation Successful"));
                }
                else
                {
                    OpenrouterModelValidationStatus = "✗ Invalid model";
                    await _dialogService.ShowDialogAsync(SimpleDialogViewModel.Create(
                        $"Model validation failed: {errorMessage}",
                        SimpleDialogTypes.Error, "Model Validation Failed"));
                }
            }
            catch (Exception ex)
            {
                OpenrouterModelValidationStatus = "✗ Error";
                _logger.LogError(ex, "OpenRouter model validation failed");
                await _dialogService.ShowDialogAsync(SimpleDialogViewModel.Create(
                    $"Validation error: {ex.Message}",
                    SimpleDialogTypes.Error, "Validation Error"));
            }
            finally
            {
                IsValidatingModel = false;
            }
        }

        private async Task<(bool isValid, string error)> ValidateOpenrouterModelRequestAsync(string apiKey, string modelName)
        {
            var reader = new HttpReader();
            reader.ContentType = "application/json";
            reader.Accept = "application/json";
            reader.ThrowExceptions = false;
            reader.OptionalHeaders.Add("Authorization", $"Bearer {apiKey}");

            var payload = new
            {
                model = modelName,
                messages = new[] { new { role = "user", content = "Hi" } },
                max_tokens = 1
            };

            var response = await reader.RequestWebDataAsync(
                "https://openrouter.ai/api/v1/chat/completions",
                HttpMethods.POST, JsonSerializer.Serialize(payload));

            if (response.IsSuccessful)
                return (true, null);

            // Try to extract a meaningful error message from the response body
            try
            {
                using var doc = JsonDocument.Parse(response.Body);
                if (doc.RootElement.TryGetProperty("error", out var errorElement))
                {
                    var message = errorElement.TryGetProperty("message", out var msgElement)
                        ? msgElement.GetString()
                        : response.Body;
                    return (false, message);
                }
            }
            catch { /* Ignore parse failures, fall through to generic message */ }

            return (false, response.Body ?? "Unable to validate model. Check your API key, model name, and network.");
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
