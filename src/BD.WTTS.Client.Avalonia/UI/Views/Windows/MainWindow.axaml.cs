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
            DataContext ??= GetMainWinodwViewModel();

#if DEBUG
        if (Design.IsDesignMode)
            Design.SetDataContext(this, MainWindow.GetMainWinodwViewModel());
#endif
    }

    //protected override void OnClosing(WindowClosingEventArgs e)
    //{
    //    if (GeneralSettings.IsEnableTrayIcon.Value)
    //    {
    //        e.Cancel = true;
    //        Hide();
    //    }
    //    base.OnClosing(e);
    //}

    public static MainWindowViewModel GetMainWinodwViewModel()
    {
#pragma warning disable SA1114 // Parameter list should follow declaration
        return new MainWindowViewModel(new TabItemViewModel[]
        {
                 new MenuTabItemViewModel()
                 {
                    ResourceKeyOrName = "Welcome",
                    PageType = typeof(HomePage),
                    IsResourceGet = true,
                    IconKey = "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/home.ico",
                 },
        }, ImmutableArray.Create<TabItemViewModel>(
#if DEBUG
            new MenuTabItemViewModel()
            {
                ResourceKeyOrName = "Debug",
                PageType = typeof(DebugPage),
                IsResourceGet = false,
                IconKey = "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/bug.ico",
            },
            new MenuTabItemViewModel()
            {
                ResourceKeyOrName = "Plugin_Store",
                PageType = null,
                IsResourceGet = true,
                IconKey = "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/store.ico",
            },
#endif
            new MenuTabItemViewModel()
            {
                ResourceKeyOrName = "Settings",
                PageType = typeof(SettingsPage),
                IsResourceGet = true,
                IconKey = "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/settings.ico",
            }
        ));
#pragma warning restore SA1114 // Parameter list should follow declaration
    }
}

public sealed class AppSplashScreen : IApplicationSplashScreen
{
    public static bool IsInitialized = false;

    public WindowViewModel? ViewModel { get; }

    public string? AppName { get; }

    public IImage? AppIcon { get; }

    public object? SplashScreenContent => new SplashScreen();

    int IApplicationSplashScreen.MinimumShowTime =>
#if DEBUG
        3000;
#else
        0;
#endif

    Task IApplicationSplashScreen.RunTasks(CancellationToken token)
    {
        return Task.Run(async () =>
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

             var mainWindow = App.Instance.MainWindow;
             mainWindow.ThrowIsNull();

             var mainWindowVM = MainWindow.GetMainWinodwViewModel();

             Dispatcher.UIThread.Post(() =>
             {
                 mainWindow.DataContext = mainWindowVM;
                 s.InitSettingSubscribe();
             });
#if STARTUP_WATCH_TRACE || DEBUG
             WatchTrace.Record("InitMainWindowViewModel");
#endif

#if STARTUP_WATCH_TRACE || DEBUG
             WatchTrace.Stop();
#endif
             s.OnStartup();
             await mainWindowVM!.Initialize();
             IsInitialized = true;
         }, cancellationToken: token);
    }
}
