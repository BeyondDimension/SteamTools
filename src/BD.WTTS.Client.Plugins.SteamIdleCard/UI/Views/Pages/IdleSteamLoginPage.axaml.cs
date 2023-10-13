using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace BD.WTTS.UI.Views.Pages;

public partial class IdleSteamLoginPage : ReactiveUserControl<IdleSteamLoginPageViewModel>
{
    public IdleSteamLoginPage()
    {
        InitializeComponent();
        //DataContext ??= new IdleSteamLoginPageViewModel();
    }
}
