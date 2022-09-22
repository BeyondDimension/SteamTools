namespace System.Application.UI.Views.Controls;

public partial class FlyoutHeader : ContentView
{
    public FlyoutHeader()
    {
        this.InitializeComponent();
    }

    void OnFlyoutHeaderTapped(object? sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = false;
    }
}