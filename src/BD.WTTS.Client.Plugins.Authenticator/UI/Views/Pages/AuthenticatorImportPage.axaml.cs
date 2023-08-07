using Avalonia.Controls;
using Avalonia.ReactiveUI;
using BD.WTTS.UI.Views.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace BD.WTTS.UI.Views.Pages;

public partial class AuthenticatorImportPage : ReactiveUserControl<AuthenticatorImportPageViewModel>
{
    public AuthenticatorImportPage()
    {
        InitializeComponent();
        DataContext ??= new AuthenticatorImportPageViewModel();
    }

    private void AuthenticatorImportPage_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (sender is Control item && item.Tag is Type t)
        {
            InnerNavFrame?.Navigate(t, null, new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromRight,
                FromHorizontalOffset = 70,
            });
        }
    }
}