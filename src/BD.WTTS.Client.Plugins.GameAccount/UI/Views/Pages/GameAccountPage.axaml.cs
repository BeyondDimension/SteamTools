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
        DataContext ??= new GameAccountPageViewModel();

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
                            Toast.Show($"{ViewModel.SelectedPlatform.FullName} 平台路径没有正确选择");
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
                if (!await IWindowManager.Instance.ShowTaskDialogAsync(model,
                    $"{platform.FullName} 设置",
                    subHeader: $"第一次使用需要先设置 {platform.FullName} 平台路径",
                    pageContent: new PlatformSettingsPage(),
                    isCancelButton: true,
                    cancelCloseAction: () =>
                    {
                        var cancel = !File.Exists(platform.PlatformSetting?.PlatformPath);
                        if (cancel)
                        {
                            Toast.Show($"{platform.FullName} 平台路径没有正确选择");
                        }
                        return cancel;
                    }))
                {
                    if (!File.Exists(platform.PlatformSetting?.PlatformPath))
                    {
                        Toast.Show($"路径没有正确选择，{platform.FullName} 平台账号切换功能无法使用");
                    }
                }
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
