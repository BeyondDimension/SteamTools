#if DEBUG
using Avalonia.Controls;
using ReactiveUI;
using System.Application.UI.ViewModels;
using System.Application.UI.ViewModels.Windows;
using System.Application.UI.Windows;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    [Obsolete("use ShowWindowServiceImpl")]
    internal sealed class MessageWindowServiceImpl : IMessageWindowService
    {
        public Task<bool> ShowDialog(string messageBoxText)
        {
            return ShowDialog(messageBoxText, "", false);
        }

        public Task<bool> ShowDialog(string messageBoxText, string caption)
        {
            return ShowDialog(messageBoxText, caption, false);
        }

        public async Task<bool> ShowDialog(string messageBoxText, string caption, bool isCancelcBtn)
        {
            return await MainThreadDesktop.InvokeOnMainThreadAsync(async () =>
            {
                var window = new MessageBoxWindow();
                var dialog = new MessageBoxWindowViewModel()
                {
                    Content = messageBoxText,
                    Title = caption,
                    IsCancelcBtn = isCancelcBtn
                };
                dialog.OK = ReactiveCommand.Create(() =>
                {
                    dialog.DialogResult = true;
                    window.Close();
                });

                dialog.Cancel = ReactiveCommand.Create(() =>
                {
                    dialog.DialogResult = false;
                    window.Close();
                });
                window.DataContext = dialog;
                //window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                await IDesktopAvaloniaAppService.Instance.ShowDialogWindow(window);
                return dialog.DialogResult;
            });
        }

        static Type GetWindowType(IMessageWindowService.CustomWindow customWindow) => customWindow switch
        {
            IMessageWindowService.CustomWindow.MessageBox => typeof(MessageBoxWindow),
            _ => throw new ArgumentOutOfRangeException(nameof(customWindow), customWindow, null),
        };

        public Task Show(IMessageWindowService.CustomWindow customWindow, ViewModelBase? dataContext = null)
        {
            return MainThreadDesktop.InvokeOnMainThreadAsync(() =>
            {
                var windowType = GetWindowType(customWindow);
                var window = (Window)Activator.CreateInstance(windowType);
                if (dataContext != null)
                {
                    window.DataContext = dataContext;
                }
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                IDesktopAvaloniaAppService.Instance.ShowWindow(window);
            });
        }
    }
}
#endif