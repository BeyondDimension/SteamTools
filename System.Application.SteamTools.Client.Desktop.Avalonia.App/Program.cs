using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Application.UI.ViewModels;

namespace System.Application.UI
{
    static class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        static void Main(string[] args)
        {
            ModelValidatorProvider.Init();
            DI.Init(ConfigureServices);
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        static AppBuilder BuildAvaloniaApp()
           => AppBuilder.Configure<App>()
               .UsePlatformDetect()
               .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 })
               .With(new Win32PlatformOptions { AllowEglInitialization = true })
               .LogToTrace()
               .UseReactiveUI();

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(l => l.AddProvider(NullLoggerProvider.Instance));
            services.AddDesktopPlatformService();
            services.TryAddModelValidator();
            services.AddLocalizationService();
            services.AddMainThreadPlatformService();
            services.AddViewModelCollectionService();
            services.AddViewModel<MainWindowViewModel>();
            services.AddViewModel<SettingsPageViewModel>();
        }
    }
}