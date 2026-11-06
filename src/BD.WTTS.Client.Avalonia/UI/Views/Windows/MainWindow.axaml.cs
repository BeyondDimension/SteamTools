using Avalonia.Data;
using BD.WTTS.Plugins.Abstractions;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using static BD.WTTS.Startup;

namespace BD.WTTS.UI.Views.Windows;

public sealed partial class MainWindow : ReactiveAppWindow<MainWindowViewModel>
{
    readonly List<WeakReference<Control>> trackedPages = new();
    bool closingForTray;
    MainWindowViewState? closingState;

    public MainWindow()
    {
        InitializeComponent();
        ClosingBehavior = WindowClosingBehavior.OwnerWindowOnly;
        _ = new MainWindowTrayLifetime(this);
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
        if (!closingForTray && GeneralSettings.TrayIcon.Value &&
            e.CloseReason is WindowCloseReason.WindowClosing or WindowCloseReason.Undefined)
        {
            e.Cancel = true;
            if (!HasProtectedUi())
                Hide();
        }
        base.OnClosing(e);

        if ((!e.Cancel || !IsVisible) && Steamworks.SteamClient.IsValid)
        {
            Steamworks.SteamFriends.ClearRichPresence();
        }
    }

    protected override bool DisposeViewModelOnClose => !closingForTray;

    internal void TrackPage(Control page)
    {
        for (var i = trackedPages.Count - 1; i >= 0; i--)
        {
            if (!trackedPages[i].TryGetTarget(out var tracked))
                trackedPages.RemoveAt(i);
            else if (ReferenceEquals(tracked, page))
                return;
        }
        trackedPages.Add(new WeakReference<Control>(page));
    }

    internal bool HasProtectedUi()
    {
        if (MainWindowUiActivity.IsActive)
            return true;
        if (App.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.Windows.Any(x => !ReferenceEquals(x, this) && x.PlatformImpl != null))
            return true;
        return this.GetVisualDescendants().OfType<SteamLoginView>()
            .Any(x => x.ViewModel?.HasPendingLogin == true);
    }

    bool HasUnboundInput()
    {
        foreach (var visual in this.GetVisualDescendants())
        {
            if (visual is AutoCompleteBox { Text.Length: > 0 } search &&
                BindingOperations.GetBindingExpressionBase(search, AutoCompleteBox.TextProperty) == null)
                return true;

            if (visual is TextBox { IsReadOnly: false, Text.Length: > 0 } text &&
                !text.GetVisualAncestors().OfType<AutoCompleteBox>().Any() &&
                BindingOperations.GetBindingExpressionBase(text, TextBox.TextProperty) == null)
                return true;
        }
        return false;
    }

    internal bool TryCloseForTray(WindowState lastVisibleState)
    {
        if (!AppSplashScreen.IsInitialized || IViewModelManager.Instance.MainWindow2?.IsInitialized != true ||
            HasProtectedUi() || HasUnboundInput() || Content is not MainView)
            return false;

        var state = NavigationService.Instance.CaptureFrameState();
        if (state == null)
            return false;
        foreach (var reference in trackedPages)
        {
            if (reference.TryGetTarget(out var page))
                state.Pages[page.GetType()] = TrayPageState.Capture(page);
        }
        state.Position = Position;
        state.Width = Width;
        state.Height = Height;
        state.WindowState = lastVisibleState;
        closingState = state;
        closingForTray = true;
        try
        {
            Close();
            return PlatformImpl == null;
        }
        finally
        {
            closingForTray = false;
            closingState = null;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (closingForTray && closingState is { } state)
        {
            if (Content is MainView view)
                view.SaveFrameState(state);
            AvaloniaToastServiceImpl.Instance.ReleaseSnackbarManager(this);
            TrayPageState.Set(state.Pages);
            App.Instance.ReleaseMainWindow(this, state);
        }
        base.OnClosed(e);
    }

    static DateTime lastOpenedTime;

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
    readonly object initializationSync = new();
    Task? initializationTask;


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
        lock (initializationSync)
            return initializationTask ??= RunTasksCore(token);
    }

    Task RunTasksCore(CancellationToken token)
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
                    NoticeService.Current.GetNews();

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
                    // new MenuTabItemViewModel("Plugin_Store")
                    // {
                    //     PageType = typeof(PluginStorePage),
                    //     IsResourceGet = true,
                    //     IconKey = "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/store.ico",
                    // },
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
                    var initialization = IViewModelManager.Instance.MainWindow.Initialize();
                    if (await Task.WhenAny(initialization, Task.Delay(TimeSpan.FromSeconds(3), token)) == initialization)
                    {
                        await initialization;
                    }
                    else
                    {
                        token.ThrowIfCancellationRequested();
                        _ = ObserveInitializationAsync(initialization);
                    }

                    App.Instance.CompositeDisposable.Add(IViewModelManager.Instance.MainWindow);
                    App.Instance.CompositeDisposable.Add(SteamConnectService.Current.Dispose);

                    IsInitialized = true;
                }
            }, cancellationToken: token);
    }

    static async Task ObserveInitializationAsync(Task initialization)
    {
        try
        {
            await initialization;
        }
        catch (Exception ex)
        {
            Log.Error(nameof(AppSplashScreen), ex, "Main window initialization failed after the splash screen closed.");
        }
    }
}
