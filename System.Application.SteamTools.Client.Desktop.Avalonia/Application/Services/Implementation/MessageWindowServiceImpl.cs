using Avalonia.Controls;
using Avalonia.Threading;
using System.Application.UI;
using System.Application.UI.ViewModels;
using System.Application.UI.ViewModels.Windows;
using System.Application.UI.Windows;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
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
                var window = new MessageWindow();
                var dialog = new MessageWindowViewModel(window)
                {
                    Content = messageBoxText,
                    Title = caption,
                    IsCancelcBtn = isCancelcBtn
                };
                window.DataContext = dialog;
                //window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                await AppHelper.Current.ShowChildWindow(window);
                return dialog.DialogResult;
            });
        }

        public void CloseWindow(object window)
        {
            if (window is Window w)
                w.Close();
        }

        static Type GetWindowType(IMessageWindowService.CustomWindow customWindow) => customWindow switch
        {
            IMessageWindowService.CustomWindow.MessageBox => typeof(MessageWindow),
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
                AppHelper.Current.ShowWindow(window);
            });
        }
    }
}