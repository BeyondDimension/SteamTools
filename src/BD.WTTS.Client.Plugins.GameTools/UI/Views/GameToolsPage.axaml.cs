using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class GameToolsPage : PageBase
{
    public GameToolsPage()
    {
        InitializeComponent();
    }

    private void GameToolsPage_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Control c && c.Tag is Type t)
        {
            NavigationService.Instance.Navigate(t, NavigationTransitionEffect.FromRight);
        }
        else
        {
            NavigationService.Instance.Navigate(null, NavigationTransitionEffect.FromRight);
        }
    }
}
