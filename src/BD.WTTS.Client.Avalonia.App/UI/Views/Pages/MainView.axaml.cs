namespace BD.WTTS.UI.Views.Pages;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        DebugButton.Click += (s, e) =>
        {
            var window = new DebugWindow();
            window.Show();
        };
    }
}
