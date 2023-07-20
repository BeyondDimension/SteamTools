using AppResources = BD.WTTS.Client.Resources.Strings;

using FluentAvalonia.UI.Controls;
using BD.WTTS.Client.Resources;
using Avalonia.Controls;
using BD.WTTS.UI.Views.Controls;
using AngleSharp.Text;

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

        PlatformSettingButton.Click += async (_, e) =>
        {
            if (ViewModel?.SelectedPlatform != null)
            {
                var model = new PlatformSettingsPageViewModel(ViewModel.SelectedPlatform);
                await IWindowManager.Instance.ShowTaskDialogAsync(model, AppResources.Title_SetUp_.Format(ViewModel.SelectedPlatform.FullName),
                    pageContent: new PlatformSettingsPage(),
                    isCancelButton: true,
                    cancelCloseAction: () =>
                    {
                        var cancel = !File.Exists(ViewModel.SelectedPlatform.PlatformSetting?.PlatformPath);
                        if (cancel)
                        {
                            Toast.Show(ToastIcon.Error, AppResources.Error_IncorrectPlatformPathSelection_.Format(ViewModel.SelectedPlatform.FullName));
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
                if (!await IWindowManager.Instance.ShowTaskDialogAsync(model, AppResources.Title_SetUp_.Format(platform.FullName),
                    subHeader: AppResources.SubHeader_FirstNeedToSetPlatformPath_.Format(platform.FullName),
                    pageContent: new PlatformSettingsPage(),
                    isCancelButton: true,
                    cancelCloseAction: () =>
                    {
                        var cancel = !File.Exists(platform.PlatformSetting?.PlatformPath);
                        if (cancel)
                        {
                            Toast.Show(ToastIcon.Error, AppResources.Error_IncorrectPlatformPathSelection_.Format(platform.FullName));
                        }
                        return cancel;
                    }))
                {
                    if (!File.Exists(platform.PlatformSetting?.PlatformPath))
                    {
                        Toast.Show(ToastIcon.Error, AppResources.Error_PathFailedUnableSwitchAccount_.Format(platform.FullName));
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
