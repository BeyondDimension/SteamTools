#pragma warning disable CA1416 // 验证平台兼容性
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System.Application.Mvvm;
using System.Application.UI.ViewModels;
using System.Application.UI.Views;
using System.Collections.Generic;
using System.IO;
using System.Properties;
using System.Windows;
using AvaloniaApplication = Avalonia.Application;
using ShutdownMode = Avalonia.Controls.ShutdownMode;
using Window = Avalonia.Controls.Window;
using WindowState = Avalonia.Controls.WindowState;
using Avalonia.Themes.Fluent;
using Avalonia.Markup.Xaml.Styling;
using System.Application.Services;
using System.Windows.Input;
using System.Linq;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Application.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;
#if WINDOWS
//using WpfApplication = System.Windows.Application;
#endif
using APIConst = System.Application.Services.CloudService.Constants;

[assembly: AssemblyTitle(ThisAssembly.AssemblyTrademark + " v" + ThisAssembly.Version)]
namespace System.Application.UI
{
    public partial class App : AvaloniaApplication, IDisposableHolder, IDesktopAppService, IDesktopAvaloniaAppService
    {
        public static App Instance => Current is App app ? app : throw new Exception("Impossible");

        [Obsolete("use IOPath.AppDataDirectory", true)]
        public DirectoryInfo LocalAppData => new(IOPath.AppDataDirectory);

        public static DirectoryInfo RootDirectory => new(AppContext.BaseDirectory);

        [Obsolete("use AppHelper.ProgramName", true)]
        public string ProgramName => AppHelper.ProgramName;

        AppTheme mTheme = AppTheme.Dark;
        public AppTheme Theme
        {
            get
            {
                return mTheme;
            }
            set
            {
                static AppTheme GetAppThemeByIsLightOrDarkTheme(bool isLightOrDarkTheme) => isLightOrDarkTheme ? AppTheme.Light : AppTheme.Dark;

                if (value == mTheme) return;
                AppTheme switch_value = value;

                if (value == AppTheme.FollowingSystem)
                {
                    var dps = DI.Get<IDesktopPlatformService>();
                    var isLightOrDarkTheme = dps.IsLightOrDarkTheme;
                    if (isLightOrDarkTheme.HasValue)
                    {
                        switch_value = GetAppThemeByIsLightOrDarkTheme(isLightOrDarkTheme.Value);
                        dps.SetLightOrDarkThemeFollowingSystem(true);
                        if (switch_value == mTheme) goto setValue;
                    }
                }
                else if (mTheme == AppTheme.FollowingSystem)
                {
                    var dps = DI.Get<IDesktopPlatformService>();
                    dps.SetLightOrDarkThemeFollowingSystem(false);
                    var isLightOrDarkTheme = dps.IsLightOrDarkTheme;
                    if (isLightOrDarkTheme.HasValue)
                    {
                        var mThemeFS = GetAppThemeByIsLightOrDarkTheme(isLightOrDarkTheme.Value);
                        if (mThemeFS == switch_value) goto setValue;
                    }
                }

                SetThemeNotChangeValue(switch_value);

            setValue: mTheme = value;
            }
        }

        public void SetThemeNotChangeValue(AppTheme value)
        {
            string? the;
            FluentThemeMode mode;

            switch (value)
            {
                case AppTheme.Light:
                    the = "Light";
                    mode = FluentThemeMode.Light;
                    break;
                case AppTheme.Dark:
                    the = "Dark";
                    mode = FluentThemeMode.Dark;
                    break;
                default:
                    the = "Dark";
                    mode = FluentThemeMode.Dark;
                    break;
            }

            var uri_0 = new Uri($"avares://Avalonia.Themes.Fluent/Fluent{the}.xaml");
            var uri_1 = new Uri($"avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Styles/Theme{the}.xaml");

            Styles[0] = new FluentTheme(uri_0)
            {
                Mode = mode
            };
            Styles[1] = new StyleInclude(uri_1)
            {
                Source = uri_1,
            };
        }

