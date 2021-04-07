using Avalonia.Controls;
using ReactiveUI;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Windows;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.Services.Implementation
{
    internal sealed class ShowWindowServiceImpl : IShowWindowService
    {
        static Type GetWindowType(CustomWindow customWindow) => customWindow switch
        {
            CustomWindow.MessageBox => typeof(MessageBoxWindow),
            CustomWindow.LoginOrRegister => typeof(LoginOrRegisterWindow),
            CustomWindow.AddAuth => typeof(AddAuthWindow),
            CustomWindow.ShowAuth => typeof(ShowAuthWindow),
            CustomWindow.AuthTrade => typeof(AuthTradeWindow),
            _ => throw new ArgumentOutOfRangeException(nameof(customWindow), customWindow, null),
        };

        Task Show<TWindowViewModel>(
            bool isDialog,
            CustomWindow customWindow,
            string title,
            TWindowViewModel viewModel,
            ResizeModeCompat resizeMode,
            Action<Window>? action = null)
            where TWindowViewModel : WindowViewModel, new() => MainThreadDesktop.InvokeOnMainThreadAsync(async () =>
           {
               var windowType = GetWindowType(customWindow);
               var window = (Window)Activator.CreateInstance(windowType);
               if (!string.IsNullOrWhiteSpace(title))
                   viewModel.Title = title;
               window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
               window.SetResizeMode(resizeMode);
               action?.Invoke(window);
               window.DataContext = viewModel;
               if (isDialog)
               {
                   await IDesktopAvaloniaAppService.Instance.ShowDialogWindow(window);
               }
               else
               {
                   IDesktopAvaloniaAppService.Instance.ShowWindow(window);
               }
           });

        public Task Show<TWindowViewModel>(
            CustomWindow customWindow,
            TWindowViewModel? viewModel = null,
            string title = "",
            ResizeModeCompat resizeMode = ResizeModeCompat.NoResize)
            where TWindowViewModel : WindowViewModel,new()
        {
            viewModel ??= new TWindowViewModel();
            return Show(false, customWindow, title, viewModel, resizeMode);
        }

        public async Task<bool> ShowDialog<TDialogWindowViewModel>(
            CustomWindow customWindow,
            TDialogWindowViewModel? viewModel = null,
            string title = "",
            ResizeModeCompat resizeMode = ResizeModeCompat.NoResize)
            where TDialogWindowViewModel : DialogWindowViewModel, new()
        {
            viewModel ??= new TDialogWindowViewModel();
            await Show(true, customWindow, title, viewModel, resizeMode, window =>
            {
                viewModel.OK = ReactiveCommand.Create(() =>
                {
                    viewModel.DialogResult = true;
                    window.Close();
                });

                viewModel.Cancel = ReactiveCommand.Create(() =>
                {
                    viewModel.DialogResult = false;
                    window.Close();
                });
            });
            return viewModel.DialogResult;
        }
    }
}