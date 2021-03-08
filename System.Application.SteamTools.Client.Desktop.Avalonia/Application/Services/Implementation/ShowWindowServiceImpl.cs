using Avalonia.Controls;
using ReactiveUI;
using System.Application.UI.ViewModels;
using System.Application.UI.Windows;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.Services.Implementation
{
    internal sealed class ShowWindowServiceImpl : IShowWindowService
    {
        static Type GetWindowType(CustomWindow customWindow) => customWindow switch
        {
            CustomWindow.MessageBox => typeof(MessageBoxWindow),
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
               viewModel.Title = title;
               window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
            string title,
            TWindowViewModel? viewModel = null,
            ResizeModeCompat resizeMode = ResizeModeCompat.NoResize)
            where TWindowViewModel : WindowViewModel, new()
        {
            viewModel ??= new TWindowViewModel();
            return Show(false, customWindow, title, viewModel, resizeMode);
        }

        public async Task<bool> ShowDialog<TDialogWindowViewModel>(
            CustomWindow customWindow,
            string title,
            TDialogWindowViewModel? viewModel = null,
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