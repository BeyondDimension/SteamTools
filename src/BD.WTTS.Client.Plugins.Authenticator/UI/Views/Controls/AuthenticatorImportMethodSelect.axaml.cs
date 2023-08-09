using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using BD.WTTS.UI.Views.Pages;
using FluentAvalonia.UI.Media.Animation;

namespace BD.WTTS.UI.Views.Controls;

public partial class AuthenticatorImportMethodSelect : UserControl
{
    public AuthenticatorImportMethodSelect()
    {
        InitializeComponent();
    }

    private void AuthenticatorImportPage_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (sender is Control item && item.Tag is Type t)
        {
            AuthenticatorImportPage.InnerFrame?.Navigate(t, null, new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromRight,
                FromHorizontalOffset = 70,
            });
        }
    }
}
