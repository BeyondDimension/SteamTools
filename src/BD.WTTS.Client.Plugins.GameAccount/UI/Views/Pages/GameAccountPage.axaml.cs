using AppResources = BD.WTTS.Client.Resources.Strings;

using FluentAvalonia.UI.Controls;
using Avalonia.Controls;
using BD.WTTS.UI.Views.Controls;
using System.Runtime.Devices;
using BD.WTTS.Helpers;

namespace BD.WTTS.UI.Views.Pages;

public partial class GameAccountPage : PageBase<GameAccountPageViewModel>
{
    public GameAccountPage()
    {
        InitializeComponent();
        this.SetViewModel<GameAccountPageViewModel>(true);
        //DataContext ??= new GameAccountPageViewModel();

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

                TracepointHelper.TrackEvent("GameAccountPlatformLoad", new Dictionary<string, string> {
                    { "Name", platform.Platform.ToString() },
                });

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

    private async void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is PlatformAccount platform)
        {
            if (platform.FullName == "Steam")
            {
                Toast.Show(ToastIcon.Warning, AppResources.Warning_PlatformDeletionNotAllowed_.Format(platform.FullName));
                return;
            }
            var isOK = await MessageBox.ShowAsync(AppResources.Message_AreYouSureYouWantToDeleteTheAccount.Format(platform.FullName), button: MessageBox.Button.OKCancel);

            if (isOK == MessageBox.Result.OK)
                ViewModel?.RemovePlatform(platform);
        }
    }
}
