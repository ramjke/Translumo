using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using SharpDX.XInput;
using Translumo.HotKeys;
using Translumo.MVVM.Models;
using Translumo.Utils;
using Translumo.Utils.Extensions;

namespace Translumo.MVVM.ViewModels
{
    public class HotkeysSettingsViewModel : IDisposable
    {
        public IList<HotKeyModel> Model { get; set; }

        public ICommand ExitEditModeCommand => new ActionCommand(OnExitEditMode);
        public ICommand EnterEditModeCommand => new ActionCommand(OnEnterEditMode);

        private readonly HotKeysConfiguration _configuration;
        private readonly HotKeysServiceManager _serviceManager;

        public HotkeysSettingsViewModel(HotKeysServiceManager hotKeysServiceManager)
        {
            this._configuration = hotKeysServiceManager.Configuration;
            this._serviceManager = hotKeysServiceManager;

            var defaultGamepadHotKey = new GamepadHotKeyInfo(GamepadKeyCode.None);
            Model = new List<HotKeyModel>(new[]
            {
                new HotKeyModel(_configuration.ChatVisibilityKey,
                    hotKeysServiceManager.GamepadHotkeysEnabled ? _configuration.ChatVisibilityGamepadKey : defaultGamepadHotKey,
                    nameof(_configuration.ChatVisibilityKey), nameof(_configuration.ChatVisibilityGamepadKey),
                    LocalizationManager.GetValue("Str.Hotkeys.ChatVisibility", false, OnLocalizedValueChanged, this)),
                new HotKeyModel(_configuration.SettingVisibilityKey,
                    hotKeysServiceManager.GamepadHotkeysEnabled ? _configuration.SettingVisibilityGamepadKey : defaultGamepadHotKey,
                    nameof(_configuration.SettingVisibilityKey), nameof(_configuration.SettingVisibilityGamepadKey),
                    LocalizationManager.GetValue("Str.Hotkeys.SettingsVisibility", false, OnLocalizedValueChanged, this)),
                new HotKeyModel(_configuration.SelectAreaKey,
                    hotKeysServiceManager.GamepadHotkeysEnabled ? _configuration.SelectAreaGamepadKey : defaultGamepadHotKey,
                    nameof(_configuration.SelectAreaKey), nameof(_configuration.SelectAreaGamepadKey),
                    LocalizationManager.GetValue("Str.Hotkeys.SelectArea", false, OnLocalizedValueChanged, this)),
                new HotKeyModel(_configuration.TranslationStateKey,
                    hotKeysServiceManager.GamepadHotkeysEnabled ? _configuration.TranslationStateGamepadKey : defaultGamepadHotKey,
                    nameof(_configuration.TranslationStateKey), nameof(_configuration.TranslationStateGamepadKey),
                    LocalizationManager.GetValue("Str.Hotkeys.OnOffTranslation", false, OnLocalizedValueChanged, this)),
                new HotKeyModel(_configuration.ShowSelectionAreaKey,
                    hotKeysServiceManager.GamepadHotkeysEnabled ? _configuration.ShowSelectionAreaGamepadKey : defaultGamepadHotKey,
                    nameof(_configuration.ShowSelectionAreaKey), nameof(_configuration.ShowSelectionAreaGamepadKey),
                    LocalizationManager.GetValue("Str.Hotkeys.ShowSelectArea", false, OnLocalizedValueChanged, this)),
                new HotKeyModel(_configuration.OnceTranslateKey,
                    hotKeysServiceManager.GamepadHotkeysEnabled ? _configuration.OnceTranslateGamepadKey : defaultGamepadHotKey,
                    nameof(_configuration.OnceTranslateKey), nameof(_configuration.OnceTranslateGamepadKey),
                    LocalizationManager.GetValue("Str.Hotkeys.OnceTranslate", false, OnLocalizedValueChanged, this)),
                new HotKeyModel(_configuration.ImageTranslateKey,
                    hotKeysServiceManager.GamepadHotkeysEnabled ? _configuration.ImageTranslateGamepadKey : defaultGamepadHotKey,
                    nameof(_configuration.ImageTranslateKey), nameof(_configuration.ImageTranslateGamepadKey),
                    LocalizationManager.GetValue("Str.Hotkeys.ImageTranslate", false, OnLocalizedValueChanged, this)),
                new HotKeyModel(_configuration.WindowStyleChangeKey,
                    hotKeysServiceManager.GamepadHotkeysEnabled ? _configuration.WindowStyleChangeGamepadKey : defaultGamepadHotKey,
                    nameof(_configuration.WindowStyleChangeKey), nameof(_configuration.WindowStyleChangeGamepadKey),
                    LocalizationManager.GetValue("Str.Hotkeys.WindowChangeStyle", false, OnLocalizedValueChanged, this))
            });

            Model.ForEach(m => m.PropertyChanged += OnPropertyChanged);
        }

        private void OnExitEditMode()
        {
            _serviceManager.RegisterAll();
        }

        private void OnEnterEditMode()
        {
            _serviceManager.UnregisterAll();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = sender as HotKeyModel;
            if (model == null)
            {
                return;
            }

            if (e.PropertyName == nameof(HotKeyModel.HotKey))
            {
                UpdateTargetHotKey(model);
            }
            else if (e.PropertyName == nameof(HotKeyModel.GamepadHotKey))
            {
                UpdateTargetGamepadHotKey(model);
            }
        }

        private void UpdateTargetHotKey(HotKeyModel model)
        {
            var targetPropInfo = _configuration.GetType().GetProperty(model.ConfigurationPropertyName);
            var sameKeyModel = Model.FirstOrDefault(m => m.HotKey.Equals(model.HotKey) && m != model);
            if (sameKeyModel != null)
            {
                _serviceManager.UnregisterHotKey(model.ConfigurationPropertyName);
                sameKeyModel.HotKey = targetPropInfo.GetValue(_configuration) as HotKeyInfo;
            }

            targetPropInfo.SetValue(_configuration, model.HotKey);
        }

        private void UpdateTargetGamepadHotKey(HotKeyModel model)
        {
            var targetPropInfo = _configuration.GetType().GetProperty(model.GamepadConfigurationPropertyName);
            var sameKeyModel = Model.FirstOrDefault(m => m.GamepadHotKey.Equals(model.GamepadHotKey) && m != model);
            if (sameKeyModel != null && model.GamepadHotKey.Key != GamepadKeyCode.None)
            {
                var oldValue = targetPropInfo.GetValue(_configuration) as GamepadHotKeyInfo;
                sameKeyModel.GamepadHotKey = oldValue;
                if (oldValue.Key == GamepadKeyCode.None)
                {
                    sameKeyModel.HotKey = model.HotKey;
                }
            }

            targetPropInfo.SetValue(_configuration, model.GamepadHotKey);
        }

        private void OnLocalizedValueChanged(string key, string oldValue)
        {
            var displayHotKey = Model.First(hk => hk.Description == oldValue);
            displayHotKey.Description = LocalizationManager.GetValue(key, false, OnLocalizedValueChanged, this);
        }

        public void Dispose()
        {
            LocalizationManager.ReleaseChangedValuesCallbacks(this);
        }
    }
}
