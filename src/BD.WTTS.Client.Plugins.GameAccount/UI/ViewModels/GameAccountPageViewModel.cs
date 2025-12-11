using AppResources = BD.WTTS.Client.Resources.Strings;
using SJsonSerializer = System.Text.Json.JsonSerializer;
using Avalonia.Platform;
using BD.WTTS.UI.Views.Pages;
using BD.WTTS.Helpers;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class GameAccountPageViewModel
{
    public GameAccountPageViewModel()
    {
        GamePlatforms = new ObservableCollection<PlatformAccount>
        {
            new PlatformAccount(ThirdpartyPlatform.Steam)
            {
                FullName = "Steam",
                Icon = "Steam",
                DefaultExePath = SteamSettings.SteamProgramPath.Value,
            },
        };

        AddPlatformCommand = ReactiveCommand.Create<PlatformAccount>(AddPlatform);
        LoginNewCommand = ReactiveCommand.Create(LoginNewUser);
        SaveCurrentUserCommand = ReactiveCommand.Create(SaveCurrentUser);
        RefreshCommand = ReactiveCommand.Create(() => SelectedPlatform?.LoadUsers());
        ShareManageCommand = ReactiveCommand.Create(async () =>
        {
            var vm = new SteamFamilyShareManagePageViewModel();
            var r = await IWindowManager.Instance.ShowTaskDialogAsync(vm, vm.Title, pageContent: new SteamFamilyShareManagePage(),
                isOkButton: true, isCancelButton: true, okButtonText: Strings.Save, moreInfoText: Strings.AccountChange_ShareManageAboutTips);

            if (r == true)
            {
                vm.SetActivity_Click();
            }
        });

        LoadPlatforms();

        this.WhenAnyValue(x => x.SelectedPlatform)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(IsSelectedSteam));
            });
    }

    public void AddPlatform(PlatformAccount platform)
    {
        GamePlatforms?.Add(platform);
        AddGamePlatforms?.Remove(platform);
        GameAccountSettings.EnablePlatforms.Add(platform.FullName);
    }

    public void RemovePlatform(PlatformAccount platform)
    {
        if (platform.FullName == "Steam")
        {
            Toast.Show(ToastIcon.Warning, AppResources.Warning_PlatformDeletionNotAllowed_.Format(platform.FullName));
            return;
        }
        AddGamePlatforms?.Add(platform);
        GamePlatforms?.Remove(platform);
        GameAccountSettings.EnablePlatforms.Remove(platform.FullName);
    }

    IEnumerable<PlatformAccount>? GetSupportPlatforms()
    {
        if (AssetLoader.Exists(PlatformsPath))
        {
            var stream = AssetLoader.Open(PlatformsPath);
            if (stream == null) return null;

            var platforms = SJsonSerializer.Deserialize<PlatformAccount[]>(stream);
            if (platforms == null) return null;

            foreach (var platform in platforms)
            {
                if (platform.ClearPaths?.Contains("SAME_AS_LOGIN_FILES") == true && platform.LoginFiles != null)
                {
                    platform.ClearPaths = platform.LoginFiles.Keys.ToList();
                }
                if (GameAccountSettings.PlatformSettings.ContainsKey(platform.FullName))
                {
                    platform.PlatformSetting = GameAccountSettings.PlatformSettings.Value?[platform.FullName];
                }
            }
            return platforms;
        }
        return null;
    }

    public void LoadPlatforms()
    {
        GamePlatforms?[0].LoadUsers();

        if (!OperatingSystem2.IsWindows())
        {
            AddGamePlatforms = [];
            return;
        }

        var temp = GetSupportPlatforms();
        if (temp != null)
        {
            if (GameAccountSettings.EnablePlatforms.Any_Nullable())
            {
                AddGamePlatforms = [];

                foreach (var p in temp)
                {
                    if (GameAccountSettings.EnablePlatforms.Contains(p.FullName))
                    {
                        if (GamePlatforms?.Contains(p) == false)
                        {
                            GamePlatforms.Add(p);
                            p.LoadUsers();
                        }
                    }
                    else
                    {
                        AddGamePlatforms.Add(p);
                    }
                }
            }
            else
            {
                AddGamePlatforms = new ObservableCollection<PlatformAccount>(temp);
            }
        }
    }

    async void SaveCurrentUser()
    {
        if (!CheckPlatformStatus(SelectedPlatform)) return;
        var textModel = new TextBoxWindowViewModel();
        var result = await IWindowManager.Instance.ShowTaskDialogAsync(textModel, AppResources.Title_AddAccount_.Format(SelectedPlatform?.FullName), subHeader: AppResources.Title_PleaseInputCurrentAccountName_.Format(SelectedPlatform.FullName), isCancelButton: true);
        if (result)
        {
            if (string.IsNullOrEmpty(textModel.Value))
            {
                Toast.Show(ToastIcon.Warning, AppResources.Warning_PleaseInputAccountName);
                return;
            }
            var userAddResult = await SelectedPlatform.CurrnetUserAdd(textModel.Value);
            if (userAddResult)
            {
                Toast.Show(ToastIcon.Success, AppResources.Success_SavedSuccessfully_.Format(textModel.Value));
                SelectedPlatform.LoadUsers();
            }
        }
    }

    async void LoginNewUser()
    {
        if (!CheckPlatformStatus(SelectedPlatform)) return;
        var textModel = new MessageBoxWindowViewModel
        {
            Content = AppResources.ModelContent_LoginNewUser,
        };
        var result = await IWindowManager.Instance.ShowTaskDialogAsync(textModel, AppResources.Title_LoginAccount_.Format(SelectedPlatform!.FullName), isCancelButton: true);
        if (result && SelectedPlatform != null)
            await SelectedPlatform.CurrnetUserAdd(null);
    }

    static bool CheckPlatformStatus(PlatformAccount? platform)
    {
        if (platform == null) return false;
        if (platform.Platform == ThirdpartyPlatform.Steam) return true;
        if (!File.Exists(platform.ExePath))
        {
            Toast.Show(ToastIcon.Error, Strings.Error_UnableSwitchPlatformAccount_.Format(platform.FullName));
            return false;
        }
        return true;
    }
}
