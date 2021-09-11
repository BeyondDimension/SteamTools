using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System.Application.UI.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AvaloniaApplication = Avalonia.Application;

namespace System.Application.Services.Implementation
{
    internal sealed class ShowWindowServiceImpl : IShowWindowService
    {
        readonly IDesktopAvaloniaAppService daas;

        public ShowWindowServiceImpl(IDesktopAvaloniaAppService daas)
        {
            this.daas = daas;
        }


        static Type GetWindowType(CustomWindow customWindow)
        {
            var windowType = Type.GetType($"System.Application.UI.Views.Windows.{customWindow}Window");
            if (windowType != null && typeof(Window).IsAssignableFrom(windowType)) return windowType;
            throw new ArgumentOutOfRangeException(nameof(customWindow), customWindow, null);
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
            ResizeModeCompat resizeMode,
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
                        await daas.ShowDialogWindow(window);
                    }
                    else
                    {
                        if (isParent)
                            daas.ShowWindow(window);
                        else
                            daas.ShowWindowNoParent(window);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(nameof(ShowWindowServiceImpl), e,
                        "Show fail, windowType: {0}", window?.GetType().Name);
                }
            });

        public Task Show<TWindowViewModel>(
            CustomWindow customWindow,
            TWindowViewModel? viewModel = null,
            string title = "",
            ResizeModeCompat resizeMode = ResizeModeCompat.NoResize,
            bool isDialog = false,
            bool isParent = true)
            where TWindowViewModel : WindowViewModel, new() => Show(typeof(TWindowViewModel),
                customWindow, viewModel, title, resizeMode, isDialog, isParent);

        public Task Show(Type typeWindowViewModel,
            CustomWindow customWindow,
            WindowViewModel? viewModel = null,
            string title = "",
            ResizeModeCompat resizeMode = ResizeModeCompat.NoResize,
            bool isDialog = false,
            bool isParent = true) => Show(typeWindowViewModel, isDialog, customWindow,
                title, viewModel, resizeMode, isParent);

        public async Task<bool> ShowDialog<TWindowViewModel>(
            CustomWindow customWindow,
            TWindowViewModel? viewModel = null,
            string title = "",
            ResizeModeCompat resizeMode = ResizeModeCompat.NoResize,
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
                ResizeModeCompat resizeMode = ResizeModeCompat.NoResize,
                bool isDialog = true) => Show(typeWindowViewModel, isDialog, customWindow,
            title, viewModel, resizeMode);


        public void CloseWindow(WindowViewModel vm)
        {
            try
            {
                daas.CloseWindow(vm);
            }
            catch (Exception e)
            {
                Log.Error(nameof(ShowWindowServiceImpl), e,
                    "CloseWindow fail, vmType: {0}", vm.GetType().Name);
            }
        }

        public bool IsVisibleWindow(WindowViewModel vm)
        {
            try
            {
                return daas.IsVisibleWindow(vm);
            }
            catch (Exception e)
            {
                Log.Error(nameof(ShowWindowServiceImpl), e,
                    "HideWindow fail, vmType: {0}", vm.GetType().Name);
                return false;
            }
        }

        public void HideWindow(WindowViewModel vm)
        {
            try
            {
                daas.HideWindow(vm);
            }
            catch (Exception e)
            {
                Log.Error(nameof(ShowWindowServiceImpl), e,
                    "HideWindow fail, vmType: {0}", vm.GetType().Name);
            }
        }

        public void ShowWindow(WindowViewModel vm)
        {
            try
            {
                daas.ShowWindowNoParent(vm);
            }
            catch (Exception e)
            {
                Log.Error(nameof(ShowWindowServiceImpl), e,
                    "ShowWindowNoParent fail, vmType: {0}", vm.GetType().Name);
            }
        }
    }
}