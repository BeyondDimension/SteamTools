using Avalonia.Controls;
using ReactiveUI.Avalonia.Reactive;

namespace BD.WTTS.UI.Views.Pages;

public partial class IdleSteamLoginPage : ReactiveUserControl<IdleSteamLoginPageViewModel>
{
    public IdleSteamLoginPage()
    {
        InitializeComponent();
        //DataContext ??= new IdleSteamLoginPageViewModel();
    }
}
