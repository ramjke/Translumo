using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Translumo.Dialog;
using Translumo.HotKeys;
using Translumo.Infrastructure;
using Translumo.Infrastructure.Constants;
using Translumo.Infrastructure.Dispatching;
using Translumo.Infrastructure.Language;
using Translumo.MVVM.Models;
using Translumo.Processing.ImageTranslation;
using Translumo.Processing.Interfaces;
using Translumo.Services;
using Translumo.Update;
using Translumo.Utils;
using RelayCommand = Microsoft.Toolkit.Mvvm.Input.RelayCommand;

namespace Translumo.MVVM.ViewModels
{
    public sealed class ChatWindowViewModel : BindableBase
    {
        public ChatWindowModel Model { get; set; }
        public bool ChatWindowIsVisible
        {
            get => _chatWindowIsVisible;
            set => SetProperty(ref _chatWindowIsVisible, value);
        }

        public ICommand ShowHideSettingsCommand => new RelayCommand(OnShowHideSettings);
        public ICommand ShowHideChatCommand => new RelayCommand(OnShowHideChat);
        public ICommand LoadedCommand => new RelayCommand(OnLoadedCommand);

        private bool _chatWindowIsVisible = true;
        private bool _hasUpdates = false;

        private readonly DialogService _dialogService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly HotKeysServiceManager _hotKeysServiceManager;
        private readonly UpdateManager _updateManager;
        private readonly ImageTranslationService _imageTranslationService;
        private readonly ICapturerFactory _capturerFactory;
        private readonly LanguageService _languageService;

        private IScreenCapturer _imageCapturer;
        private Languages _lastImageTarget = Languages.Vietnamese;

        public ChatWindowViewModel(ChatWindowModel model, HotKeysServiceManager hotKeysManager, ChatUITextMediator chatTextMediator, UpdateManager updateManager,
            IActionDispatcher dispatcher, DialogService dialogService, IServiceProvider serviceProvider, ImageTranslationService imageTranslationService,
            ICapturerFactory capturerFactory, LanguageService languageService, ILogger<ChatWindowViewModel> logger)
        {
            this.Model = model;
            this._logger = logger;
            this._dialogService = dialogService;
            this._serviceProvider = serviceProvider;
            this._hotKeysServiceManager = hotKeysManager;
            this._updateManager = updateManager;
            this._imageTranslationService = imageTranslationService;
            this._capturerFactory = capturerFactory;
            this._languageService = languageService;

            dispatcher.RegisterConsumer<BrowseSiteDispatchArg, BrowseSiteDispatchResult>(DispatcherActions.PASS_SITE, BrowseSiteHandler);

            hotKeysManager.SelectAreaKeyPressed += HotKeysManagerOnSelectAreaKeyPressed;
            hotKeysManager.TranslationStateKeyPressed += HotKeysManagerOnTranslationStateKeyPressed;
            hotKeysManager.ChatVisibilityKeyPressed += HotKeysManagerOnChatVisibilityKeyPressed;
            hotKeysManager.SettingVisibilityKeyPressed += HotKeysManagerOnSettingVisibilityKeyPressed;
            hotKeysManager.ShowSelectionAreaKeyPressed += HotKeysManagerOnShowSelectionAreaKeyPressed;
            hotKeysManager.OnceTranslateKeyPressed += HotKeysManagerOnOnceTranslateKeyPressed;
            hotKeysManager.ImageTranslateKeyPressed += HotKeysManagerOnImageTranslateKeyPressed;
            hotKeysManager.WindowStyleChangeKeyPressed += HotKeysManagerOnWindowStyleChangeKeyPressed;
            chatTextMediator.TextRaised += ChatTextMediatorOnTextRaised;
            chatTextMediator.ClearTextsRaised += ChatTextMediatorOnClearTextsRaised;
        }

        private void HotKeysManagerOnSettingVisibilityKeyPressed(object sender, EventArgs e)
        {
            OnShowHideSettings();
        }

        private void ChatTextMediatorOnTextRaised(object sender, TranslatedEventArgs e)
        {
            Model.AddChatItem(e.Text, e.TextType);
        }

