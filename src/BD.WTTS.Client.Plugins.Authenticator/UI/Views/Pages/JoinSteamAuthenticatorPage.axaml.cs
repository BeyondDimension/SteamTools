using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class JoinSteamAuthenticatorPage : UserControl
{
    public JoinSteamAuthenticatorPage()
    {
        InitializeComponent();

        DataContext = new JoinSteamAuthenticatorPageViewModel();
    }
}