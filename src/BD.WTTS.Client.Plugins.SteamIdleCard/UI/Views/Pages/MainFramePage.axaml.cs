using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using BD.SteamClient.Models;

namespace BD.WTTS.UI.Views.Pages;

public partial class MainFramePage : ReactiveUserControl<IdleCardPageViewModel>
{
    public MainFramePage()
    {
        InitializeComponent();
        DataContext ??= new IdleCardPageViewModel();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

    }
}
