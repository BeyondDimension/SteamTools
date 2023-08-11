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
        if (sender is AppItem item && item.Tag is AuthenticatorImportMethod importMethod && item.ClickCommand == null && AuthenticatorImportPage.InnerFrame != null)
        {
            AuthenticatorImportPage.InnerFrame.Navigate(importMethod.PageType, null, new SlideNavigationTransitionInfo
            {
                Effect = SlideNavigationTransitionEffect.FromRight,
                FromHorizontalOffset = 70,
            });

            if (importMethod.PageType == typeof(AuthenticatorGeneralImportPage) && importMethod.Platform != null && AuthenticatorImportPage.InnerFrame.Content is Control content)
            {
                content.DataContext = new AuthenticatorGeneralImportPageViewModel(importMethod.Platform.Value);
            }
        }
    }
}
