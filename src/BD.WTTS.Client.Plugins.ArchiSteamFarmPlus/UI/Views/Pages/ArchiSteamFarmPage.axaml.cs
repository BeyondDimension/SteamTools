using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using BD.WTTS.UI.Views.Controls;

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

    private void ConsoleShell_CommandSubmit(object? sender, CommandEventArgs e)
    {
        ASFService.Current.ShellMessageInput(e.Command);
    }
}
