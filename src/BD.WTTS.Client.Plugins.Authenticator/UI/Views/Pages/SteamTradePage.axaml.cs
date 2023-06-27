using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BD.WTTS.UI.Views.Pages;

public partial class SteamTradePage : UserControl
{
    public SteamTradePage()
    {
        InitializeComponent();
        DataContext = new SteamTradePageViewModel();
    }

    public SteamTradePage(IAuthenticatorDTO authenticatorDto)
    {
        InitializeComponent();
        DataContext = new SteamTradePageViewModel(authenticatorDto);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}