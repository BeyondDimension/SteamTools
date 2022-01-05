using Avalonia;
using Avalonia.ReactiveUI;
using System.Application.Settings;
using System.Reflection;

namespace System.Application.UI
{
    partial class Program
    {
        /// <inheritdoc cref="IAvaloniaApplication.RenderingSubsystemName"/>
        internal static string RenderingSubsystemName { get; private set; } = string.Empty;

        // Avalonia configuration, don't remove; also used by visual designer.
        static AppBuilder BuildAvaloniaApp()
        {
#if DEBUG
            // 设计器模式不会执行 Main 函数所以以此区分来初始化文件系统
            if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
            {
                FileSystem2.InitFileSystem();
            }
#endif
            SettingsHost.Load();
            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();

            var useGpu = !IApplication.DisableGPU && GeneralSettings.UseGPURendering.Value;

#if MAC
            builder.With(new AvaloniaNativePlatformOptions
            {
                UseGpu = useGpu
            });
#elif LINUX
            builder.With(new X11PlatformOptions
            {
                UseGpu = useGpu
            });
#elif WINDOWS
            var useWgl = IApplication.UseWgl || GeneralSettings.UseWgl.Value;
            var options = new Win32PlatformOptions
            {
                UseWgl = useWgl,
                AllowEglInitialization = useGpu,
            };
            builder.With(options);
#else
            throw new PlatformNotSupportedException();
#endif

            RenderingSubsystemName = builder.RenderingSubsystemName;
            return builder;
        }

        static void BuildAvaloniaAppAndStartWithClassicDesktopLifetime(string[] args)
            => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
}