using BD.WTTS.Plugins.Abstractions;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using static BD.WTTS.Startup;

namespace BD.WTTS.UI.Views.Windows;

public sealed partial class MainWindow : ReactiveAppWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        if (!AppSplashScreen.IsInitialized)
            SplashScreen = new AppSplashScreen();
        else
            DataContext ??= IViewModelManager.Instance.MainWindow;

#if DEBUG
        if (Design.IsDesignMode)
            Design.SetDataContext(this, IViewModelManager.Instance.MainWindow!);
#endif
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (GeneralSettings.TrayIcon.Value)
        {
            e.Cancel = true;
            Hide();
        }
        base.OnClosing(e);

        if (Steamworks.SteamClient.IsValid)
        {
            Steamworks.SteamFriends.ClearRichPresence();
        }
    }

    DateTime lastOpenedTime;

    internal bool SetStartDefaultPageName()
    {
        if (NavigationService.Instance.CurrnetPage != null)
            return false;

        var mw2 = IViewModelManager.Instance.MainWindow2;
        if (mw2 == null)
            return false;

        Type? pageType = null;
        if (!string.IsNullOrEmpty(UISettings.StartDefaultPageName.Value))
        {
            var tabItems = mw2?.TabItems;
            if (tabItems == null)
                return false;

            var page = tabItems.OfType<MenuTabItemViewModel>()
                        .FirstOrDefault(s => s.Id == UISettings.StartDefaultPageName.Value);
            if (page != null)
            {
                pageType = page.PageType;
            }
        }
        INavigationService.Instance.Navigate(pageType ?? typeof(HomePage));
        return true;
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (Steamworks.SteamClient.IsValid)
        {
            Steamworks.SteamFriends.SetRichPresence("steam_display", "#Status_AtMainMenu");
        }
        if (AppSplashScreen.IsInitialized)
        {
            SetStartDefaultPageName();
        }
        if (lastOpenedTime == default ||
            (DateTime.Now - lastOpenedTime) > TimeSpan.FromMinutes(30))
        {
            lastOpenedTime = DateTime.Now;
            await IViewModelManager.Instance.Get<HomePageViewModel>().GetServerContent();
            await AdvertiseService.Current.RefrshAdvertiseAsync();
            await NoticeService.Current.GetNewsAsync();
        }
        try // 在主窗口显示时调用此函数检查是否需要显示新版本通知窗口
        {
            Ioc.Get_Nullable<IAppUpdateService>()?.OnMainOpenTryShowNewVersionWindow();
        }
        catch
        {

        }
    }
}

public sealed class AppSplashScreen : IApplicationSplashScreen
{
    public static bool IsInitialized = false;

    //public WindowViewModel? ViewModel { get; }

    public string? AppName { get; }

    public IImage? AppIcon { get; }

    public object? SplashScreenContent => new SplashScreen();

    int IApplicationSplashScreen.MinimumShowTime =>
#if DEBUG
        1000;
#else
        0;
#endif

    Task IApplicationSplashScreen.RunTasks(CancellationToken token)
    {
        return Task.Run(
            async () =>
            {
                if (!AppSplashScreen.IsInitialized)
                {
#if STARTUP_WATCH_TRACE || DEBUG
                    WatchTrace.Start();
#endif

                    var s = Instance;
                    if (s.IsMainProcess)
                    {
                        VersionTracking2.Track();
#if STARTUP_WATCH_TRACE || DEBUG
                        WatchTrace.Record("VersionTracking2.Track");
#endif

                        Migrations.Up();
#if STARTUP_WATCH_TRACE || DEBUG
                        WatchTrace.Record("Migrations.Up");
#endif

                        // 仅在主进程中启动 IPC 服务端
                        IPCMainProcessService.Instance.Run();
#if STARTUP_WATCH_TRACE || DEBUG
                        WatchTrace.Record("IPC.StartServer");
#endif
                    }

                    LiveCharts.Configure(config =>
                    {
                        config
                            // registers SkiaSharp as the library backend
                            // REQUIRED unless you build your own
                            .AddSkiaSharp();
                        // adds the default supported types
                        // OPTIONAL but highly recommend
                        //.AddDefaultMappers()

                        // select a theme, default is Light
                        // OPTIONAL
                        //.AddDarkTheme()

                        // In case you need a non-Latin based font, you must register a typeface for SkiaSharp
                        config.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('汉')); // <- Chinese // mark
                                                                                               //.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('أ'))  // <- Arabic // mark
                                                                                               //.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('あ')) // <- Japanese // mark
                                                                                               //.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('헬')) // <- Korean // mark
                                                                                               //.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('Ж'))  // <- Russian // mark

                        if (App.Instance.Theme != AppTheme.FollowingSystem)
                        {
                            if (App.Instance.Theme == AppTheme.Light)
                                config.AddLightTheme();
                            else
                                config.AddDarkTheme();
                        }
                        else
                        {
                            var dps = IPlatformService.Instance;
                            var isLightOrDarkTheme = dps.IsLightOrDarkTheme;
                            if (isLightOrDarkTheme.HasValue)
                            {
                                var mThemeFS = IApplication.GetAppThemeByIsLightOrDarkTheme(isLightOrDarkTheme.Value);
                                if (mThemeFS == AppTheme.Light)
                                    config.AddLightTheme();
                                else
                                    config.AddDarkTheme();
                            }
                        }
                    });

                    AdvertiseService.Current.InitAdvertise();
                    await NoticeService.Current.GetNewsAsync();

                    var mainWindow = App.Instance.MainWindow;
                    mainWindow.ThrowIsNull();

#pragma warning disable SA1114 // Parameter list should follow declaration
                    IViewModelManager.Instance.InitViewModels(new TabItemViewModel[]
                    {
                        new MenuTabItemViewModel("Welcome")
                        {
                           PageType = typeof(HomePage),
                           IsResourceGet = true,
                           IconKey = "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/home.ico",
                        },
                    },
                    ImmutableArray.Create<TabItemViewModel>(
#if DEBUG
                    new MenuTabItemViewModel("Debug")
                    {
                        PageType = typeof(DebugPage),
                        IsResourceGet = false,
                        IconKey = "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/bug.ico",
                    },
#endif
                    new MenuTabItemViewModel("Plugin_Store")
                    {
                        PageType = typeof(PluginStorePage),
                        IsResourceGet = true,
                        IconKey = "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/store.ico",
                    },
                    new MenuTabItemViewModel("Settings")
                    {
                        PageType = typeof(SettingsPage),
                        IsResourceGet = true,
                        IconKey = "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/settings.ico",
                    }));
#pragma warning restore SA1114 // Parameter list should follow declaration
                    IViewModelManager.Instance.MainWindow.ThrowIsNull();

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        mainWindow.DataContext = IViewModelManager.Instance.MainWindow;
                        s.InitSettingSubscribe();
                        if (mainWindow is MainWindow mainWindow1)
                        {
                            mainWindow1.SetStartDefaultPageName();
                        }
                    });
#if STARTUP_WATCH_TRACE || DEBUG
                    WatchTrace.Record("InitMainWindowViewModel");
#endif

#if STARTUP_WATCH_TRACE || DEBUG
                    WatchTrace.Stop();
#endif
                    s.OnStartup();
                    await IViewModelManager.Instance.MainWindow.Initialize();

                    App.Instance.CompositeDisposable.Add(IViewModelManager.Instance.MainWindow);
                    App.Instance.CompositeDisposable.Add(SteamConnectService.Current.Dispose);

                    IsInitialized = true;
                }
            }, cancellationToken: token);
    }
}
