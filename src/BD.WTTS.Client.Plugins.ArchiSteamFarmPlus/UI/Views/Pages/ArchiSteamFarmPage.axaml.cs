using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

namespace BD.WTTS.UI.Views.Pages;

public partial class ArchiSteamFarmPage : PageBase<ArchiSteamFarmPlusPageViewModel>
{
    public ArchiSteamFarmPage()
    {
        InitializeComponent();
        DataContext ??= new ArchiSteamFarmPlusPageViewModel();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

    }
}
