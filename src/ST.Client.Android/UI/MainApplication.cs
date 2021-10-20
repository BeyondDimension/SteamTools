using Android.App;
using Android.Runtime;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Diagnostics;
using Xamarin.Essentials;
using _ThisAssembly = System.Properties.ThisAssembly;
using AppTheme = System.Application.Models.AppTheme;
using XEFileProvider = Xamarin.Essentials.FileProvider;
using XEPlatform = Xamarin.Essentials.Platform;
using XEVersionTracking = Xamarin.Essentials.VersionTracking;
using System.Application.Settings;
using AndroidX.AppCompat.App;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Windows.Input;
using System.Collections.Generic;
using System.Reactive.Disposables;
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
    public sealed partial class MainApplication : IApplication
    {
        public static MainApplication Instance => Context is MainApplication app ? app : throw new Exception("Impossible");

        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            IViewModelBase.IsInDesignMode = false;
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

            IApplication.InitLogDir();

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
                PlatformNotificationServiceImpl.InitNotificationChannels(this);
                XEVersionTracking.Track();
                if (XEVersionTracking.IsFirstLaunchForCurrentVersion)
                {
                    // 当前版本第一次启动时，清除存放升级包缓存文件夹的目录
                    IApplicationUpdateService.ClearAllPackCacheDir();
                }
                ImageLoader.Init(this);
                UISettings.Theme.Subscribe(x =>
                {
                    Theme = (AppTheme)x;
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

        const AppTheme _DefaultActualTheme = AppTheme.Light;
        AppTheme IApplication.DefaultActualTheme => _DefaultActualTheme;

        public new AppTheme Theme
        {
            get => AppCompatDelegate.DefaultNightMode switch
            {
                AppCompatDelegate.ModeNightNo => AppTheme.Light,
                AppCompatDelegate.ModeNightYes => AppTheme.Dark,
                _ => AppTheme.FollowingSystem,
            };
            set
            {
                var defaultNightMode = value switch
                {
                    AppTheme.FollowingSystem => AppCompatDelegate.ModeNightFollowSystem,
                    AppTheme.Light => AppCompatDelegate.ModeNightNo,
                    AppTheme.Dark => AppCompatDelegate.ModeNightYes,
                    _ => int.MinValue,
                };
                if (defaultNightMode != int.MinValue) return;
                AppCompatDelegate.DefaultNightMode = defaultNightMode;
            }
        }

        AppTheme IApplication.GetActualThemeByFollowingSystem()
            => DarkModeUtil.IsDarkMode(this) ? AppTheme.Dark : AppTheme.Light;

        public static async void ShowUnderConstructionTips() => await MessageBox.ShowAsync(AppResources.UnderConstruction);

        bool IApplication.HasActiveWindow() => XEPlatform.CurrentActivity.HasValue();

        IApplication.AppType IApplication.GetType() => IApplication.AppType.HybridAndroid;

        #region Compat

        void IApplication.SetThemeNotChangeValue(AppTheme value) => Theme = value;

        object IApplication.CurrentPlatformUIHost => XEPlatform.CurrentActivity;

        void IApplication.Shutdown()
        {

        }

        void IApplication.RestoreMainWindow()
        {

        }

        readonly Dictionary<string, ICommand> _NotifyIconMenus = new();
        IReadOnlyDictionary<string, ICommand> IApplication.NotifyIconMenus => _NotifyIconMenus;

        readonly CompositeDisposable compositeDisposable = new();

        CompositeDisposable IApplication.CompositeDisposable => compositeDisposable;

        #endregion
    }
}