        private void ChatTextMediatorOnClearTextsRaised(object sender, EventArgs e)
        {
            Model.ClearAllChatItems();
        }


        private void HotKeysManagerOnChatVisibilityKeyPressed(object sender, EventArgs e)
        {
            OnShowHideChat();
        }

        private void HotKeysManagerOnTranslationStateKeyPressed(object sender, EventArgs e)
        {
            if (Model.TranslationIsRunning)
            {
                Model.EndTranslation();
            }
            else
            {
                StartTranslation(true);
            }
        }

        private void HotKeysManagerOnSelectAreaKeyPressed(object sender, EventArgs e)
        {
            Model.EndTranslation();
            
            var result = _dialogService.ShowWindowDialog<SelectionAreaWindow>(out var window);
            if (result.HasValue && result.Value)
            {
                Model.CaptureConfiguration.CaptureArea = window.SelectedArea;
            }
        }

        private void HotKeysManagerOnShowSelectionAreaKeyPressed(object sender, EventArgs e)
        {
            if (!Model.CaptureConfiguration.CaptureArea.IsEmpty)
            {
                _dialogService.ShowWindowDialog<SelectionAreaWindow>(out _, Model.CaptureConfiguration.CaptureArea);
            }
        }

        private void HotKeysManagerOnOnceTranslateKeyPressed(object sender, EventArgs e)
        {
            if (_dialogService.WindowIsOpened<SettingsViewModel>())
            {
                Model.AddChatItem(LocalizationManager.GetValue("Str.Chat.SettingsOpened"), TextTypes.Info);

                return;
            }

            var result = _dialogService.ShowWindowDialog<SelectionAreaWindow>(out var window);
            if (result.HasValue && result.Value)
            {
                Model.OnceTranslation(window.SelectedArea);
            }
        }

        private void HotKeysManagerOnImageTranslateKeyPressed(object sender, EventArgs e)
        {
            if (_dialogService.WindowIsOpened<SettingsViewModel>())
            {
                Model.AddChatItem(LocalizationManager.GetValue("Str.Chat.SettingsOpened"), TextTypes.Info);

                return;
            }

            var result = _dialogService.ShowWindowDialog<SelectionAreaWindow>(out var window);
            if (result.HasValue && result.Value && window != null && !window.SelectedArea.IsEmpty)
            {
                _ = ShowImageTranslationAsync(window.SelectedArea);
            }
        }

        private async Task ShowImageTranslationAsync(RectangleF area)
        {
            byte[] image;
            try
            {
                image = EnsureImageCapturer().CaptureScreen(area);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture region for image translation");
                Model.AddChatItem($"Failed to capture screen ({ex.Message})", TextTypes.Info);

                return;
            }

            ImageTranslationResult initial;
            try
            {
                initial = await _imageTranslationService.TranslateRegionAsync(image, null, _lastImageTarget);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image translation failed");
                Model.AddChatItem($"Image translation failed: {ex.Message}", TextTypes.Info);

                return;
            }

            var targetOptions = BuildTargetOptions();
            var sourceOptions = _imageTranslationService.GetAvailableSourceLanguages()
                .Select(s => new ImageTranslationOverlayWindow.SourceLanguageOption { Tag = s.Tag, Display = s.DisplayName })
                .ToList();

            var overlay = new ImageTranslationOverlayWindow(area, image, initial, targetOptions, sourceOptions, _lastImageTarget,
                (forcedTag, target) =>
                {
                    _lastImageTarget = target;
                    return _imageTranslationService.TranslateRegionAsync(image, forcedTag, target);
                });
            overlay.ShowDialog();
        }

        private IReadOnlyList<ImageTranslationOverlayWindow.TargetLanguageOption> BuildTargetOptions()
        {
            return _languageService.GetAll(includeTranslationOnly: true)
                .Select(descriptor => new ImageTranslationOverlayWindow.TargetLanguageOption
                {
                    Value = descriptor.Language,
                    Display = LocalizationManager.GetValue($"Str.Languages.{descriptor.Language}") ?? descriptor.Language.ToString()
                })
                .OrderBy(option => option.Display)
                .ToList();
        }

