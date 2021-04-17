using System.Application.Services;
using System.Diagnostics;
using static System.Application.Services.CloudService.Constants;

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
        /// 兼容 Linux 和 Mac 和 .NetCore 的打开链接方法
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Process? StartUrl(string url)
        {
            if (url.StartsWith(Prefix_HTTPS, StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith(Prefix_HTTP, StringComparison.OrdinalIgnoreCase))
            {
                return Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            return default;
        }
    }
}