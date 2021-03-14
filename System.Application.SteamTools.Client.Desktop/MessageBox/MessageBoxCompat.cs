using System.Application;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Properties;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Windows
{
    /// <summary>
    /// 显示消息框。
    /// </summary>
    public static class MessageBoxCompat
    {
        /// <inheritdoc cref="IMessageBoxCompatService.ShowAsync(string, string, MessageBoxButtonCompat, MessageBoxImageCompat?)"/>
        public static async Task<MessageBoxResultCompat> ShowAsync(
            string messageBoxText, string caption, MessageBoxButtonCompat button, MessageBoxImageCompat? icon = null)
        {
            var f = DI.Get_Nullable<IMessageBoxCompatService>();

            if (f != null)
            {
                return await f.ShowAsync(messageBoxText, caption, button, icon);
            }

            var viewModel = new MessageBoxWindowViewModel
            {
                Content = messageBoxText,
                IsCancelcBtn = button == MessageBoxButtonCompat.OKCancel,
            };

            var r = await IShowWindowService.Instance.ShowDialog(
                CustomWindow.MessageBox, viewModel, caption, ResizeModeCompat.NoResize);

            return r ? MessageBoxResultCompat.OK : MessageBoxResultCompat.Cancel;
        }

        /// <inheritdoc cref="IMessageBoxCompatService.ShowAsync(string, string, MessageBoxButtonCompat, MessageBoxImageCompat?)"/>
        public static async void Show(string messageBoxText, string caption = ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat button = MessageBoxButtonCompat.OK, MessageBoxImageCompat? icon = null)
        {
            await ShowAsync(messageBoxText, caption, button, icon);
        }
    }
}