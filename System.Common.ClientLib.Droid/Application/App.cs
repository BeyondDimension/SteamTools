using Android.App;
using Android.Runtime;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using Plugin.CurrentActivity;
using System.Application.Models;
using System.Application.UI;
using System.Logging;
using System.Properties;
using Xamarin.Essentials;
using AndroidApplication = Android.App.Application;
using R = System.Common.R;
using XEFileProvider = Xamarin.Essentials.FileProvider;
using XEPlatform = Xamarin.Essentials.Platform;
using XEVersionTracking = Xamarin.Essentials.VersionTracking;

namespace System.Application
{
    public abstract class App<TAppSettings> : AndroidApplication
        where TAppSettings : class, IAppSettings, new()
    {
        public App(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        /// <summary>
        /// 是否为主进程
        /// </summary>
        /// <returns></returns>
        public virtual bool IsMainProcess()
        {
            // 注意：进程名可以自定义，默认是包名，如果自定义了主进程名，这里就有误，所以不要自定义主进程名！
            var name = this.GetCurrentProcessName();
            return name == PackageName;
        }

        /// <summary>
        /// 是否启用 <see cref="ThreadUncaughtExceptionHandler"/> 纪录全局异常，默认仅在Debug模式中启用
        /// </summary>
        protected virtual bool EnableThreadUncaughtExceptionHandler => ThisAssembly.Debuggable;

        /// <summary>
        /// 获取APP设置
        /// </summary>
        protected virtual TAppSettings AppSettings
            => AppClientAttribute.Get<TAppSettings>(GetType().Assembly)
            ?? throw new NullReferenceException("GetAppSettings is null.");

        public sealed override void OnCreate()
        {
            base.OnCreate();

            if (EnableThreadUncaughtExceptionHandler)
            {
                ThreadUncaughtExceptionHandler.Initialize();
            }

            CrossCurrentActivity.Current.Init(this);

            var options = AppSettings;

            #region Visual Studio App Center

            if (options.AppSecretVisualStudioAppCenter != null)
            {
                // Visual Studio App Center
                // 将移动开发人员常用的多种服务整合到一个集成的产品中。
                // 您可以构建，测试，分发和监控移动应用程序，还可以实施推送通知。
                // https://docs.microsoft.com/zh-cn/appcenter/sdk/getting-started/xamarin
                // https://visualstudio.microsoft.com/zh-hans/app-center
                AppCenter.Start(options.AppSecretVisualStudioAppCenter, typeof(Analytics), typeof(Crashes));
            }

            #endregion

            XEFileProvider.TemporaryLocation = FileProviderLocation.Internal;
            XEPlatform.Init(this); // 初始化 Xamarin.Essentials.Platform.Init

            var isMainProcess = IsMainProcess();

            DI.Init(ConfigureServices);

            void ConfigureServices(IServiceCollection services)
            {
                ConfigureRequiredServices(services, options);
                if (isMainProcess)
                {
                    ConfigureMainProcessServices(services, options);
                }
            }

            if (isMainProcess)
            {
                XEVersionTracking.Track();

                // 厂商ROM检测
                AndroidROM.Initialize();

                // Emoji表情
                EmojiCompatLibrary.Init(this);
            }

            OnCreated(options, isMainProcess);

#if DEBUG
            Log.Debug("Application", "OnCreate Complete.");
#endif
        }

        public virtual void OnCreated(TAppSettings options, bool isMainProcess)
        {
        }

        /// <summary>
        /// 配置任何线程都必要的依赖注入服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        public virtual void ConfigureRequiredServices(IServiceCollection services, TAppSettings options)
        {
            services.AddClientLogging();
            services.TryAddOptions(options);
            services.TryAddStorage();
            services.TryAddDebuggerDisplay();
        }

        /// <summary>
        /// 配置主线程所需的依赖注入服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        public virtual void ConfigureMainProcessServices(IServiceCollection services, TAppSettings options)
        {
            services.TryAddModelValidator();
            services.TryAddPermissions();
            services.TryAddToast();
            services.AddPlatformPermissions();
            services.AddTelephonyService();
        }

        protected static void InitResourceValues<TEntrance>(
            int toolbar,
            int btn_back,
            int adapter_item_click,
            int adapter_item_long_click,
            int slide_in_right,
            int slide_out_left,
            int slide_in_left,
            int slide_out_right,
            int ic_stat_notify_msg,
            string app_name) where TEntrance : Activity
        {
            R.id.toolbar = toolbar;
            R.id.btn_back = btn_back;
            R.id.adapter_item_click = adapter_item_click;
            R.id.adapter_item_long_click = adapter_item_long_click;
            R.anim.slide_in_right = slide_in_right;
            R.anim.slide_out_left = slide_out_left;
            R.anim.slide_in_left = slide_in_left;
            R.anim.slide_out_right = slide_out_right;
            R.drawable.ic_stat_notify_msg = ic_stat_notify_msg;
            R.@string.app_name = app_name;
            R.activities.entrance = typeof(TEntrance).GetJClass();
        }
    }
}