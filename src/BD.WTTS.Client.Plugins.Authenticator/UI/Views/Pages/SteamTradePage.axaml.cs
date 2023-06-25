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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}