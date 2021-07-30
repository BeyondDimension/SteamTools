using Avalonia;
using Avalonia.ReactiveUI;
using System.Application.Models.Settings;

namespace System.Application.UI
{
    partial class Program
    {
        // Avalonia configuration, don't remove; also used by visual designer.
        static AppBuilder BuildAvaloniaApp()
        {
            SettingsHost.Load();
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new SkiaOptions
                {
                    MaxGpuResourceSizeBytes = 8096000,
                })
                .With(new AvaloniaNativePlatformOptions
                {
                    UseGpu = GeneralSettings.UseGPURendering.Value
                })
                .With(new Win32PlatformOptions
                {
                    AllowEglInitialization = true /*!DI.IsmacOS*/,
                    UseWindowsUIComposition = true
                })
                .LogToTrace()
                .UseReactiveUI();
        }

        static void BuildAvaloniaAppAndStartWithClassicDesktopLifetime(string[] args)
            => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
}