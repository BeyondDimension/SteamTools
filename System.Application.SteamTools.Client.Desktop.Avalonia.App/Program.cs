using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System.Application.Models;
using System.Application.Services.Implementation;
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
            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                FileSystemDesktop.InitFileSystem();
                ModelValidatorProvider.Init();
                DI.Init(ConfigureServices);
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                AppHelper.EnableLogger = true;
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
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
            // 添加日志实现
            services.AddDesktopLogging();

            // 模型验证框架
            services.TryAddModelValidator();

            //var options = AppClientAttribute.Get<AppSettings>();
            var options = new AppSettings
            {
                //AppSecretVisualStudioAppCenter = "",
            };
            // app 配置项
            services.TryAddOptions(options);

            // 键值对存储
            services.TryAddStorage();

            // 业务平台用户管理
            services.TryAddUserManager();

            // 服务端API调用
            services.TryAddCloudServiceClient<CloudServiceClient>();

            // 桌面平台服务
            services.AddDesktopPlatformService();

            // 本地化服务
            services.AddLocalizationService();

            // 主线程助手类(MainThreadDesktop)
            services.AddMainThreadPlatformService();

            // 模型视图组
            services.AddViewModelCollectionService();
            services.AddViewModel<MainWindowViewModel>();
            services.AddViewModel<SettingsPageViewModel>();
        }
    }
}