using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SharpDX.XInput;
using Translumo.Services;
using Translumo.Utils.Extensions;

namespace Translumo.HotKeys
{
    public class HotKeysServiceManager
    {
        public event EventHandler SelectAreaKeyPressed;
        public event EventHandler ChatVisibilityKeyPressed;
        public event EventHandler TranslationStateKeyPressed;
        public event EventHandler SettingVisibilityKeyPressed;
        public event EventHandler ShowSelectionAreaKeyPressed;
        public event EventHandler OnceTranslateKeyPressed;
        public event EventHandler ImageTranslateKeyPressed;
        public event EventHandler WindowStyleChangeKeyPressed;

        public HotKeysConfiguration Configuration { get; }
        public bool GamepadHotkeysEnabled { get; }

        private readonly IControllerInputProvider _controllerInputProvider;
        private readonly IDictionary<string, HotKey> _registeredHotKeys;
        private readonly IDictionary<string, GamepadHotKey> _registeredGamepadHotKeys;
        private readonly IEnumerable<(string keyActionName, string gamepadActionName)> _keyNamesLink;

        public HotKeysServiceManager(HotKeysConfiguration configuration, IControllerInputProvider controllerInputProvider, IControllerService controllerService)
        {
            this._registeredHotKeys = InitializeHotKeys(configuration);
            this._registeredGamepadHotKeys = new Dictionary<string, GamepadHotKey>();
            this._controllerInputProvider = controllerInputProvider;
            this.Configuration = configuration;
            this.GamepadHotkeysEnabled = controllerService.TryChangeListenState(true);
            this._keyNamesLink = new[]
            {
                (nameof(configuration.ChatVisibilityKey), nameof(configuration.ChatVisibilityGamepadKey)),
                (nameof(configuration.SelectAreaKey), nameof(configuration.SelectAreaGamepadKey)),
                (nameof(configuration.TranslationStateKey), nameof(configuration.TranslationStateGamepadKey)),
                (nameof(configuration.SettingVisibilityKey), nameof(configuration.SettingVisibilityGamepadKey)),
                (nameof(configuration.ShowSelectionAreaKey), nameof(configuration.ShowSelectionAreaGamepadKey)),
                (nameof(configuration.OnceTranslateKey), nameof(configuration.OnceTranslateGamepadKey)),
                (nameof(configuration.ImageTranslateKey), nameof(configuration.ImageTranslateGamepadKey)),
                (nameof(configuration.WindowStyleChangeKey), nameof(configuration.WindowStyleChangeGamepadKey)),
            };

            if (GamepadHotkeysEnabled)
            {
                this._registeredGamepadHotKeys = InitializeGamepadHotKeys(configuration);
                _registeredGamepadHotKeys.ForEach(key =>
                {
                    RegisterHotKey(key.Key);
                    SuspendAssociativeHotKey(key.Key);
                });
            }

            this.Configuration.PropertyChanged += ConfigurationOnPropertyChanged;
        }

        public void RegisterAll()
        {
            _registeredHotKeys.ForEach(key => RegisterHotKey(key.Key));
            if (GamepadHotkeysEnabled)
            {
                _registeredGamepadHotKeys.ForEach(key => RegisterHotKey(key.Key));
            }
        }

        public void UnregisterAll()
        {
            _registeredHotKeys.ForEach(key => UnregisterHotKey(key.Key));
            if (GamepadHotkeysEnabled)
            {
                _registeredGamepadHotKeys.ForEach(key => UnregisterHotKey(key.Key));
            }
        }

        public void UnregisterHotKey(string keyActionName)
        {
            if (_registeredHotKeys.ContainsKey(keyActionName))
            {
                _registeredHotKeys[keyActionName].Unregister();
            }
            else if (_registeredGamepadHotKeys.ContainsKey(keyActionName))
            {
                _controllerInputProvider.UnregisterHotKey(_registeredGamepadHotKeys[keyActionName]);
            }
        }

        public void RegisterHotKey(string keyActionName)
        {
            if (_registeredHotKeys.ContainsKey(keyActionName))
            {
                _registeredHotKeys[keyActionName].Register();
            }
            else if (_registeredGamepadHotKeys.ContainsKey(keyActionName))
            {
                _controllerInputProvider.RegisterHotKey(_registeredGamepadHotKeys[keyActionName]);
            }
        }

        public string GetRegisteredKeyCaption(string keyActionName)
        {
            if (GamepadHotkeysEnabled)
            {
                var gpKeyActionName = GetAssociativeGamepadHotKey(keyActionName);
                var gpHotKey = Configuration.GetType().GetProperty(gpKeyActionName)?.GetValue(Configuration) as GamepadHotKeyInfo;
                if ((gpHotKey?.Key ?? GamepadKeyCode.None) != GamepadKeyCode.None)
                {
                    return gpHotKey.ToString();
                }
            }

            var hotKey = Configuration.GetType().GetProperty(keyActionName)?.GetValue(Configuration) as HotKeyInfo;

            return hotKey?.ToString();
        }

