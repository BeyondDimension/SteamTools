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
        TitleBar.Height = 40;
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint = WindowTransparencyLevel.Mica;
    }
}
