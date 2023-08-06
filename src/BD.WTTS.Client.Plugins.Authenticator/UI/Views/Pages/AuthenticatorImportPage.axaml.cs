using Avalonia.ReactiveUI;

namespace BD.WTTS.UI.Views.Pages;

public partial class AuthenticatorImportPage : ReactiveUserControl<AuthenticatorImportPageViewModel>
{
    public AuthenticatorImportPage()
    {
        InitializeComponent();
        DataContext ??= new AuthenticatorImportPageViewModel();
    }
}