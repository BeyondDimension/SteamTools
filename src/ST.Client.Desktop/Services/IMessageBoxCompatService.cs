using System.Threading.Tasks;
using System.Windows;

namespace System.Application.Services
{
    /// <summary>
    /// System.Windows.MessageBoxCompat.ShowAsync
    /// </summary>
    public interface IMessageBoxCompatService
    {
        /// <summary>
        /// 显示一个消息框，该消息框包含消息、标题栏标题、按钮和图标，并且返回结果。
        /// </summary>
        /// <param name="messageBoxText">一个 <see cref="string"/>，用于指定要显示的文本。</param>
        /// <param name="caption">一个 <see cref="string"/>，用于指定要显示的标题栏标题。</param>
        /// <param name="button">一个 <see cref="MessageBoxButtonCompat"/> 值，用于指定要显示哪个按钮或哪些按钮。</param>
        /// <param name="icon">一个 <see cref="MessageBoxImageCompat"/> 值，用于指定要显示的图标。</param>
        /// <returns>一个 <see cref="MessageBoxResultCompat"/> 值，用于指定用户单击了哪个消息框按钮。</returns>
        Task<MessageBoxResultCompat> ShowAsync(string messageBoxText, string caption, MessageBoxButtonCompat button, MessageBoxImageCompat? icon);
    }
}