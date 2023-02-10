namespace BD.WTTS.UI.Views.Windows;

public sealed partial class MainWindow : Window
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
