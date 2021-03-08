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

        public Task<bool> ShowDialog(string messageBoxText, string caption, bool isCancelcBtn)
        {
            return MainThreadDesktop.InvokeOnMainThreadAsync(() =>
            {
                var dialog = new MessageWindowViewModel
                {
                    Content = messageBoxText,
                    Title = caption
                };
                var window = new MessageWindow { DataContext = dialog };
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                AppHelper.Current.ShowChildWindow(window);
                return dialog.DialogResult;
            });
        }

        static Type GetWindowType(IMessageWindowService.CustomWindow customWindow) => customWindow switch
        {
            IMessageWindowService.CustomWindow.MessageBox => typeof(MessageWindowViewModel),
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
                AppHelper.Current.ShowChildWindow(window);
            });
        }
    }
}