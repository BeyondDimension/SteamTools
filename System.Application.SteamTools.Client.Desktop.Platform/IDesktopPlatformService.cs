using System.Diagnostics;
using ResizeMode = System.Int32;

namespace System.Application.Services
{
    public interface IDesktopPlatformService
    {
        void SetResizeMode(IntPtr hWnd, ResizeMode value);

        /// <summary>
        /// 获取一个正在运行的进程的命令行参数。
        /// 与 <see cref="Environment.GetCommandLineArgs"/> 一样，使用此方法获取的参数是包含应用程序路径的。
        /// 关于 <see cref="Environment.GetCommandLineArgs"/> 可参见：
        /// .NET 命令行参数包含应用程序路径吗？https://blog.walterlv.com/post/when-will-the-command-line-args-contain-the-executable-path.html
        /// </summary>
        /// <param name="process">一个正在运行的进程。</param>
        /// <returns>表示应用程序运行命令行参数的字符串。</returns>
        string GetCommandLineArgs(Process process);

        public const ResizeMode ResizeMode_NoResize = 0;
        public const ResizeMode ResizeMode_CanMinimize = 1;
        public const ResizeMode ResizeMode_CanResize = 2;
        public const ResizeMode ResizeMode_CanResizeWithGrip = 3;
    }
}