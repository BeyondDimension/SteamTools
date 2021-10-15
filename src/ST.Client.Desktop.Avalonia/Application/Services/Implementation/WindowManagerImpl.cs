using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System.Application.UI;
using System.Application.UI.ViewModels;
using System.Application.UI.Views;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaApplication = Avalonia.Application;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="IWindowManager"/>
    internal sealed class WindowManagerImpl : IWindowManagerImpl
    {
        readonly IAvaloniaApplication app;

        public WindowManagerImpl(IAvaloniaApplication app)
        {
            this.app = app;
        }

        Type IWindowManagerImpl.WindowType => typeof(Window);

        Type GetWindowType(CustomWindow customWindow)
        {
            IWindowManagerImpl @this = this;
            return @this.GetWindowType(customWindow, typeof(MainView).Assembly);
        }

        Type GetWindowViewModelType(CustomWindow customWindow)
        {
            IWindowManagerImpl @this = this;
            return @this.GetWindowViewModelType(customWindow, typeof(WindowViewModel).Assembly);
        }

        static bool IsSingletonWindow(CustomWindow customWindow) => customWindow switch
        {
            CustomWindow.MessageBox or
            CustomWindow.TextBox or
            CustomWindow.AuthTrade or
            CustomWindow.ShowAuth => false,
            _ => true,
        };

        static bool TryShowSingletonWindow(Type windowType)
        {
            if (AvaloniaApplication.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = desktop.Windows.FirstOrDefault(x => x.GetType() == windowType);
                if (window != null)
                {
                    window.Show();
                    if (window.WindowState == WindowState.Minimized)
                        window.WindowState = WindowState.Normal;
                    window.Activate();
                    return true;
                }
            }
            return false;
        }

        Task Show(Type typeWindowViewModel,
            bool isDialog,
            CustomWindow customWindow,
            string title,
            WindowViewModel? viewModel,
            ResizeMode resizeMode,
            bool isParent = true,
            Action<DialogWindowViewModel>? actionDialogWindowViewModel = null)
            => MainThread2.InvokeOnMainThreadAsync(async () =>
            {
                var windowType = GetWindowType(customWindow);
                if (IsSingletonWindow(customWindow) && TryShowSingletonWindow(windowType))
                {
                    return;
                }
                var window = (Window?)Activator.CreateInstance(windowType);
                if (window == null) return;
                if (viewModel == null && typeWindowViewModel != typeof(object))
                {
                    viewModel = (WindowViewModel?)Activator.CreateInstance(typeWindowViewModel);
                }
                if (!string.IsNullOrEmpty(title) && viewModel != null)
                {
                    viewModel.Title = title;
                }
                if (isParent)
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.SetResizeMode(resizeMode);
                if (typeof(DialogWindowViewModel).IsAssignableFrom(typeWindowViewModel))
                {
                    static void BindingDialogWindowViewModel(Window window, DialogWindowViewModel dialogWindowViewModel, Action<DialogWindowViewModel>? actionDialogWindowViewModel)
                    {
                        actionDialogWindowViewModel?.Invoke(dialogWindowViewModel);
                        if (dialogWindowViewModel.OK == null)
                        {
                            dialogWindowViewModel.OK = ReactiveCommand.Create(() =>
                            {
                                dialogWindowViewModel.DialogResult = true;
                                if (dialogWindowViewModel.OnOKClickCanClose())
                                {
                                    window.Close();
                                }
                            });
                        }
                        if (dialogWindowViewModel.Cancel == null)
                        {
                            dialogWindowViewModel.Cancel = ReactiveCommand.Create(() =>
                            {
                                dialogWindowViewModel.DialogResult = false;
                                window.Close();
                            });
                        }
                    }
                    if (viewModel == null)
                    {
                        void Window_DataContextChanged(object? _, EventArgs __)
                        {
                            if (window.DataContext is DialogWindowViewModel dialogWindowViewModel)
                            {
                                BindingDialogWindowViewModel(window, dialogWindowViewModel, actionDialogWindowViewModel);
                            }
                        }
                        void Window_Closed(object? _, EventArgs __)
                        {
                            window.DataContextChanged -= Window_DataContextChanged;
                            window.Closed -= Window_Closed;
                        }
                        window.DataContextChanged += Window_DataContextChanged;
                        window.Closed += Window_Closed;
                    }
                    else if (viewModel is DialogWindowViewModel dialogWindowViewModel)
                    {
                        BindingDialogWindowViewModel(window, dialogWindowViewModel, actionDialogWindowViewModel);
                    }
                }
                if (viewModel != null) window.DataContext = viewModel;
                try
                {
                    if (isDialog)
                    {
                        await app.ShowDialogWindow(window);
                    }
                    else
                    {
                        if (isParent)
                            app.ShowWindow(window);
                        else
                            app.ShowWindowNoParent(window);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(nameof(WindowManagerImpl), e,
                        "Show fail, windowType: {0}", window?.GetType().Name);
                }
            });

        public Task Show<TWindowViewModel>(
            CustomWindow customWindow,
            TWindowViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = false,
            bool isParent = true)
            where TWindowViewModel : WindowViewModel, new() => Show(typeof(TWindowViewModel),
                customWindow, viewModel, title, resizeMode, isDialog, isParent);

        public Task Show(Type typeWindowViewModel,
            CustomWindow customWindow,
            WindowViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = false,
            bool isParent = true) => Show(typeWindowViewModel, isDialog, customWindow,
                title, viewModel, resizeMode, isParent);

        public Task Show(CustomWindow customWindow,
           WindowViewModel? viewModel = null,
           string title = "",
           ResizeMode resizeMode = ResizeMode.NoResize,
           bool isDialog = false,
           bool isParent = true) => Show(GetWindowViewModelType(customWindow), isDialog, customWindow,
                title, viewModel, resizeMode, isParent);

        public async Task<bool> ShowDialog<TWindowViewModel>(
            CustomWindow customWindow,
            TWindowViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = true,
            bool isParent = true)
            where TWindowViewModel : WindowViewModel, new()
        {
            DialogWindowViewModel? dialogWindowViewModel = null;
            await Show(typeof(TWindowViewModel), isDialog, customWindow,
                title, viewModel, resizeMode, isParent, dwvm =>
            {
                dialogWindowViewModel = dwvm;
            });
            return dialogWindowViewModel?.DialogResult ?? false;
        }

        public Task ShowDialog(Type typeWindowViewModel,
                CustomWindow customWindow,
                WindowViewModel? viewModel = null,
                string title = "",
                ResizeMode resizeMode = ResizeMode.NoResize,
                bool isDialog = true) => Show(typeWindowViewModel, isDialog, customWindow,
                    title, viewModel, resizeMode);

        public Task ShowDialog(CustomWindow customWindow,
            WindowViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = true) => Show(GetWindowViewModelType(customWindow), isDialog, customWindow,
                title, viewModel, resizeMode);


        public void CloseWindow(WindowViewModel vm)
        {
            try
            {
                app.CloseWindow(vm);
            }
            catch (Exception e)
            {
                Log.Error(nameof(WindowManagerImpl), e,
                    "CloseWindow fail, vmType: {0}", vm.GetType().Name);
            }
        }

        public bool IsVisibleWindow(WindowViewModel vm)
        {
            try
            {
                return app.IsVisibleWindow(vm);
            }
            catch (Exception e)
            {
                Log.Error(nameof(WindowManagerImpl), e,
                    "HideWindow fail, vmType: {0}", vm.GetType().Name);
                return false;
            }
        }

        public void HideWindow(WindowViewModel vm)
        {
            try
            {
                app.HideWindow(vm);
            }
            catch (Exception e)
            {
                Log.Error(nameof(WindowManagerImpl), e,
                    "HideWindow fail, vmType: {0}", vm.GetType().Name);
            }
        }

        public void ShowWindow(WindowViewModel vm)
        {
            try
            {
                app.ShowWindowNoParent(vm);
            }
            catch (Exception e)
            {
                Log.Error(nameof(WindowManagerImpl), e,
                    "ShowWindowNoParent fail, vmType: {0}", vm.GetType().Name);
            }
        }
    }
}