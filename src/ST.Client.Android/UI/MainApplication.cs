#if !NET6_0_OR_GREATER
using Android.App;
using Android.Runtime;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Application.Mvvm;
using System.Diagnostics;
using System.Text;
using Xamarin.Essentials;
using System.Application.Settings;
using AndroidX.AppCompat.App;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Windows.Input;
using System.Collections.Generic;
using System.Reactive.Disposables;
#if __XAMARIN_FORMS__
using Xamarin.Forms;
#endif
//#if DEBUG
//using Square.LeakCanary;
//#endif
#if __XAMARIN_FORMS__
using ImageCircle.Forms.Plugin.Droid;
using XFApplication = Xamarin.Forms.Application;
#endif
using AppTheme = System.Application.Models.AppTheme;
using XEAppTheme = Xamarin.Essentials.AppTheme;
using XEFileProvider = Xamarin.Essentials.FileProvider;
using XEPlatform = Xamarin.Essentials.Platform;
using XEVersionTracking = Xamarin.Essentials.VersionTracking;

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
        public static MainApplication Instance => Context is MainApplication app ? app : throw new ArgumentNullException(nameof(app));

        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
#if __XAMARIN_FORMS__
            // https://github.com/xamarin/Xamarin.Forms/blob/release-5.0.0-sr7/Xamarin.Forms.Platform.Android/Resources/Layout/RootLayout.axml
            // https://github.com/xamarin/Xamarin.Forms/blob/release-5.0.0-sr7/Xamarin.Forms.Platform.Android/Renderers/ShellSectionRenderer.cs
            // 替换 RootLayout.axml 中的 Toolbar 修复一些样式问题，更换 ShellSectionRenderer 类会导致子页面无法加载，或许需要继承自，但只能通过反射修改 layout
            Xamarin.Forms.Platform.Android.Resource.Layout.RootLayout = Resource.Layout.xf_root_layout;
#endif
            //#if NET6_0_OR_GREATER
            //            Microsoft.Net.Http.Headers.DateTimeFormatterPatch.Init();
            //#endif
        }

        //#if DEBUG
        //RefWatcher? _refWatcher;

        //void SetupLeakCanary()
        //{
        //    // “A small leak will sink a great ship.” - Benjamin Franklin
        //    if (LeakCanaryXamarin.IsInAnalyzerProcess(this))
        //    {
        //        // This process is dedicated to LeakCanary for heap analysis.
        //        // You should not init your app in this process.
        //        return;
        //    }
        //    _refWatcher = LeakCanaryXamarin.Install(this);
        //}
        //#endif

        public override void OnCreate()
        {
            base.OnCreate();
#if SHADOWSOCKS
            Shadowsocks.Core.Instance.Init(this);
#endif
            //#if DEBUG
            //            //SetupLeakCanary();
            //#endif

            const bool isTrace = true;

            bool GetIsMainProcess()
            {
                // 注意：进程名可以自定义，默认是包名，如果自定义了主进程名，这里就有误，所以不要自定义主进程名！
                var name = this.GetCurrentProcessName();
                return name == PackageName;
            }
            IsMainProcess = GetIsMainProcess();
            if (isTrace) StartWatchTrace.Record("IsMainProcess");

            OnCreateAppExecuting(isTrace);

            XEFileProvider.TemporaryLocation = FileProviderLocation.Internal;
            XEPlatform.Init(this); // 初始化 Xamarin.Essentials.Platform.Init

            XEPlatform.ActivityStateChanged += OnActivityStateChanged;
            if (isTrace) StartWatchTrace.Record("Essentials");

            var level = DILevel.Min;
            if (IsMainProcess) level = DILevel.MainProcess;
            IApplication.IProgramHost host = this;
            host.ConfigureServices(level, isTrace);
            if (isTrace) StartWatchTrace.Record("Startup");

            if (IsMainProcess)
            {
                XEVersionTracking.Track();
                if (isTrace) StartWatchTrace.Record("VersionTracking");

                if (XEVersionTracking.IsFirstLaunchForCurrentVersion)
                {
                    // 当前版本第一次启动时，清除存放升级包缓存文件夹的目录
                    IApplicationUpdateService.ClearAllPackCacheDir();
                    if (isTrace) StartWatchTrace.Record("ClearAllPackCacheDir");
                }
            }

            OnCreateAppExecuted(this, isTrace: isTrace);

            host.OnStartup();
            if (isTrace) StartWatchTrace.Record("OnStartup");

            if (IsMainProcess)
            {
#if __XAMARIN_FORMS__
                Forms.Init(this, null);
                FormsMaterial.Init(this, null);
                ImageCircleRenderer.Init();
                if (isTrace) StartWatchTrace.Record("XF");

                _Current = new(RealTheme);
                if (isTrace) StartWatchTrace.Record("XFApp");
#endif
            }

            StartupTrack = StartWatchTrace.ToString();
#if DEBUG
            Log.Warn("Application", $"OnCreate Complete({StartWatchTrace.ElapsedMilliseconds}ms");
#endif
        }

        static readonly HashSet<Android.App.Activity> activities = new();

        public static IReadOnlyCollection<Android.App.Activity> Activities => activities;

        void OnActivityStateChanged(object? sender, ActivityStateChangedEventArgs e)
        {
            switch (e.State)
            {
                case ActivityState.Created:
                    activities.Add(e.Activity);
                    break;
                case ActivityState.Resumed:
                    break;
                case ActivityState.Paused:
                    break;
                case ActivityState.Destroyed:
                    activities.Remove(e.Activity);
                    break;
                case ActivityState.SaveInstanceState:
                    break;
                case ActivityState.Started:
                    break;
                case ActivityState.Stopped:
                    break;
            }
        }

