using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using MaterialDesignThemes.Wpf;
using Translumo.MVVM.ViewModels;
using Translumo.Utils;

namespace Translumo.Dialog
{
    public class DialogService : BindableBase
    {
        public bool AllowCloseOnClickAway
        {
            get => _allowCloseOnClickAway;
            set => SetProperty(ref _allowCloseOnClickAway, value);
        }

        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        private readonly Regex _viewModelReplaceRegex = new Regex("Model$", RegexOptions.Compiled);

        private bool _allowCloseOnClickAway;
        private bool _isOpen;
        private readonly ConcurrentDictionary<Type, Window> _openedWindows = new ConcurrentDictionary<Type, Window>();

        public async Task<MessageBoxResult?> ShowDialogAsync<TViewModel>(TViewModel dialogViewModel)
            where TViewModel : class
        {
            var view = await GetViewByViewModel<UserControl>(dialogViewModel);
            if (view == null)
            {
                throw new ArgumentException($"Appropriate dialog view is not found", nameof(dialogViewModel));
            }

            if (dialogViewModel is INonInteractionDialogViewModel nonInteractionVm)
            {
                if (nonInteractionVm.IsClosed)
                {
                    return default(MessageBoxResult);
                }

                AllowCloseOnClickAway = false;
                nonInteractionVm.DialogIsClosed += (sender, args) => IsOpen = false;
            }
            else
            {
                AllowCloseOnClickAway = true;
            }

            object dialogResult = await Application.Current.Dispatcher.Invoke(() => DialogHost.Show(view));

            return (MessageBoxResult?)dialogResult;
        }

        public async Task ShowWindowAsync<TViewModel>(TViewModel windowViewModel, Action onCloseCallback = null)
        {
            var view = await GetViewByViewModel<Window>(windowViewModel);
            if (view == null)
            {
                throw new ArgumentException($"Appropriate window view is not found", nameof(windowViewModel));
            }

            view.Closed += (sender, args) =>
            {
                ViewOnClosed(sender, args);
                onCloseCallback?.Invoke();
            };

            Application.Current.Dispatcher.Invoke(() => view.Show());
            _openedWindows[typeof(TViewModel)] = view;
        }

        public bool? ShowWindowDialog<TView>(out TView windowView, object param = null)
            where TView : Window
        {
            if (_openedWindows.ContainsKey(typeof(TView)))
            {
                windowView = null;
                return null;
            }

            windowView = param != null ? (TView)Activator.CreateInstance(typeof(TView), param) : Activator.CreateInstance<TView>();
            _openedWindows[typeof(TView)] = windowView;

            try
            {
                return windowView.ShowDialog();
            }
            finally
            {
                _openedWindows.TryRemove(typeof(TView), out _);
            }
        }

        public bool WindowIsOpened<TViewModel>()
        {
            return _openedWindows.ContainsKey(typeof(TViewModel));
        }

        public bool CloseWindow<TViewModel>()
        {
            if (WindowIsOpened<TViewModel>())
            {
                _openedWindows[typeof(TViewModel)].Close();

                return true;
            }

            return false;
        }

        public IntPtr GetWindowHandle<TViewModel>()
        {
            if (!_openedWindows.ContainsKey(typeof(TViewModel)))
            {
                return default(IntPtr);
            }

            return new WindowInteropHelper(_openedWindows[typeof(TViewModel)]).Handle;
        }

        private void ViewOnClosed(object sender, EventArgs e)
        {
            var view = sender as Window;
            if (view == null)
            {
                return;
            }

            var viewModelType = view.DataContext.GetType();
            _openedWindows.TryRemove(viewModelType, out var window);
        }

        private async Task<TView> GetViewByViewModel<TView>(object viewModel)
            where TView : FrameworkElement
        {
            Type viewModelType = viewModel.GetType();
            string viewTypeName = _viewModelReplaceRegex.Replace(viewModelType.Name, string.Empty);
            Type viewType = viewModelType.Assembly.GetTypes().FirstOrDefault(type => type.Name == viewTypeName);
            if (viewType == null)
            {
                throw new ArgumentException($"View for ViewModel '{viewModelType.Name}' is not found");
            }

            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var view = Activator.CreateInstance(viewType) as TView;
                if (view != null)
                {
                    view.DataContext = viewModel;
                }

                return view;
            });
        }
    }
}
