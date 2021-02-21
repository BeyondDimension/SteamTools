using System.Diagnostics;
using System.Windows;

namespace System.Application.Services
{
    public interface IDesktopPlatformService
    {
        /// <inheritdoc cref="WpfCompatExtensions.SetResizeMode(Avalonia.Controls.Window, ResizeMode)"/>
        void SetResizeMode(IntPtr hWnd, ResizeMode value);

        /// <summary>
        /// 获取CPU名称
        /// </summary>
        /// <returns></returns>
        string[] GetCPUName();

        /// <summary>
        /// 获取GPU名称
        /// </summary>
        /// <returns></returns>
        string[] GetGPUName();

        /// <summary>
        /// 当前操作系统是否为 Windows Server 版本
        /// </summary>
        /// <returns></returns>
        bool IsWindowsServer();

        string GetOSVersion();

        bool Is64Bit(Process process);

        /// <summary>
        /// 是否是管理员权限
        /// </summary>
        /// <returns></returns>
        bool IsAdministrator();

        /// <summary>
        /// 启用 Fluent Design System 模糊窗口样式
        /// </summary>
        /// <param name="hWnd"></param>
        void Enable_Fluent_Design_System_Style_Blur(IntPtr hWnd);

        string NETFrameworkDescription { get; }
    }
}