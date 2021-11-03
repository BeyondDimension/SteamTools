using Android.App;
using Android.Runtime;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Diagnostics;
using System.Text;
using Xamarin.Essentials;
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
using Xamarin.Forms;
#if DEBUG
using Square.LeakCanary;
#endif
using ImageCircle.Forms.Plugin.Droid;
using XFApplication = Xamarin.Forms.Application;
using XEAppTheme = Xamarin.Essentials.AppTheme;

namespace System.Application.UI
{
    [Register(JavaPackageConstants.UI + nameof(MainApplication))]
    [Application(
        Label = "@string/app_name",
        Theme = "@style/MainTheme2",
        Icon = "@mipmap/ic_launcher",
        RoundIcon = "@mipmap/ic_launcher_round")]
    public sealed partial class MainApplication
    {
        public static MainApplication Instance => Context is MainApplication app ? app :
            throw new NullReferenceException("Mainapplication cannot be null.");

        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            // 此页面当前使用 Square.Picasso 库加载图片
            AuthTradeWindowViewModel.IsLoadImage = false;
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
            var startTrace = new StringBuilder();

            FileSystem2.InitFileSystem();

            stopwatch.Stop();
            startTrace.AppendFormatLine("init FileSystem {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            IApplication.InitLogDir();

            stopwatch.Stop();
            startTrace.AppendFormatLine("init NLog {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            Startup.InitGlobalExceptionHandler();

            stopwatch.Stop();
            startTrace.AppendFormatLine("init ExceptionHandler {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            VisualStudioAppCenterSDK.Init();

            stopwatch.Stop();
            startTrace.AppendFormatLine("init AppCenter {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            XEFileProvider.TemporaryLocation = FileProviderLocation.Internal;
            XEPlatform.Init(this); // 初始化 Xamarin.Essentials.Platform.Init

            stopwatch.Stop();
            startTrace.AppendFormatLine("init Essentials {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            bool GetIsMainProcess()
            {
                // 注意：进程名可以自定义，默认是包名，如果自定义了主进程名，这里就有误，所以不要自定义主进程名！
                var name = this.GetCurrentProcessName();
                return name == PackageName;
            }
            IsMainProcess = GetIsMainProcess();

            stopwatch.Stop();
            startTrace.AppendFormatLine("init IsMainProcess {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            SettingsHost.Load();

            stopwatch.Stop();
            startTrace.AppendFormatLine("init SettingsHost {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            InitSettingSubscribe();

            stopwatch.Stop();
            startTrace.AppendFormatLine("init SettingSubscribe {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            var level = DILevel.Min;
            if (IsMainProcess) level = DILevel.MainProcess;
            Startup.Init(level);

            stopwatch.Stop();
            startTrace.AppendFormatLine("init Startup {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            if (IsMainProcess)
            {
                XEVersionTracking.Track();

                stopwatch.Stop();
                startTrace.AppendFormatLine("init VersionTracking {0}ms", stopwatch.ElapsedMilliseconds);
                stopwatch.Restart();

                if (XEVersionTracking.IsFirstLaunchForCurrentVersion)
                {
                    // 当前版本第一次启动时，清除存放升级包缓存文件夹的目录
                    IApplicationUpdateService.ClearAllPackCacheDir();

                    stopwatch.Stop();
                    startTrace.AppendFormatLine("init ClearAllPackCacheDir {0}ms", stopwatch.ElapsedMilliseconds);
                    stopwatch.Restart();
                }
            }
            UISettings.Language.Subscribe(x => R.ChangeLanguage(x));

            stopwatch.Stop();
            startTrace.AppendFormatLine("init Language.Subscribe {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            Startup.OnStartup(IsMainProcess);

            stopwatch.Stop();
            startTrace.AppendFormatLine("init OnStartup {0}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            if (IsMainProcess)
            {
                var vmService = IViewModelManager.Instance;
                vmService.InitViewModels();

                stopwatch.Stop();
                startTrace.AppendFormatLine("init ViewModels {0}ms", stopwatch.ElapsedMilliseconds);
                stopwatch.Restart();

                Forms.Init(this, null);
                FormsMaterial.Init(this, null);
                ImageCircleRenderer.Init();

                stopwatch.Stop();
                startTrace.AppendFormatLine("init XF {0}ms", stopwatch.ElapsedMilliseconds);
                stopwatch.Restart();

                _Current = new();

                stopwatch.Stop();
                startTrace.AppendFormatLine("init XFApp {0}ms", stopwatch.ElapsedMilliseconds);
                stopwatch.Restart();
            }

            stopwatch.Stop();
            StartupTrack = startTrace.ToString();
#if DEBUG
            Log.Warn("Application", $"OnCreate Complete({stopwatch.ElapsedMilliseconds}ms");
#endif
        }

        static App? _Current;
        public static App Current => _Current ??
            throw new NullReferenceException("XFApplication Create in main process only.");

        public static string? StartupTrack { get; private set; }

        /// <summary>
        /// 当前是否是主进程
        /// </summary>
        public static bool IsMainProcess { get; private set; }

        const AppTheme _DefaultActualTheme = AppTheme.Light;
        AppTheme IApplication.DefaultActualTheme => _DefaultActualTheme;

        public bool IsRuntimeSwitchXFAppTheme { get; set; } = true;

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
                int defaultNightMode;
                OSAppTheme theme;
                switch (value)
                {
                    case AppTheme.Light:
                        defaultNightMode = AppCompatDelegate.ModeNightNo;
                        theme = OSAppTheme.Light;
                        break;
                    case AppTheme.Dark:
                        defaultNightMode = AppCompatDelegate.ModeNightYes;
                        theme = OSAppTheme.Dark;
                        break;
                    case AppTheme.FollowingSystem:
                        defaultNightMode = AppCompatDelegate.ModeNightFollowSystem;
                        theme = AppInfo.RequestedTheme switch
                        {
                            XEAppTheme.Light => OSAppTheme.Light,
                            XEAppTheme.Dark => OSAppTheme.Dark,
                            _ => OSAppTheme.Unspecified,
                        };
                        break;
                    default:
                        return;
                }
                if (IsRuntimeSwitchXFAppTheme && XFApplication.Current is App app)
                {
                    app.AppTheme = theme;
                }
                AppCompatDelegate.DefaultNightMode = defaultNightMode;
            }
        }

        AppTheme IApplication.GetActualThemeByFollowingSystem()
            => DarkModeUtil.IsDarkMode(this) ? AppTheme.Dark : AppTheme.Light;

        public static async void ShowUnderConstructionTips() => await MessageBox.ShowAsync(AppResources.UnderConstruction);

        bool IApplication.HasActiveWindow() => XEPlatform.CurrentActivity.HasValue();

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

        /// <inheritdoc cref="IApplication.InitSettingSubscribe"/>
        void InitSettingSubscribe()
        {
            ((IApplication)this).InitSettingSubscribe();
        }
    }
}