using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Pages;

public partial class AcceleratorPage : PageBase<AcceleratorPageViewModel>
{
    public AcceleratorPage()
    {
        InitializeComponent();
        DataContext = new AcceleratorPageViewModel();

        Title = Strings.CommunityFix;
        Subtitle = "插件作者: Steam++ 官方";
        Description = "提供一些游戏相关网站服务的加速功能。";
    }
}
