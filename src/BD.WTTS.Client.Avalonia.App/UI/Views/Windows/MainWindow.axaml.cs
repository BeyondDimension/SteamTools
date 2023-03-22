namespace BD.WTTS.UI.Views.Windows;

public sealed partial class MainWindow : AppWindow
{
    const string TAG = "MainWindow";

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        Width = 1080;
        Height = 660;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        //ExtendClientAreaToDecorationsHint = true;
        //ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome;
        TitleBar.Height = 40;
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint = WindowTransparencyLevel.Mica;

    }

    void InitializeComponent()
    {
        try
        {
            AvaloniaXamlLoader.Load(this);
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "load Xaml fail.");
        }
    }
}
