using FluentAvalonia.Styling;
using FluentAvalonia.UI;
using FluentAvalonia.UI.Media;
using FluentAvalonia.UI.Windowing;

namespace BD.WTTS.UI.Views.Windows;

public sealed partial class MainWindow : AppWindow
{
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
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint = WindowTransparencyLevel.Mica;

    }

    void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
