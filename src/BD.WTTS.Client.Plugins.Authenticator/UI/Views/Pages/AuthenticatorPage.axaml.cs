using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace BD.WTTS.UI.Views.Pages;
public partial class AuthenticatorPage : ReactiveUserControl<AuthenticatorPageViewModel>
{
    public AuthenticatorPage()
    {
        InitializeComponent();
        DataContext = new AuthenticatorPageViewModel();
    }
}