        private void ConfigurationOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.PropertyName))
            {
                var newValueProperty = typeof(HotKeysConfiguration).GetProperty(e.PropertyName);
                if (_registeredHotKeys.ContainsKey(e.PropertyName))
                {
                    var newValue = newValueProperty.GetValue(Configuration) as HotKeyInfo;

                    var gamepadKeyActionName = GetAssociativeGamepadHotKey(e.PropertyName);
                    var forceSuspend = !_registeredGamepadHotKeys.ContainsKey(gamepadKeyActionName)  || _registeredGamepadHotKeys[gamepadKeyActionName].KeyCode == GamepadKeyCode.None;
                    _registeredHotKeys[e.PropertyName].Reassign(newValue.Key, newValue.KeyModifier, forceSuspend);
                }
                else if (_registeredGamepadHotKeys.ContainsKey(e.PropertyName))
                {
                    var newValue = newValueProperty.GetValue(Configuration) as GamepadHotKeyInfo;
                    _registeredGamepadHotKeys[e.PropertyName].KeyCode = newValue.Key;
                    _controllerInputProvider.ReassignHotKey(_registeredGamepadHotKeys[e.PropertyName]);
                    SuspendAssociativeHotKey(e.PropertyName);
                }
            }
        }

        private void SuspendAssociativeHotKey(string gamepadKeyActionName)
        {
            if (_registeredGamepadHotKeys[gamepadKeyActionName].KeyCode == GamepadKeyCode.None)
            {
                return;
            }

            var hotkeyActionName = GetAssociativeHotKey(gamepadKeyActionName);
            if (_registeredHotKeys.ContainsKey(hotkeyActionName))
            {
                _registeredHotKeys[hotkeyActionName].Suspend();
            }
        }

        private string GetAssociativeHotKey(string gamepadKeyActionName)
        {
            return _keyNamesLink.First(name => name.gamepadActionName == gamepadKeyActionName)
                .keyActionName;
        }

        private string GetAssociativeGamepadHotKey(string keyActionName)
        {
            return _keyNamesLink.First(name => name.keyActionName == keyActionName)
                .gamepadActionName;
        }

        private void OnTranslationStatePressed()
        {
            TranslationStateKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectAreaPressed()
        {
            SelectAreaKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        private void OnChatVisibilityPressed()
        {
            ChatVisibilityKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        private void OnSettingVisibilityPressed()
        {
            SettingVisibilityKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        private void OnShowSelectionAreaPressed()
        {
            ShowSelectionAreaKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        private void OnOnceTranslatePressed()
        {
            OnceTranslateKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        private void OnImageTranslatePressed()
        {
            ImageTranslateKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        private void OnWindowStyleChangePressed()
        {
            WindowStyleChangeKeyPressed?.Invoke(this, EventArgs.Empty);
        }

        private IDictionary<string, HotKey> InitializeHotKeys(HotKeysConfiguration configuration)
        {
            return new Dictionary<string, HotKey>()
            {
                {
                    nameof(configuration.ChatVisibilityKey), new HotKey(configuration.ChatVisibilityKey.Key, 
                        configuration.ChatVisibilityKey.KeyModifier, OnChatVisibilityPressed)
                },
                {
                    nameof(configuration.SelectAreaKey), new HotKey(configuration.SelectAreaKey.Key,
                        configuration.SelectAreaKey.KeyModifier, OnSelectAreaPressed)
                },
                {
                    nameof(configuration.TranslationStateKey), new HotKey(configuration.TranslationStateKey.Key,
                        configuration.TranslationStateKey.KeyModifier, OnTranslationStatePressed)
                },
                {
                    nameof(configuration.SettingVisibilityKey), new HotKey(configuration.SettingVisibilityKey.Key,
                        configuration.SettingVisibilityKey.KeyModifier, OnSettingVisibilityPressed)
                },
                {
                    nameof(configuration.ShowSelectionAreaKey), new HotKey(configuration.ShowSelectionAreaKey.Key,
                        configuration.ShowSelectionAreaKey.KeyModifier, OnShowSelectionAreaPressed)
                },
                {
                    nameof(configuration.OnceTranslateKey), new HotKey(configuration.OnceTranslateKey.Key,
                        configuration.OnceTranslateKey.KeyModifier, OnOnceTranslatePressed)
                },
                {
                    nameof(configuration.ImageTranslateKey), new HotKey(configuration.ImageTranslateKey.Key,
                        configuration.ImageTranslateKey.KeyModifier, OnImageTranslatePressed)
                },
                {
                    nameof(configuration.WindowStyleChangeKey), new HotKey(configuration.WindowStyleChangeKey.Key,
                        configuration.WindowStyleChangeKey.KeyModifier, OnWindowStyleChangePressed)
                }
            };
        }

        private IDictionary<string, GamepadHotKey> InitializeGamepadHotKeys(HotKeysConfiguration configuration)
        {
            return new Dictionary<string, GamepadHotKey>()
            {
                { nameof(configuration.ChatVisibilityGamepadKey), new GamepadHotKey(configuration.ChatVisibilityGamepadKey.Key, OnChatVisibilityPressed) },
                { nameof(configuration.SelectAreaGamepadKey), new GamepadHotKey(configuration.SelectAreaGamepadKey.Key, OnSelectAreaPressed) },
                { nameof(configuration.TranslationStateGamepadKey), new GamepadHotKey(configuration.TranslationStateGamepadKey.Key, OnTranslationStatePressed) },
                { nameof(configuration.SettingVisibilityGamepadKey), new GamepadHotKey(configuration.SettingVisibilityGamepadKey.Key, OnSettingVisibilityPressed) },
                { nameof(configuration.ShowSelectionAreaGamepadKey), new GamepadHotKey(configuration.ShowSelectionAreaGamepadKey.Key, OnShowSelectionAreaPressed) },
                { nameof(configuration.OnceTranslateGamepadKey), new GamepadHotKey(configuration.OnceTranslateGamepadKey.Key, OnOnceTranslatePressed) },
                { nameof(configuration.ImageTranslateGamepadKey), new GamepadHotKey(configuration.ImageTranslateGamepadKey.Key, OnImageTranslatePressed) },
                { nameof(configuration.WindowStyleChangeGamepadKey), new GamepadHotKey(configuration.WindowStyleChangeGamepadKey.Key, OnWindowStyleChangePressed) }
            };
        }
    }
}
