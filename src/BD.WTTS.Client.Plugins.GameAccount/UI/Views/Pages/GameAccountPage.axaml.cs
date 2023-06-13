using FluentAvalonia.UI.Controls;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Pages;

public partial class GameAccountPage : PageBase<GameAccountPageViewModel>
{
    public GameAccountPage()
    {
        InitializeComponent();
        DataContext = new GameAccountPageViewModel();

        Title = Strings.UserFastChange;
        Subtitle = "插件作者: Steam++ 官方";
        Description = "可支持自行添加多平台账号快速切换功能，Steam 可自动读取账号信息，其它平台请手动添加账号信息。 ";
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is PlatformAccount platform)
        {
            ViewModel?.RemovePlatform(platform);
        }
    }
}
