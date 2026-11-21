using BD.WTTS.Enums;
using ReactiveUI.Avalonia;

namespace BD.WTTS.UI.Views.Pages;

public partial class AuthenticatorGeneralImportPage : ReactiveUserControl<AuthenticatorGeneralImportPageViewModel>
{
    public AuthenticatorGeneralImportPage()
    {
        InitializeComponent();
    }

    public AuthenticatorGeneralImportPage(AuthenticatorPlatform platform) : this()
    {
        DataContext = new AuthenticatorGeneralImportPageViewModel(platform);
    }
}