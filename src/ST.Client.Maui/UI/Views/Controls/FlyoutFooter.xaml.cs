namespace System.Application.UI.Views.Controls;

public partial class FlyoutFooter : ContentView
{
    public FlyoutFooter()
    {
        InitializeComponent();
    }

    public static string FooterText { get; } = $"Powered by .NET {Environment.Version}";
}