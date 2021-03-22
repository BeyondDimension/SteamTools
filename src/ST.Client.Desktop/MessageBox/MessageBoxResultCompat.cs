// ReSharper disable once CheckNamespace
namespace System.Windows
{
    /// <summary>
    /// 指定用户单击的消息框按钮。 由 <see cref="MessageBoxCompat"/>.Show... 方法返回。
    /// </summary>
    public enum MessageBoxResultCompat
    {
        Cancel = 2,
        None = 0,
        OK = 1,

        [Obsolete("non-standard api")]
        Yes = 6,
        [Obsolete("non-standard api")]
        No = 7,
        [Obsolete("non-standard api")]
        Abort = 1000,
    }
}