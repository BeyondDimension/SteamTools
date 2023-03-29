namespace BD.WTTS.UI.Views.Windows;

public sealed partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();

        Width = 1080;
        Height = 660;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        //ExtendClientAreaToDecorationsHint = true;
        //ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome;
        this.TryFindResource("TitleBarHeight", App.Instance.RequestedThemeVariant, out object? titleBarHeight);
        TitleBar.Height = (double?)titleBarHeight ?? 70;
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint = WindowTransparencyLevel.Mica;
    }
}
