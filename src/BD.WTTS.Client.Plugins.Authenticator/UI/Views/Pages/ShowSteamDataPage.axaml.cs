using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BD.WTTS.UI.Views.Pages;

public partial class ShowSteamDataPage : UserControl
{
    public ShowSteamDataPage()
    {
        InitializeComponent();
        //DataContext = new ShowSteamDataViewModel();
    }

    // public ShowSteamDataPage(IAuthenticatorDTO authenticatorDto)
    // {
    //     InitializeComponent();
    //     DataContext = new ShowSteamDataViewModel(authenticatorDto);
    // }
}