using static BD.WTTS.Startup;

namespace BD.WTTS.UI.Views.Windows;

public sealed partial class MainWindow : ReactiveAppWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        SplashScreen = new AppSplashScreen();
    }
}

public sealed class AppSplashScreen : IApplicationSplashScreen
{
    public AppSplashScreen()
    {
        SplashScreenContent = new SplashScreen();
    }

    public WindowViewModel? ViewModel { get; }

    public string? AppName { get; }

    public IImage? AppIcon { get; }

    public object? SplashScreenContent { get; }

    int IApplicationSplashScreen.MinimumShowTime => 0;

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

#pragma warning disable SA1114 // Parameter list should follow declaration
             var mainWindowVM = new MainWindowViewModel(new TabItemViewModel[]
             {
                 new MenuTabItemViewModel()
                 {
                    ResourceKeyOrName = "Welcome",
                    PageType = typeof(HomePage),
                    IsResourceGet = true,
                    IconKey = "Home",
                 },
             }, ImmutableArray.Create<TabItemViewModel>(
#if DEBUG
                 new MenuTabItemViewModel()
                 {
                     ResourceKeyOrName = "Debug",
                     PageType = typeof(DebugPage),
                     IsResourceGet = false,
                     IconKey = "Bug",
                 },
#endif   
                 new MenuTabItemViewModel()
                 {
                     ResourceKeyOrName = "Settings",
                     PageType = typeof(SettingsPage),
                     IsResourceGet = true,
                     IconKey = "Settings",
                 }
             ));
#pragma warning restore SA1114 // Parameter list should follow declaration

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
         });
    }
}
