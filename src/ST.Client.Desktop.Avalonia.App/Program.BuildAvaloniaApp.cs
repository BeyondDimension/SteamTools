using Avalonia;
using Avalonia.ReactiveUI;

namespace System.Application.UI
{
    partial class Program
    {
        // Avalonia configuration, don't remove; also used by visual designer.
        static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new SkiaOptions
                {
                    MaxGpuResourceSizeBytes = 8096000,
                })
                .With(new Win32PlatformOptions
                {
                    AllowEglInitialization = true,
                })
                .LogToTrace()
                .UseReactiveUI();

        static void BuildAvaloniaAppAndStartWithClassicDesktopLifetime(string[] args)
            => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        /// <summary>
        /// 当前是否是主进程
        /// </summary>
        internal static bool IsMainProcess { get; private set; }

        /// <summary>
        /// 当前是否是命令行工具进程
        /// </summary>
        internal static bool IsCLTProcess { get; private set; }
    }
}