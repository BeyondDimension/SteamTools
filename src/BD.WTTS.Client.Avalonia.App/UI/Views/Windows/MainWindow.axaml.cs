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

public class AppSplashScreen : IApplicationSplashScreen
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
        var mainWindowVM = new MainWindowViewModel();
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