        private IScreenCapturer EnsureImageCapturer()
        {
            return _imageCapturer ??= _capturerFactory.CreateCapturer(true);
        }

        private void HotKeysManagerOnWindowStyleChangeKeyPressed(object sender, EventArgs e)
        {
            const int WS_EX_TRANSPARENT = 0x00000020;
            const int GWL_EXSTYLE = -20;

            IntPtr hwnd = _dialogService.GetWindowHandle<ChatWindowViewModel>();
            int extendedStyle = Win32Interfaces.GetWindowLong(hwnd, GWL_EXSTYLE);
            Win32Interfaces.SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle ^ WS_EX_TRANSPARENT);
            
            bool isLocked = (extendedStyle | WS_EX_TRANSPARENT) != extendedStyle;
            Model.AddChatItem(LocalizationManager.GetValue(isLocked ? "Str.Chat.WindowLocked" : "Str.Chat.WindowUnlocked"), TextTypes.Info);
        }

        private void OnShowHideSettings()
        {
            if (!_dialogService.CloseWindow<SettingsViewModel>())
            {
                Model.EndTranslation();
                var scope = _serviceProvider.CreateScope();
                var viewModel = scope.ServiceProvider.GetService<SettingsViewModel>();
                viewModel.HasUpdates = _hasUpdates;
                _dialogService.ShowWindowAsync(viewModel, () =>
                {
                    scope.Dispose();
                    GC.Collect(2);
                });
            }
        }

        private void OnShowHideChat()
        {
            ChatWindowIsVisible = !ChatWindowIsVisible;
            if (ChatWindowIsVisible)
            {
                StartTranslation(false);
            }
            else
            {
                Model.EndTranslation();
            }
        }

        private async void OnLoadedCommand()
        {
            SendHelpText();

            _hasUpdates = await _updateManager.CheckNewVersionAsync();
            if (_hasUpdates)
            {
                Model.AddChatItem(LocalizationManager.GetValue("Str.NewVersion"), TextTypes.Info);
            }
        }


        private async Task<BrowseSiteDispatchResult> BrowseSiteHandler(BrowseSiteDispatchArg argument)
        {
            _logger.LogTrace($"Web page requested (Target url: '{argument.TargetUrl}'; Proxy: {argument.Proxy?.Address})");
            var result = await WebBrowserProvider.BrowsePageAsync(argument.SourceUrl, argument.TargetUrl, CancellationToken.None,
                argument.Proxy, LocalizationManager.GetValue("Str.Notification.CaptchaPass", true));

            return new BrowseSiteDispatchResult()
            {
                HtmlPage = result?.Body,
                Cookies = result?.Cookies
            };
        }

        private void StartTranslation(bool showWarning)
        {
            if (_dialogService.WindowIsOpened<SettingsViewModel>())
            {
                if (showWarning)
                {
                    Model.AddChatItem(LocalizationManager.GetValue("Str.Chat.SettingsOpened"), TextTypes.Info);
                }

                return;
            }

            Model.StartTranslation();
        }

        private void SendHelpText()
        {
            string GetHotKeyHelpText(string hotKeyName, string localizationKey)
            {
                return string.Format(LocalizationManager.GetValue(localizationKey), _hotKeysServiceManager.GetRegisteredKeyCaption(hotKeyName));
            }

            var configuration = _hotKeysServiceManager.Configuration;
            Model.AddChatItem(LocalizationManager.GetValue("Str.Hotkeys.GeneralHelp"), TextTypes.Info);
            Model.AddChatItem(GetHotKeyHelpText(nameof(configuration.SettingVisibilityKey), "Str.Hotkeys.SettingsShowHelp"), TextTypes.Info);
            Model.AddChatItem(GetHotKeyHelpText(nameof(configuration.SelectAreaKey), "Str.Hotkeys.SelectAreaHelp"), TextTypes.Info);
            Model.AddChatItem(GetHotKeyHelpText(nameof(configuration.TranslationStateKey), "Str.Hotkeys.OnTranslationHelp"), TextTypes.Info);
        }
    }
}