#if __XAMARIN_FORMS__
        static App? _Current;
        public static App Current => _Current ??
            throw new NullReferenceException("XFApplication Create in main process only.");
#endif

        public static string? StartupTrack { get; private set; }

        const AppTheme _DefaultActualTheme = AppTheme.Light;

        AppTheme IApplication.DefaultActualTheme => _DefaultActualTheme;

        //public bool IsRuntimeSwitchXFAppTheme { get; set; } = true;

#region AppTheme

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
#if __XAMARIN_FORMS__
                OSAppTheme theme;
#endif
                switch (value)
                {
                    case AppTheme.Light:
                        defaultNightMode = AppCompatDelegate.ModeNightNo;
#if __XAMARIN_FORMS__
                        theme = OSAppTheme.Light;
#endif
                        break;
                    case AppTheme.Dark:
                        defaultNightMode = AppCompatDelegate.ModeNightYes;
#if __XAMARIN_FORMS__
                        theme = OSAppTheme.Dark;
#endif
                        break;
                    case AppTheme.FollowingSystem:
                        defaultNightMode = AppCompatDelegate.ModeNightFollowSystem;
#if __XAMARIN_FORMS__
                        theme = AppInfo.RequestedTheme switch
                        {
                            XEAppTheme.Light => OSAppTheme.Light,
                            XEAppTheme.Dark => OSAppTheme.Dark,
                            _ => OSAppTheme.Unspecified,
                        };
#endif
                        break;
                    default:
                        return;
                }
#if __XAMARIN_FORMS__
                if (IsRuntimeSwitchXFAppTheme && XFApplication.Current is App app)
                {
                    app.AppTheme = theme;
                }
#endif
                AppCompatDelegate.DefaultNightMode = defaultNightMode;
            }
        }

        AppTheme ActualThemeByFollowingSystem => DarkModeUtil.IsDarkMode(this) ? AppTheme.Dark : AppTheme.Light;

        AppTheme IApplication.GetActualThemeByFollowingSystem() => ActualThemeByFollowingSystem;

        public AppTheme RealTheme
        {
            get
            {
                var value = Theme;
                return value switch
                {
                    AppTheme.Light or AppTheme.Dark => value,
                    _ => ActualThemeByFollowingSystem,
                };
            }
        }

#endregion

        public static async void ShowUnderConstructionTips() => await MessageBox.ShowAsync(AppResources.UnderConstruction);

        bool IApplication.HasActiveWindow() => XEPlatform.CurrentActivity.HasValue();

#region Compat

        void IApplication.SetThemeNotChangeValue(AppTheme value) => Theme = value;

        object IApplication.CurrentPlatformUIHost => XEPlatform.CurrentActivity;

        readonly Dictionary<string, ICommand> _NotifyIconMenus = new();

        IReadOnlyDictionary<string, ICommand> IApplication.NotifyIconMenus => _NotifyIconMenus;

        readonly CompositeDisposable compositeDisposable = new();

        CompositeDisposable IApplication.CompositeDisposable => compositeDisposable;

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => compositeDisposable;

#endregion

        /// <inheritdoc cref="IApplication.InitSettingSubscribe"/>
        void PlatformInitSettingSubscribe()
        {
            ((IApplication)this).InitSettingSubscribe();
        }

        void IApplication.PlatformInitSettingSubscribe() => PlatformInitSettingSubscribe();
    }
}
#endif