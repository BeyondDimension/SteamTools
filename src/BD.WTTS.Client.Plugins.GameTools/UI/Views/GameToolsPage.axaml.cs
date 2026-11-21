using Avalonia.Interactivity;

namespace BD.WTTS.UI.Views.Pages;

public partial class GameToolsPage : PageBase
{
    public GameToolsPage()
    {
        InitializeComponent();
    }

    private void BorderlessGame_Click(object? sender, RoutedEventArgs e) =>
        NavigationService.Instance.Navigate(typeof(BorderlessGamePage), NavigationTransitionEffect.FromRight);

    private void VacRepair_Click(object? sender, RoutedEventArgs e) =>
        NavigationService.Instance.Navigate(typeof(CsgoVacRepairPage), NavigationTransitionEffect.FromRight);
}
