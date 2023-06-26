using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Pages;

public partial class ScriptPage : PageBase<ScriptPageViewModel>
{
    public ScriptPage()
    {
        InitializeComponent();
        DataContext = new ScriptPageViewModel();
        
        Title = Strings.ScriptConfig;
        Subtitle = "插件作者: Steam++ 官方";
        Description = "提供一些游戏相关网站服务的加速功能。";
    }
}
