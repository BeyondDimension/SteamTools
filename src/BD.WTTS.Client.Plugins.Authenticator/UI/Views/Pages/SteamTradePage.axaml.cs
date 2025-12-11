using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BD.WTTS.UI.Views.Pages;

public partial class SteamTradePage : UserControl
{
    public SteamTradePage()
    {
        InitializeComponent();

        //PasswordText.KeyUp += (_, e) =>
        //{
        //    if (e.Key != Avalonia.Input.Key.Return) return;
        //    if (DataContext is not SteamTradePageViewModel vm) return;
        //    _ = vm.Login();
        //    e.Handled = true;
        //};
    }

    // public SteamTradePage(IAuthenticatorDTO authenticatorDto)
    // {
    //     InitializeComponent();
    //     DataContext = new SteamTradePageViewModel(authenticatorDto);
    // }
}