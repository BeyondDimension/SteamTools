using FluentAvalonia.UI.Controls;
using BD.WTTS.Client.Resources;
using Avalonia.Controls;
using BD.WTTS.UI.Views.Controls;

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

        PluginSettingButton.Click += async (_, e) =>
        {
            if (ViewModel?.SelectedPlatform != null)
            {
                var model = new PlatformSettingsPageViewModel(ViewModel.SelectedPlatform);
                await IWindowManager.Instance.ShowTaskDialogAsync(model,
                    $"{ViewModel.SelectedPlatform.FullName} 设置",
                    pageContent: new PlatformSettingsPage(),
                    isCancelButton: true,
                    cancelCloseAction: () =>
                    {
                        var cancel = !File.Exists(ViewModel.SelectedPlatform.PlatformSetting?.PlatformPath);
                        if (cancel)
                        {
                            Toast.Show("路径没有正确选择");
                        }
                        return cancel;
                    });
            }
        };
    }

    private async void TabView_SelectedItemChanged(object sender, SelectionChangedEventArgs args)
    {
        foreach (var item in args.AddedItems)
        {
            if (item is PlatformAccount platform && !File.Exists(platform.ExePath))
            {
                var model = new PlatformSettingsPageViewModel(platform);
                await IWindowManager.Instance.ShowTaskDialogAsync(model,
                    $"{platform.FullName} 设置",
                    subHeader: $"第一次使用需要先设置 {platform.FullName} 平台路径",
                    pageContent: new PlatformSettingsPage(),
                    isCancelButton: true,
                    cancelCloseAction: () =>
                    {
                        var cancel = !File.Exists(platform.PlatformSetting?.PlatformPath);
                        if (cancel)
                        {
                            Toast.Show("路径没有正确选择");
                        }
                        return cancel;
                    });
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
