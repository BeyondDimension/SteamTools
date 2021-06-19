using System.Application.Services;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace System
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
    }
}