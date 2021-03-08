using Avalonia.Controls;
using Avalonia.Threading;
using System.Application.UI;
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
            return MainThreadDesktop.InvokeOnMainThreadAsync<bool>(() =>
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
    }
}