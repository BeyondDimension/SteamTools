using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        var debug = this.FindControl<Button>("DebugButton");

        if (debug != null)
            debug.Click += (s, e) =>
            {
                var window = new DebugWindow();
                window.Show();
            };
    }
}
