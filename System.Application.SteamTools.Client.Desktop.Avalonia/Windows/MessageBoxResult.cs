namespace System.Windows
{
    /// <summary>
    /// 指定用户单击的消息框按钮。 由 <see cref="MessageBox"/>.Show... 方法返回。
    /// </summary>
    public enum MessageBoxResult
    {
        Cancel = 2,
        No = 7,
        None = 0,
        OK = 1,
        Yes = 6,

        [Obsolete("non-standard api")]
        Abort = 1000,
    }
}