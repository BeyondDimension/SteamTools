namespace BD.WTTS.UI.Views.Pages;

public partial class MainView : UserControl
{
    const string TAG = "MainView";

    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        try
        {
            AvaloniaXamlLoader.Load(this);
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "load Xaml fail.");
        }

        var debug = this.FindControl<Button>("DebugButton");

        if (debug != null)
            debug.Click += (s, e) =>
            {
                var window = new DebugWindow();
                window.Show();
            };
    }
}
