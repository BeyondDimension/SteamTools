using Android.App;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using System.Properties;
using Xamarin.Essentials;
using AndroidApplication = Android.App.Application;
using XEFileProvider = Xamarin.Essentials.FileProvider;
using XEPlatform = Xamarin.Essentials.Platform;
using XEVersionTracking = Xamarin.Essentials.VersionTracking;

namespace System.Application
{
    [Application(Debuggable = ThisAssembly.Debuggable)]
    public sealed class MainApplication : AndroidApplication
    {
        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        /// <summary>
        /// 是否为主进程
        /// </summary>
        /// <returns></returns>
        public bool IsMainProcess()
        {
            // 注意：进程名可以自定义，默认是包名，如果自定义了主进程名，这里就有误，所以不要自定义主进程名！
            var name = this.GetCurrentProcessName();
            return name == PackageName;
        }

        public override void OnCreate()
        {
            base.OnCreate();

            #region Visual Studio App Center

            // Visual Studio App Center
            // 将移动开发人员常用的多种服务整合到一个集成的产品中。
            // 您可以构建，测试，分发和监控移动应用程序，还可以实施推送通知。
            // https://docs.microsoft.com/zh-cn/appcenter/sdk/getting-started/xamarin
            // https://visualstudio.microsoft.com/zh-hans/app-center
            VisualStudioAppCenterSDK.Init();

            #endregion

            XEFileProvider.TemporaryLocation = FileProviderLocation.Internal;
            XEPlatform.Init(this); // 初始化 Xamarin.Essentials.Platform.Init

            var isMainProcess = IsMainProcess();

            DI.Init(ConfigureServices);

            void ConfigureServices(IServiceCollection services)
            {
                ConfigureRequiredServices(services);
                if (isMainProcess)
                {
                    ConfigureMainProcessServices(services);
                }
            }

            if (isMainProcess)
            {
                XEVersionTracking.Track();
            }

#if DEBUG
            Log.Debug("Application", "OnCreate Complete.");
#endif
        }

        /// <summary>
        /// 配置任何进程都必要的依赖注入服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        void ConfigureRequiredServices(IServiceCollection services)
        {
            services.AddClientLogging();
            services.TryAddStorage();
        }

        /// <summary>
        /// 配置主进程所需的依赖注入服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        void ConfigureMainProcessServices(IServiceCollection services)
        {
            services.TryAddModelValidator();
            services.TryAddPermissions();
            services.TryAddToast();
            services.AddPlatformPermissions();
            services.AddTelephonyService();
        }
    }
}