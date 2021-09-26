using Avalonia;
using Avalonia.ReactiveUI;
using System.Application.Models.Settings;
using System.Application.Services;

namespace System.Application.UI
{
    partial class Program
    {
        /// <inheritdoc cref="IDesktopAppService.RenderingSubsystemName"/>
        internal static string RenderingSubsystemName { get; private set; } = string.Empty;

        // Avalonia configuration, don't remove; also used by visual designer.
        static AppBuilder BuildAvaloniaApp()
        {
            SettingsHost.Load();
            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
            if (OperatingSystem2.IsMacOS)
            {
                builder.With(new AvaloniaNativePlatformOptions
                {
                    UseGpu = !AppHelper.DisableGPU && GeneralSettings.UseGPURendering.Value
                });
            }
            else if (OperatingSystem2.IsWindows)
            {
                var useOpenGL = AppHelper.UseOpenGL || GeneralSettings.UseOpenGL.Value;
                var options = new Win32PlatformOptions
                {
                    UseWindowsUIComposition = true,
                    UseWgl = useOpenGL,
                    AllowEglInitialization = !useOpenGL,
                };
                builder.With(options);
            }
            //            if (OperatingSystem2.IsWindows &&
            //#if DEBUG
            //                true
            //#else
            //                GeneralSettings.UseDirect2D1.Value
            //#endif
            //                )
            //            {
            //                builder.UseDirect2D1();
            //            }
            RenderingSubsystemName = builder.RenderingSubsystemName;
            return builder;
        }

        static void BuildAvaloniaAppAndStartWithClassicDesktopLifetime(string[] args)
            => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
}