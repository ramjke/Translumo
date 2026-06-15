using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MaterialDesignExtensions.Model;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using Translumo.Configuration;
using Translumo.Dialog;
using Translumo.MVVM.Common;
using Translumo.Utils;
using RelayCommand = Translumo.MVVM.Common.RelayCommand;

namespace Translumo.MVVM.ViewModels
{
    public sealed class SettingsViewModel : BindableBase, IDisposable
    {
        public ObservableCollection<BaseNavigationItem> NavigationItems { get; set; }
        public object SelectedViewModel
        {
            get => _selectedViewModel;
            set => SetProperty(ref _selectedViewModel, value);
        }
        public bool AdditionPanelOpened
        {
            get => _additionalPanelOpened;
            set => SetProperty(ref _additionalPanelOpened, value);
        }
        public bool HasUpdates { get; set; }
        public CultureInfo[] AvailableLanguages { get; set; }

        public SystemConfiguration SystemConfiguration { get; set; }
        public DialogService DialogService { get; set; }
        public ICommand NavigationItemSelectedCommand => new RelayCommand<INavigationItem>(OnNavigationItemSelected);
        public ICommand AboutDialogOpenedCommand => new RelayCommand(OnAboutDialogOpened);

        private object _selectedViewModel;
        private bool _additionalPanelOpened;

        public SettingsViewModel(DialogService dialogService, AppearanceSettingsViewModel appearanceVm, OcrSettingsViewModel ocrVm,
            LanguagesSettingsViewModel languagesVm, HotkeysSettingsViewModel hotkeysVm, SystemConfiguration systemConfiguration, ILogger<SettingsViewModel> logger)
        {
            this.NavigationItems = new ObservableCollection<BaseNavigationItem>();
            this.DialogService = dialogService;
            this.SystemConfiguration = systemConfiguration;
            this.AvailableLanguages = LocalizationManager.AvailableLocalizations.ToArray();

            appearanceVm.PanelStateIsChanged += AppearanceVmOnPanelStateIsChanged;
            languagesVm.PanelStateIsChanged += AppearanceVmOnPanelStateIsChanged;

            AddNavigationItem(LocalizationManager.GetValue("Str.Navigation.Languages", false, OnLocalizedValueChanged, this), PackIconKind.Language, languagesVm);
            AddNavigationItem(LocalizationManager.GetValue("Str.Navigation.Appearance", false, OnLocalizedValueChanged, this), PackIconKind.PaletteOutline, appearanceVm);
            AddNavigationItem(LocalizationManager.GetValue("Str.Navigation.Ocr", false, OnLocalizedValueChanged, this), PackIconKind.Ocr, ocrVm);
            AddNavigationItem(LocalizationManager.GetValue("Str.Navigation.HotKeys", false, OnLocalizedValueChanged, this), PackIconKind.KeyboardSettingsOutline, hotkeysVm);
        }

        private void OnLocalizedValueChanged(string key, string oldValue)
        {
            var navItem = NavigationItems.FirstOrDefault(item => ((NavigationItem)item).Label == oldValue) as NavigationItem;
            if (navItem != null)
            {
                navItem.Label = LocalizationManager.GetValue(key, false, OnLocalizedValueChanged, this);
            }
        }

        private void AppearanceVmOnPanelStateIsChanged(object sender, bool e)
        {
            AdditionPanelOpened = e;
        }

        private void AddNavigationItem(string label, PackIconKind icon, object viewModel)
        {
            NavigationItems.Add(new FirstLevelNavigationItem() { Label = label, Icon = icon, IsSelectable = true, NavigationItemSelectedCallback = item => viewModel });
        }

        private void OnNavigationItemSelected(INavigationItem navigationItem)
        {
            var callbackResult = navigationItem?.NavigationItemSelectedCallback(navigationItem);
            if (callbackResult?.GetType() != SelectedViewModel?.GetType())
            {
                if (SelectedViewModel is IAdditionalPanelController vmController)
                {
                    vmController.ClosePanel();
                }

                SelectedViewModel = callbackResult;
            }
        }

        private async void OnAboutDialogOpened()
        {
            await DialogService.ShowDialogAsync(new AboutDialogViewModel() { HasUpdates = HasUpdates });
        }

        public void Dispose()
        {
            LocalizationManager.ReleaseChangedValuesCallbacks(this);
        }
    }
}
