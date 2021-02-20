namespace System.Windows
{
    /// <summary>
    /// 指定显示在消息框上的按钮。 用作 <see cref="MessageBoxCompat"/>.Show... 方法的参数。
    /// </summary>
    public enum MessageBoxButton
    {
        OK = 0,
        OKCancel = 1,
        YesNo = 4,
        YesNoCancel = 3,

        [Obsolete("non-standard api")]
        OkAbort = 1000,
        [Obsolete("non-standard api")]
        YesNoAbort = 1001,
    }
}