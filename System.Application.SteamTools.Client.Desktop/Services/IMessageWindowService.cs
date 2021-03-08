using System.Application.UI.ViewModels;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.Services
{
    public interface IMessageWindowService
    {
        /// <summary>
        /// 显示一个消息框，该消息框包含消息、标题栏标题、按钮，并且返回结果。
        /// </summary>
        /// <param name="messageBoxText">一个 <see cref="string"/>，用于指定要显示的文本。</param></param>
        /// <returns>一个 <see cref="MessageBoxResultCompat"/> 值，用于指定用户单击了哪个消息框按钮。</returns>
        Task<bool> ShowDialog(string messageBoxText);

        /// <summary>
        /// 显示一个消息框，该消息框包含消息、标题栏标题、按钮，并且返回结果。
        /// </summary>
        /// <param name="messageBoxText">一个 <see cref="string"/>，用于指定要显示的文本。</param>
        /// <param name="caption">一个 <see cref="string"/>，用于指定要显示的标题栏标题。</param></param>
        /// <returns>一个 <see cref="MessageBoxResultCompat"/> 值，用于指定用户单击了哪个消息框按钮。</returns>
        Task<bool> ShowDialog(string messageBoxText, string caption);

        /// <summary>
        /// 显示一个消息框，该消息框包含消息、标题栏标题、按钮，并且返回结果。
        /// </summary>
        /// <param name="messageBoxText">一个 <see cref="string"/>，用于指定要显示的文本。</param>
        /// <param name="caption">一个 <see cref="string"/>，用于指定要显示的标题栏标题。</param>
        /// <param name="button">一个 <see cref="MessageBoxButtonCompat"/> 值，用于指定要显示哪个按钮或哪些按钮。</param>
        /// <returns>一个 <see cref="MessageBoxResultCompat"/> 值，用于指定用户单击了哪个消息框按钮。</returns>
        Task<bool> ShowDialog(string messageBoxText, string caption, bool isCancelcBtn);

        void CloseWindow(object window);

        Task Show(CustomWindow window, ViewModelBase? dataContext = null);

        public enum CustomWindow : byte
        {
            MessageBox,

            XXWindow,

            XXXWindow,
        }
    }
}