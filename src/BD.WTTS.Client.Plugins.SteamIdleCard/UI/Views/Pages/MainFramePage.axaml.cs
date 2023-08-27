using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using BD.SteamClient.Models;

namespace BD.WTTS.UI.Views.Pages;

public partial class MainFramePage : ReactiveUserControl<IdleAppsPageViewModel>
{
    public MainFramePage()
    {
        InitializeComponent();
        DataContext ??= new IdleAppsPageViewModel();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

    }
}
