// ReSharper disable once CheckNamespace
namespace System.Windows
{
    /// <summary>
    /// 指定显示在消息框上的按钮。 用作 <see cref="MessageBoxCompat"/>.Show... 方法的参数。
    /// </summary>
    public enum MessageBoxButtonCompat
    {
        OK = 0,
        OKCancel = 1,

        [Obsolete("non-standard api")]
        YesNo = 4,
        [Obsolete("non-standard api")]
        YesNoCancel = 3,
        [Obsolete("non-standard api")]
        OkAbort = 1000,
        [Obsolete("non-standard api")]
        YesNoAbort = 1001,
    }
}