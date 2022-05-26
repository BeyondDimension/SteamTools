namespace System;

public abstract class DesktopBridge
{
    protected DesktopBridge() => throw new NotSupportedException();

    /// <summary>
    /// 指示当前应用程序是否正在通过 Desktop Bridge 运行在 UWP 上。
    /// <para></para>
    /// 与 <see cref="OperatingSystem2.IsRunningAsUwp"/> 不同，<see cref="OperatingSystem2.IsRunningAsUwp"/> 通过判断进程含有包标识返回 <see cref="bool"/>，从 Windows 10 版本 2004 开始，可以通过生成稀疏包并将其注册到应用，向未打包到 MSIX 包中的桌面应用授予包标识符。
    /// <para>https://docs.microsoft.com/zh-cn/windows/apps/desktop/modernize/grant-identity-to-nonpackaged-apps</para>
    /// </summary>
    public static bool IsRunningAsUwp { get; protected set; }
}