        readonly Dictionary<string, ICommand> mNotifyIconMenus = new();

        public IReadOnlyDictionary<string, ICommand> NotifyIconMenus => mNotifyIconMenus;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            Name = ThisAssembly.AssemblyTrademark;
            ViewModelBase.IsInDesignMode = ApplicationLifetime == null;
            Startup.Init(true);

            #region 启动时加载的资源

            SettingsHost.Load();
            compositeDisposable.Add(SettingsHost.Save);
            Theme = (AppTheme)UISettings.Theme.Value;
            UISettings.Theme.Subscribe(x => Theme = (AppTheme)x);
            UISettings.Language.Subscribe(x => R.ChangeLanguage(x));

            #endregion

            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        public ContextMenu? NotifyIconContextMenu { get; private set; }

        static void IsNotOfficialChannelPackageWarning()
        {
            var text = APIConst.IsNotOfficialChannelPackageWarning;
            var title = AppResources.Warning;
            MessageBoxCompat.Show(text, title, MessageBoxButtonCompat.OK, MessageBoxImageCompat.Warning);
        }

        [Conditional("DEBUG")]
        static void CheckDebugPackageReference()
        {
            if (!CheckDebugPackageReference_())
            {
                var text = "错误：依赖包 [System.Reactive] 版本不符合发布要求";
                var title = AppResources.Warning;
                MessageBoxCompat.Show(text, title, MessageBoxButtonCompat.OK, MessageBoxImageCompat.Warning);
            }
            static bool CheckDebugPackageReference_()
            {
#nullable disable
                var assemblySystemReactive = Assembly.Load("System.Reactive");
                var fileVersion = assemblySystemReactive.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
                if (fileVersion != "5.0.0.1")
                {
                    return false;
                }
                var infoVersion = assemblySystemReactive.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                if (infoVersion != "5.0.0+103c252a0e")
                {
                    return false;
                }
#nullable enable
                return true;
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // 在UI预览中，ApplicationLifetime 为 null
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
#if MAC
                AppDelegate.Init();
#endif

                #region NotifyIcon

                var notifyIcon = INotifyIcon.Instance;
                notifyIcon.ToolTipText = ThisAssembly.AssemblyTrademark;
                switch (DI.Platform)
                {
                    case Platform.Windows:
                    case Platform.Linux:
                        notifyIcon.IconPath = "avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/Icon.ico";
                        break;
                    case Platform.Apple:
                        notifyIcon.IconPath = "avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/Icon_16.png";
                        break;
                }

                notifyIcon.DoubleClick += (s, e) =>
                {
                    RestoreMainWindow();
                };

                NotifyIconContextMenu = new ContextMenu();

                mNotifyIconMenus.Add("Light", ReactiveCommand.Create(() =>
                {
                    Theme = AppTheme.Light;
                }));

                mNotifyIconMenus.Add("Dark", ReactiveCommand.Create(() =>
                {
                    Theme = AppTheme.Dark;
                }));

                mNotifyIconMenus.Add("Show",
                    ReactiveCommand.Create(RestoreMainWindow));

                mNotifyIconMenus.Add("Exit",
                    ReactiveCommand.Create(() => Shutdown()));

                NotifyIconContextMenu.Items = mNotifyIconMenus
                    .Select(x => new MenuItem { Header = x.Key, Command = x.Value }).ToList();
                notifyIcon.ContextMenu = NotifyIconContextMenu;
                notifyIcon.Visible = true;
                notifyIcon.Click += NotifyIcon_Click;
                notifyIcon.DoubleClick += NotifyIcon_Click;
                compositeDisposable.Add(() =>
                {
                    notifyIcon.IconPath = string.Empty;
                });
                #endregion

#if WINDOWS
                JumpLists.Init();
#endif

                if (!AppHelper.IsOfficialChannelPackage)
                {
                    IsNotOfficialChannelPackageWarning();
                }

                CheckDebugPackageReference();

                desktop.MainWindow = MainWindow;
                desktop.Startup += Desktop_Startup;
                desktop.Exit += ApplicationLifetime_Exit;
                desktop.ShutdownMode =
#if UI_DEMO
                    ShutdownMode.OnMainWindowClose;
#else
                    ShutdownMode.OnExplicitShutdown;
#endif
            }

            base.OnFrameworkInitializationCompleted();
        }

