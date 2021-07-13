using Avalonia;
using Avalonia.ReactiveUI;
using System.Diagnostics;

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
            .With(new AvaloniaNativePlatformOptions
            {
                UseGpu = !DI.IsmacOS
            })
                .With(new Win32PlatformOptions
                {
                    AllowEglInitialization = !DI.IsmacOS,
                })
                .LogToTrace()
                .UseReactiveUI();

        static void BuildAvaloniaAppAndStartWithClassicDesktopLifetime(string[] args)
            => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
}
