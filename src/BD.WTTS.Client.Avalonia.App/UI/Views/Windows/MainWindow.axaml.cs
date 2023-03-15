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
        MinWidth = 1080;
        MinHeight = 660;

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
