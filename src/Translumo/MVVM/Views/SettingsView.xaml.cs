using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Translumo.Infrastructure.Language;
using Translumo.MVVM.ViewModels;
using Translumo.Services;
using Translumo.Utils;

namespace Translumo.MVVM.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        private SettingsViewModel ViewModel => DataContext as SettingsViewModel;

        private Storyboard _toOpenStoryboard;
        private Storyboard _toCloseStoryboard;

        public SettingsView()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _toOpenStoryboard = CreateStoryboardToOpenWindow(true);
            _toCloseStoryboard = CreateStoryboardToOpenWindow(false);

            this.Activate();
        }

        private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsViewModel.AdditionPanelOpened))
            {
                if (ViewModel.AdditionPanelOpened)
                {
                    this.BeginStoryboard(_toOpenStoryboard, HandoffBehavior.Compose, true);
                }
                else
                {
                    this.BeginStoryboard(_toCloseStoryboard, HandoffBehavior.Compose, true);
                }
            }
        }

        private Storyboard CreateStoryboardToOpenWindow(bool toOpen)
        {
            const int ADD_WINDOW_WIDTH = 205;
            DoubleAnimation widthAnimation = new DoubleAnimation
            {
                To = this.ActualWidth + (toOpen ? ADD_WINDOW_WIDTH : 0),
                From = this.ActualWidth + (toOpen ? 0 : ADD_WINDOW_WIDTH),
                Duration = TimeSpan.FromMilliseconds(500)
            };

            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(Window.WidthProperty));
            Storyboard.SetTarget(widthAnimation, this);

            var storyboard = new Storyboard();
            storyboard.Children.Add(widthAnimation);

            return storyboard;
        }

        private void wSettings_SourceInitialized(object sender, EventArgs e)
        {
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void wSettings_Activated(object sender, EventArgs e)
        {
            //this.Topmost = true;
        }

        private void wSettings_Deactivated(object sender, EventArgs e)
        {
            //this.Topmost = true;
            //this.Activate();
        }

        private void wSettings_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void SnMenu_Loaded(object sender, RoutedEventArgs e)
        {
            var sideNav = sender as MaterialDesignExtensions.Controls.SideNavigation;
            if (sideNav == null) return;

            // Check if there is at least one item
            if (sideNav.Items.Count > 0)
            {
                // Set the first item as selected
                sideNav.SelectedItem = (MaterialDesignExtensions.Model.INavigationItem)sideNav.Items[0];
            }
        }

        private void OnLookupperLinkClick(object sender, RoutedEventArgs e)
        {
            var destinationurl = LocalizationManager.GetValue($"Str.Lookupper.Url");
            var sInfo = new System.Diagnostics.ProcessStartInfo(destinationurl)
            {
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(sInfo);
        }
    }
}
