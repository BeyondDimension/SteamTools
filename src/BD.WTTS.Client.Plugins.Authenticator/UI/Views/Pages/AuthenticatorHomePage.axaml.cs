using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BD.WTTS.UI.Views.Pages;

public partial class AuthenticatorHomePage : PageBase<AuthenticatorPageViewModel>
{
    public AuthenticatorHomePage()
    {
        InitializeComponent();
        DataContext = new AuthenticatorPageViewModel();

        Title = "云令牌";
        Subtitle = "插件作者：Steam++ 官方";
        Description = "提供令牌管理、加密、云同步等功能";
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}