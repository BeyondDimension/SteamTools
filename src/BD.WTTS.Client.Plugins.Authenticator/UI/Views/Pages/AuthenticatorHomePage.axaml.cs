using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BD.WTTS.UI.Views.Pages;

public partial class AuthenticatorHomePage : PageBase<AuthenticatorHomePageViewModel>
{
    public AuthenticatorHomePage()
    {
        InitializeComponent();
        DataContext ??= new AuthenticatorHomePageViewModel();
    }
}