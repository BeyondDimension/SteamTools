// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 打开桌面图标设置
    /// </summary>
    void OpenDesktopIconsSettings() { }

    void OpenGameControllers() { }

    /// <summary>
    /// 以正常权限启动进程
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    Process? StartAsInvoker(string fileName, string? arguments = null)
        => Process2.Start(fileName, arguments);

    /// <summary>
    /// 设置窗口右上角系统按钮显示或隐藏
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="isVisible"></param>
    void SetWindowSystemButtonsIsVisible(IntPtr hWnd, bool isVisible) { }

#if DEBUG

    /// <summary>
    /// 在 Win7 上是否开启了 Aero
    /// <para>https://docs.microsoft.com/zh-cn/windows/win32/api/dwmapi/nf-dwmapi-dwmiscompositionenabled</para>
    /// </summary>
    [Obsolete("Windows 7 is no longer supported.")]
    bool DwmIsCompositionEnabled => true;

    /// <summary>
    /// 在 Win7 上开启 Aero 时隐藏 SystemButtons
    /// </summary>
    /// <param name="hWnd"></param>
    [Obsolete("Windows 7 is no longer supported.")]
    void FixAvaloniaFluentWindowStyleOnWin7(IntPtr hWnd)
    {
        if (DwmIsCompositionEnabled)
        {
            SetWindowSystemButtonsIsVisible(hWnd, true);
        }
    }

#endif

    /// <summary>
    /// 获取当前 Windows 系统产品名称，例如 Windows 10 Pro
    /// </summary>
    string WindowsProductName => string.Empty;

    /// <summary>
    /// 获取当前 Windows 系统第四位版本号
    /// </summary>
    int WindowsVersionRevision => default;

    /// <summary>
    /// 获取当前 Windows 10/11 系统显示版本，例如 21H1
    /// </summary>
    string WindowsReleaseIdOrDisplayVersion => string.Empty;
}