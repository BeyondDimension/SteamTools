using Avalonia;
using Avalonia.ReactiveUI;
using System.Application.Settings;
using System.Application.Services;
using System.Reflection;

namespace System.Application.UI
{
    partial class Program
    {
        /// <inheritdoc cref="IDesktopApplication.RenderingSubsystemName"/>
        internal static string RenderingSubsystemName { get; private set; } = string.Empty;

        // Avalonia configuration, don't remove; also used by visual designer.
        static AppBuilder BuildAvaloniaApp()
        {
#if DEBUG
            //设计器模式不会执行Main函数所以以此区分来初始化文件系统
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
            {
                FileSystemDesktop.InitFileSystem();
            }
#endif
            SettingsHost.Load();
            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();

            var useGpu = !IApplication.DisableGPU && GeneralSettings.UseGPURendering.Value;

            if (OperatingSystem2.IsMacOS)
            {
                builder.With(new AvaloniaNativePlatformOptions
                {
                    UseGpu = useGpu
                });
            }
            else if (OperatingSystem2.IsWindows)
            {
                var useWgl = IApplication.UseWgl || GeneralSettings.UseWgl.Value;
                var options = new Win32PlatformOptions
                {
                    UseWgl = useWgl,
                    AllowEglInitialization = useGpu,
                };
                builder.With(options);
            }
            else if (OperatingSystem2.IsLinux)
            {
                builder.With(new X11PlatformOptions
                {
                    UseGpu = useGpu
                });
            }
            RenderingSubsystemName = builder.RenderingSubsystemName;
            return builder;
        }

        static void BuildAvaloniaAppAndStartWithClassicDesktopLifetime(string[] args)
            => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
}