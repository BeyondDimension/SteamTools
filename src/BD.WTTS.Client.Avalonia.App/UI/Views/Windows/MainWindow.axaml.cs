using FluentAvalonia.UI;
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
    }

    void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
