namespace Mobius.Enums;

/// <summary>
/// 客户端窗口显示方式
/// <para>https://learn.microsoft.com/zh-cn/windows/win32/api/winuser/nf-winuser-showwindow</para>
/// </summary>
public enum XunYouShowCommands
{
    /// <summary>
    /// 隐藏窗口并激活另一个窗口。
    /// </summary>
    SW_HIDE = 0,

    /// <summary>
    /// 激活并显示窗口。 如果窗口最小化、最大化或排列，系统会将其还原到其原始大小和位置。 应用程序应在首次显示窗口时指定此标志。
    /// </summary>
    SW_SHOWNORMAL = 1,

    SW_SHOWMINIMIZED = 2,

    SW_SHOWMAXIMIZED = 3,

    SW_SHOWNOACTIVATE = 4,

    SW_SHOW = 5,

    SW_MINIMIZE = 6,

    SW_SHOWMINNOACTIVE = 7,

    SW_SHOWNA = 8,

    SW_RESTORE = 9,

    SW_SHOWDEFAULT = 10,

    SW_FORCEMINIMIZE = 11,
}
