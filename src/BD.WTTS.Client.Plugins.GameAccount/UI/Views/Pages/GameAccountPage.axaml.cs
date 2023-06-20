using FluentAvalonia.UI.Controls;
using BD.WTTS.Client.Resources;
using Avalonia.Controls;

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

        //this.WhenActivated(disposable =>
        //{
        //    ViewModel?.LoadPlatforms();
        //});
    }

    private void TabView_SelectedItemChanged(object sender, SelectionChangedEventArgs args)
    {
        foreach (var item in args.AddedItems)
        {
            if (item is PlatformAccount platform)
            {
                IWindowManager.Instance.ShowTaskDialogAsync(new PlatformSettingsViewModel(), $"设置 {platform.FullName} 平台程序路径", pageContent: new Controls.PlatformSettings());
            }
        }
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is PlatformAccount platform)
        {
            ViewModel?.RemovePlatform(platform);
        }
    }
}
