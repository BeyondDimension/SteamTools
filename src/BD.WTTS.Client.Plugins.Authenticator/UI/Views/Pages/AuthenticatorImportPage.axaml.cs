using Avalonia.Controls;
using Avalonia.ReactiveUI;
using BD.WTTS.UI.Views.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace BD.WTTS.UI.Views.Pages;

public partial class AuthenticatorImportPage : ReactiveUserControl<AuthenticatorImportPageViewModel>
{
    public static Frame? InnerFrame { get; set; }

    public AuthenticatorImportPage()
    {
        InitializeComponent();
        DataContext ??= new AuthenticatorImportPageViewModel();

        InnerNavFrame.Navigate(typeof(AuthenticatorImportMethodSelect));
        InnerFrame = InnerNavFrame;
    }
}