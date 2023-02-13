// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// MessageBox 自定义实现服务(可选)
/// </summary>
public interface IMessageBoxService
{
    static IMessageBoxService? Instance => Ioc.Get_Nullable<IMessageBoxService>();

    /// <summary>
    /// 显示一个消息框，该消息框包含消息、标题栏标题、按钮和图标，并且返回结果
    /// </summary>
    /// <param name="messageBoxText">一个 <see cref="string"/>，用于指定要显示的文本。</param>
    /// <param name="caption">一个 <see cref="string"/>，用于指定要显示的标题栏标题。</param>
    /// <param name="button">一个 <see cref="MessageBox.Button"/> 值，用于指定要显示哪个按钮或哪些按钮。</param>
    /// <param name="icon">一个 <see cref="MessageBox.Image"/> 值，用于指定要显示的图标。</param>
    /// <returns>一个 <see cref="MessageBox.Result"/> 值，用于指定用户单击了哪个消息框按钮。</returns>
    Task<MessageBox.Result> ShowAsync(string messageBoxText, string caption, MessageBox.Button button, MessageBox.Image icon);
}
