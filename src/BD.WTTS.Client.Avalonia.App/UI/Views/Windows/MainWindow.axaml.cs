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

    int IApplicationSplashScreen.MinimumShowTime => 1000;

    Task IApplicationSplashScreen.RunTasks(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}
