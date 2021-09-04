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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Application.UI.Views.Windows;
using System.Application.Services.Implementation;
using System.Threading.Tasks;
using System.Application.Security;
using Avalonia;
using Avalonia.Platform;
using FluentAvalonia.Styling;
using Avalonia.Media;
#if WINDOWS
//using WpfApplication = System.Windows.Application;
#endif

[assembly: Guid("82cda250-48a2-48ad-ab03-5cda873ef80c")]
namespace System.Application.UI
{
    public partial class App : AvaloniaApplication, IDisposableHolder, IDesktopAppService, IDesktopAvaloniaAppService
    {
        public static App Instance => Current is App app ? app : throw new Exception("Impossible");

        //public static DirectoryInfo RootDirectory => new(IOPath.BaseDirectory);

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
#if DEBUG
                        dps.SetLightOrDarkThemeFollowingSystem(true);
#endif
                        if (switch_value == mTheme) goto setValue;
                    }
                }
                else if (mTheme == AppTheme.FollowingSystem)
                {
                    var dps = DI.Get<IDesktopPlatformService>();
#if DEBUG
                    dps.SetLightOrDarkThemeFollowingSystem(false);
#endif
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
                Mode = mode,
            };
            Styles[1] = new StyleInclude(uri_1)
            {
                Source = uri_1,
            };

            AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().RequestedTheme = the;
        }

        public void SetThemeAccent(string? colorHex)
        {
            if (colorHex == null)
            {
                return;
            }
            var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();

            if (Color.TryParse(colorHex, out var color))
            {
                thm.CustomAccentColor = color;
            }
            else
            {
                if (colorHex.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
                {
                    thm.CustomAccentColor = null;
                }
            }

            if (OperatingSystem2.IsWindows)
            {
                if (OperatingSystem2.IsWindowsVersionAtLeast(6, 2))
                    thm.GetUserAccentColor = true;
                else
                    thm.GetUserAccentColor = false;
            }
            else
            {
                thm.GetUserAccentColor = true;
            }
        }

        readonly Dictionary<string, ICommand> mNotifyIconMenus = new();

        public IReadOnlyDictionary<string, ICommand> NotifyIconMenus => mNotifyIconMenus;

        public override void Initialize()
        {
#if StartupTrace
            StartupTrace.Restart("App.Initialize");
#endif
            AvaloniaXamlLoader.Load(this);
#if StartupTrace
            StartupTrace.Restart("App.LoadXAML");
#endif
            Name = ThisAssembly.AssemblyTrademark;
            ViewModelBase.IsInDesignMode = ApplicationLifetime == null;
            if (ViewModelBase.IsInDesignMode) Startup.Init(DILevel.MainProcess);
#if StartupTrace
            StartupTrace.Restart("App.SetP");
#endif
            //SettingsHost.Load();
            var windowService = IWindowService.Instance;
            windowService.Init();
            DI.Get<IDesktopPlatformService>().SetSystemSessionEnding(() => Shutdown());
#if StartupTrace
            StartupTrace.Restart("WindowService.Init");
#endif

#if StartupTrace
            StartupTrace.Restart("SettingsHost.Init");
#endif
            Theme = (AppTheme)UISettings.Theme.Value;
#if StartupTrace
            StartupTrace.Restart("Theme");
#endif

#if DEBUG
            //实时切换主题有BUG暂且屏蔽
            UISettings.Theme.Subscribe(x => Theme = (AppTheme)x);
#endif
            UISettings.ThemeAccent.Subscribe(x => SetThemeAccent(x));
            UISettings.GetUserThemeAccent.Subscribe(x => SetThemeAccent(x ? bool.TrueString : UISettings.ThemeAccent.Value));
            UISettings.Language.Subscribe(x => R.ChangeLanguage(x));
            GeneralSettings.InitWindowsStartupAutoRun();
#if StartupTrace
            StartupTrace.Restart("UISettings.Subscribe");
#endif
            switch (windowService.MainWindow)
            {
                case AchievementWindowViewModel window:
                    Program.IsMinimize = false;
                    MainWindow = new AchievementWindow
                    {
                        DataContext = windowService.MainWindow,
                    };
                    break;

                default:
                    #region 主窗口启动时加载的资源
#if !UI_DEMO
                    compositeDisposable.Add(SettingsHost.Save);
                    compositeDisposable.Add(ProxyService.Current.Dispose);
                    compositeDisposable.Add(SteamConnectService.Current.Dispose);
                    compositeDisposable.Add(ASFService.Current.StopASF);
                    if (GeneralSettings.IsStartupAppMinimized.Value)
                    {
                        Program.IsMinimize = true;
                        //if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                        //    desktop.MainWindow = null;
                    }
#endif
                    #endregion
                    MainWindow = new MainWindow
                    {
                        DataContext = windowService.MainWindow,
                    };
                    break;
            }
#if StartupTrace
            StartupTrace.Restart("Set MainWindow");
#endif
        }

        public ContextMenu? NotifyIconContextMenu { get; private set; }

        public override void OnFrameworkInitializationCompleted()
        {
            if (Program.IsTrayProcess)
            {
                base.OnFrameworkInitializationCompleted();
                return;
            }

            // 在UI预览中，ApplicationLifetime 为 null
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                //#if MAC
                //                AppDelegate.Init();
                //#endif

                if (Program.IsMainProcess)
                {
                    if (Startup.HasNotifyIcon)
                    {
                        if (!OperatingSystem2.IsLinux)
                        {
                            (var notifyIcon, var menuItemDisposable) = NotifyIconHelper.Init(NotifyIconHelper.GetIconByCurrentAvaloniaLocator);
                            notifyIcon.Click += NotifyIcon_Click;
                            notifyIcon.DoubleClick += NotifyIcon_Click;
                            if (menuItemDisposable != null) menuItemDisposable.AddTo(this);
                            notifyIcon.AddTo(this);
                        }
                        else
                        {
#if LINUX || DEBUG
                            NotifyIconHelper.StartPipeServer();
#endif
                        }
                    }
#if WINDOWS
                    JumpLists.Init();
#endif
                }

                IWindowService.Instance.MainWindow.Initialize();

                desktop.MainWindow =
#if !UI_DEMO
                    Program.IsMinimize ? null :
#endif
                    MainWindow;

                desktop.Startup += Desktop_Startup;
                desktop.Exit += ApplicationLifetime_Exit;
                desktop.ShutdownMode =
#if UI_DEMO
                    ShutdownMode.OnMainWindowClose;
#else
                Startup.HasNotifyIcon ? ShutdownMode.OnExplicitShutdown : ShutdownMode.OnMainWindowClose;
#endif
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        /// override RegisterServices register custom service
        /// </summary>
        public override void RegisterServices()
        {
            AvaloniaLocator.CurrentMutable.Bind<IFontManagerImpl>().ToConstant(new CustomFontManagerImpl());
            base.RegisterServices();
        }

        void Desktop_Startup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            var isOfficialChannelPackage = IsNotOfficialChannelPackageDetectionHelper.Check(Program.IsMainProcess);

#if StartupTrace
            StartupTrace.Restart("Desktop_Startup.Start");
#endif
            IsNotOfficialChannelPackageDetectionHelper.Check();
#if WINDOWS
            if (isOfficialChannelPackage)
            {
                VisualStudioAppCenterSDK.Init();
            }
#endif
#if StartupTrace
            StartupTrace.Restart("AppCenterSDK.Init");
#endif
            AppHelper.Initialized?.Invoke();
#if StartupTrace
            StartupTrace.Restart("Desktop_Startup.AppHelper.Initialized?");
#endif
            Startup.OnStartup(Program.IsMainProcess);
#if StartupTrace
            if (Program.IsMainProcess)
            {
                StartupTrace.Restart("Desktop_Startup.MainProcess");
            }
#endif

            StartupToastIntercept.OnStartuped();
#if StartupTrace
            StartupTrace.Restart("Desktop_Startup.SetIsStartuped");
#endif
        }

        void ApplicationLifetime_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
#if LINUX || DEBUG
            NotifyIconHelper.StopPipeServer();
#endif

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
            AppHelper.TryShutdown();
        }

        internal void NotifyIcon_Click(object? sender, EventArgs e)
        {
            RestoreMainWindow();
        }

        Task IClipboardPlatformService.PlatformSetTextAsync(string text) => Current.Clipboard.SetTextAsync(text);

        Task<string> IClipboardPlatformService.PlatformGetTextAsync() => Current.Clipboard.GetTextAsync();

        bool IClipboardPlatformService.PlatformHasText
        {
            get
            {
                Func<Task<string>> func = () => Current.Clipboard.GetTextAsync();
                var value = func.RunSync();
                return !string.IsNullOrEmpty(value);
            }
        }

        Window? mMainWindow;

        public Window MainWindow
        {
            get => mMainWindow ?? throw new ArgumentNullException(nameof(mMainWindow));
            set => mMainWindow = value;
        }

        AvaloniaApplication IDesktopAvaloniaAppService.Current => Current;

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
                if (mainWindow == null)
                {
                    mainWindow = MainWindow;
                    desktop.MainWindow = MainWindow;
                }
            }

            if (mainWindow == null)
            {
                throw new ArgumentNullException(nameof(mainWindow));
            }

            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
            mainWindow.BringIntoView();
            mainWindow.ActivateWorkaround(); // Extension method hack because of https://github.com/AvaloniaUI/Avalonia/issues/2975
            mainWindow.Focus();

            //// Again, ugly hack because of https://github.com/AvaloniaUI/Avalonia/issues/2994
            //mainWindow.Width += 0.1;
            //mainWindow.Width -= 0.1;
        }

        public bool HasActiveWindow()
        {
            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.Windows.Any_Nullable(x => x.IsActive))
                {
                    return true;
                }
            }
            return false;
        }

        public Window GetActiveWindow()
        {
            if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var activeWindow = desktop.Windows.FirstOrDefault(x => x.IsActive);
                if (activeWindow != null)
                {
                    return activeWindow;
                }
            }
            return MainWindow;
        }

        public void SetDesktopBackgroundWindow()
        {
            if (OperatingSystem2.IsWindows)
            {
                var window = (App.Instance.MainWindow as MainWindow);
                DI.Get<ISystemWindowApiService>().SetDesktopBackgroundToWindow(
                    window._backHandle, (int)App.Instance.MainWindow.Width, (int)App.Instance.MainWindow.Height);
            }
        }


        /// <summary>
        /// Exits the app by calling <c>Shutdown()</c> on the <c>IClassicDesktopStyleApplicationLifetime</c>.
        /// </summary>
        public static bool Shutdown(int exitCode = 0)
        {
            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainThread2.BeginInvokeOnMainThread(() =>
                {
                    desktop.Shutdown(exitCode);
                });
                return true;
            }
            return false;
        }

        void IDesktopAppService.Shutdown() => Shutdown();

        bool IDesktopAppService.IsCefInitComplete => false;
        //CefNetApp.InitState == CefNetAppInitState.Complete;

        #region IDisposable members

        public readonly CompositeDisposable compositeDisposable = new();

        CompositeDisposable IDesktopAppService.CompositeDisposable => compositeDisposable;

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => compositeDisposable;

        void IDisposable.Dispose()
        {
            compositeDisposable.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}