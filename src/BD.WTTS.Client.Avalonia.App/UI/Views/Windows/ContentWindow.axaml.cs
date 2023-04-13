using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Windows;

public partial class ContentWindow : AppWindow
{
    public ContentWindow()
    {
        InitializeComponent();

        Width = 1080;
        Height = 660;

        this.TryFindResource("TitleBarHeight", App.Instance.RequestedThemeVariant, out object? titleBarHeight);
        TitleBar.Height = (double?)titleBarHeight ?? 70;
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint = WindowTransparencyLevel.Mica;
    }
}
