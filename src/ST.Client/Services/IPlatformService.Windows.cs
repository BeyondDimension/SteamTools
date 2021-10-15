using System.Diagnostics;
#if NETSTANDARD
using JustArchiNET.Madness;
#else
using System.Runtime.Versioning;
#endif

namespace System.Application.Services
{
    partial interface IPlatformService
    {
        /// <summary>
        /// 打开桌面图标设置
        /// </summary>
        [SupportedOSPlatform("Windows")]
        void OpenDesktopIconsSettings() => throw new PlatformNotSupportedException();

        [SupportedOSPlatform("Windows")]
        void OpenGameControllers() => throw new PlatformNotSupportedException();

        /// <summary>
        /// 已正常权限启动进程
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [SupportedOSPlatform("Windows")]
        Process StartAsInvoker(string fileName) => throw new PlatformNotSupportedException();

        /// <summary>
        /// 获取占用端口的进程
        /// </summary>
        /// <param name="port"></param>
        /// <param name="isTCPorUDP"></param>
        /// <returns></returns>
        [SupportedOSPlatform("Windows")]
        Process? GetProcessByPortOccupy(ushort port, bool isTCPorUDP = true) => throw new PlatformNotSupportedException();

        /// <summary>
        /// 从管理员权限进程中降权到普通权限启动进程
        /// </summary>
        /// <param name="cmdArgs"></param>
        [SupportedOSPlatform("Windows")]
        void UnelevatedProcessStart(string cmdArgs) => throw new PlatformNotSupportedException();

        [SupportedOSPlatform("Windows")]
        void FixAvaloniaFluentWindowStyleOnWin7(IntPtr hWnd) => throw new PlatformNotSupportedException();

        /// <summary>
        /// 获取当前 Windows 系统产品名称，例如 Windows 10 Pro
        /// </summary>
        [SupportedOSPlatform("Windows")]
        string WindowsProductName => throw new PlatformNotSupportedException();

        /// <summary>
        /// 获取当前 Windows 系统第四位版本号
        /// </summary>
        [SupportedOSPlatform("Windows")]
        int WindowsVersionRevision => throw new PlatformNotSupportedException();

        /// <summary>
        /// 获取当前 Windows 10/11 系统显示版本，例如 21H1
        /// </summary>
        [SupportedOSPlatform("Windows")]
        string WindowsReleaseIdOrDisplayVersion => throw new PlatformNotSupportedException();
    }
}
