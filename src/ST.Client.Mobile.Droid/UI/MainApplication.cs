using Android.App;
using Android.Runtime;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Diagnostics;
using System.Windows;
using Xamarin.Essentials;
using _ThisAssembly = System.Properties.ThisAssembly;
using AndroidApplication = Android.App.Application;
using AppTheme = System.Application.Models.AppTheme;
using XEFileProvider = Xamarin.Essentials.FileProvider;
using XEPlatform = Xamarin.Essentials.Platform;
using XEVersionTracking = Xamarin.Essentials.VersionTracking;
using System.Application.Models.Settings;
using AndroidX.AppCompat.App;
using System.Application.Services;
using System.Application.Services.Implementation;
#if DEBUG
using Square.LeakCanary;
#endif

namespace System.Application.UI
{
    [Register(JavaPackageConstants.UI + nameof(MainApplication))]
    [Application(
        Debuggable = _ThisAssembly.Debuggable,
        Label = "@string/app_name",
        Theme = "@style/MainTheme",
        Icon = "@mipmap/ic_launcher",
        RoundIcon = "@mipmap/ic_launcher_round")]
    public sealed class MainApplication : AndroidApplication
    {
        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            ViewModelBase.IsInDesignMode = false;
        }

#if DEBUG
        RefWatcher? _refWatcher;

        void SetupLeakCanary()
        {
            // “A small leak will sink a great ship.” - Benjamin Franklin
            if (LeakCanaryXamarin.IsInAnalyzerProcess(this))
            {
                // This process is dedicated to LeakCanary for heap analysis.
                // You should not init your app in this process.
                return;
            }
            _refWatcher = LeakCanaryXamarin.Install(this);
        }
#endif

        public override void OnCreate()
        {
            base.OnCreate();
#if DEBUG
            SetupLeakCanary();
#endif
            var stopwatch = Stopwatch.StartNew();

            AppHelper.InitLogDir();

            Startup.InitGlobalExceptionHandler();

            VisualStudioAppCenterSDK.Init();

            XEFileProvider.TemporaryLocation = FileProviderLocation.Internal;
            XEPlatform.Init(this); // 初始化 Xamarin.Essentials.Platform.Init

            bool GetIsMainProcess()
            {
                // 注意：进程名可以自定义，默认是包名，如果自定义了主进程名，这里就有误，所以不要自定义主进程名！
                var name = this.GetCurrentProcessName();
                return name == PackageName;
            }
            IsMainProcess = GetIsMainProcess();

            var level = DILevel.Min;
            if (IsMainProcess) level = DILevel.MainProcess;
            Startup.Init(level);
            if (IsMainProcess)
            {
                if (INotificationService.Instance is PlatformNotificationServiceImpl notification)
                {
                    notification.InitNotificationChannels(this);
                }
                XEVersionTracking.Track();
                if (XEVersionTracking.IsFirstLaunchForCurrentVersion)
                {
                    // 当前版本第一次启动时，清除存放升级包缓存文件夹的目录
                    IAppUpdateService.ClearAllPackCacheDir();
                }
                ImageLoader.Init(this);
                UISettings.Theme.Subscribe(x =>
                {
                    var defaultNightMode = (AppTheme)x switch
                    {
                        AppTheme.FollowingSystem => AppCompatDelegate.ModeNightFollowSystem,
                        AppTheme.Light => AppCompatDelegate.ModeNightNo,
                        AppTheme.Dark => AppCompatDelegate.ModeNightYes,
                        _ => (int?)null,
                    };
                    if (!defaultNightMode.HasValue) return;
                    AppCompatDelegate.DefaultNightMode = defaultNightMode.Value;
                });
            }
            UISettings.Language.Subscribe(x => R.ChangeLanguage(x));

            Startup.OnStartup(IsMainProcess);

            stopwatch.Stop();
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
#if DEBUG
            Log.Warn("Application", $"OnCreate Complete({stopwatch.ElapsedMilliseconds}ms");
#endif
        }

        public static long ElapsedMilliseconds { get; private set; }

        /// <summary>
        /// 当前是否是主进程
        /// </summary>
        public static bool IsMainProcess { get; private set; }

        public static string GetTheme()
        {
            if (DarkModeUtil.IsDarkMode(Context))
            {
                return AppTheme.Dark.ToString2();
            }
            return AppTheme.Light.ToString2();
        }

        public static async void ShowUnderConstructionTips() => await MessageBoxCompat.ShowAsync(AppResources.UnderConstruction, "", MessageBoxButtonCompat.OK);

        /// <summary>
        /// 是否允许截图
        /// </summary>
        public static bool AllowScreenshots => _ThisAssembly.IsAlphaRelease || _ThisAssembly.IsBetaRelease;
    }
}