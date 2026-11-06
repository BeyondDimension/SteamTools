using Avalonia.Controls;
using ReactiveUI.Avalonia;
using BD.WTTS.UI.Views.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace BD.WTTS.UI.Views.Pages;

public partial class AuthenticatorImportPage : ReactiveUserControl<AuthenticatorImportPageViewModel>
{
    internal Frame InnerFrame => InnerNavFrame;

    public AuthenticatorImportPage()
    {
        InitializeComponent();
        DataContext ??= new AuthenticatorImportPageViewModel();

        InnerNavFrame.Navigate(typeof(AuthenticatorImportMethodSelect));
    }
}