        void Desktop_Startup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            ActiveUserPost(ActiveUserType.OnStartup);

            AppHelper.Initialized?.Invoke();

#if WINDOWS
            var options = DI.Get_Nullable<IOptions<AppSettings>>();

            var appSecret = options?.Value.AppSecretVisualStudioAppCenter;
            if (!string.IsNullOrWhiteSpace(appSecret))
            {
                VisualStudioAppCenterSDK.Init(appSecret);
            }
#endif
        }

        void ApplicationLifetime_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            try
            {
                compositeDisposable.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error("Shutdown", ex, "compositeDisposable.Dispose()");
            }
#if WINDOWS
            //WpfApplication.Current.Shutdown();
#endif
            AppHelper.Shutdown?.Invoke();
        }

        void NotifyIcon_Click(object? sender, EventArgs e)
        {
            RestoreMainWindow();
        }

        public async void SetClipboardText(string s) => await Current.Clipboard.SetTextAsync(s);

        Window? mMainWindow;

        public Window MainWindow
        {
            get => mMainWindow ?? throw new ArgumentNullException(nameof(mMainWindow));
            set => mMainWindow = value;
        }

        public AvaloniaApplication CurrentApp => Current;

        /// <summary>
        /// Restores the app's main window by setting its <c>WindowState</c> to
        /// <c>WindowState.Normal</c> and showing the window.
        /// </summary>
        public void RestoreMainWindow()
        {
            Window? mainWindow = null;

            if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                mainWindow = desktop.MainWindow;
            }

            if (mainWindow == null)
            {
                mainWindow = MainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            }

            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.BringIntoView();
            mainWindow.ActivateWorkaround(); // Extension method hack because of https://github.com/AvaloniaUI/Avalonia/issues/2975
            mainWindow.Focus();

            // Again, ugly hack because of https://github.com/AvaloniaUI/Avalonia/issues/2994
            mainWindow.Width += 0.1;
            mainWindow.Width -= 0.1;
        }

        /// <summary>
        /// Exits the app by calling <c>Shutdown()</c> on the <c>IClassicDesktopStyleApplicationLifetime</c>.
        /// </summary>
        public static bool Shutdown(int exitCode = 0)
        {
            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown(exitCode);
                return true;
            }
            return false;
        }

        void IDesktopAppService.Shutdown() => Shutdown();

        bool IDesktopAppService.IsCefInitComplete => CefNetApp.InitState == CefNetAppInitState.Complete;

        #region IDisposable members

        public readonly CompositeDisposable compositeDisposable = new();

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => compositeDisposable;

        void IDisposable.Dispose()
        {
            compositeDisposable.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

        internal static async void ActiveUserPost(ActiveUserType type)
        {
            try
            {
                var screens = Instance.MainWindow.Screens;
                var req = new ActiveUserRecordDTO
                {
                    Type = type,
                    ScreenCount = screens.ScreenCount,
                    PrimaryScreenPixelDensity = screens.Primary.PixelDensity,
                    PrimaryScreenWidth = screens.Primary.Bounds.Width,
                    PrimaryScreenHeight = screens.Primary.Bounds.Height,
                    SumScreenWidth = screens.All.Sum(x => x.Bounds.Width),
                    SumScreenHeight = screens.All.Sum(x => x.Bounds.Height),
                };
                var rsp = await ICloudServiceClient.Instance.ActiveUser.Post(req);
            }
            catch (Exception e)
            {
                Log.Error(nameof(App), e, "ActiveUserPost");
            }
        }
    }
}
#pragma warning restore CA1416 // 验证平台兼容性