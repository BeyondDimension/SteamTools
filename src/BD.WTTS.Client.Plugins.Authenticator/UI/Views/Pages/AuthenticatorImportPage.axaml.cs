using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BD.WTTS.UI.Views.Pages;

public partial class AuthenticatorImportPage : UserControl
{
    public AuthenticatorImportPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}