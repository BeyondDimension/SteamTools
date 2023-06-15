namespace BD.WTTS.UI.Views.Pages;

public partial class AuthenticatorPage : PageBase<AuthenticatorPageViewModel>
{
    public AuthenticatorPage()
    {
        InitializeComponent();
        DataContext = new AuthenticatorPageViewModel();

        Title = "云令牌";
        Subtitle = "插件作者：Steam++ 官方";
        Description = "提供令牌管理、加密、云同步等功能";
    }
}
