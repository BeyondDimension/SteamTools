using System.Application.Services;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    /// <summary>
    /// 为 <see cref="Process"/> 类型提供扩展方法
    /// </summary>
    public static class ProcessExtensions
    {
        /// <inheritdoc cref="IDesktopPlatformService.GetCommandLineArgs(Process)"/>
        public static string GetCommandLineArgs(this Process process)
        {
            try
            {
                var p = DI.Get<IDesktopPlatformService>();
                return p.GetCommandLineArgs(process);
            }
            catch (InvalidOperationException)
            {
                // 进程已退出。
                return string.Empty;
            }
        }

        /// <summary>
        /// 兼容Linux和Mac的打开链接方法
        /// </summary>
        /// <param name="process"></param>
        /// <param name="Url"></param>
        public static Process StartUrl(this Process Process, string Url)
        {
            return Process = Process.Start(new ProcessStartInfo { FileName = Url, UseShellExecute = true });
        }
    }
}