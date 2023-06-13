using Avalonia.Platform;
using BD.WTTS.Client.Resources;

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
            },
        };

        //AddGamePlatforms = new ObservableCollection<PlatformAccount>
        //{
        //    new PlatformAccount(ThirdpartyPlatform.Epic)
        //    {
        //        FullName = "Epic Games",
        //        Icon = "EpicGames",
        //    },
        //    new PlatformAccount(ThirdpartyPlatform.Uplay)
        //    {
        //        FullName = "Ubisoft",
        //        Icon = "Ubisoft",
        //    },
        //    new PlatformAccount(ThirdpartyPlatform.EADesktop)
        //    {
        //        FullName = "EA Desktop",
        //        Icon = "Ubisoft",
        //    },
        //};

        var temp = GetSupportPlatforms();
        if (temp != null)
        {
            if (GameAccountSettings.EnablePlatforms.Value.Any_Nullable())
            {
                AddGamePlatforms = new ObservableCollection<PlatformAccount>();

                foreach (var p in temp)
                {
                    if (GameAccountSettings.EnablePlatforms.Value.Contains(p.FullName))
                    {
                        GamePlatforms.Add(p);
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

        AddPlatformCommand = ReactiveCommand.Create<PlatformAccount>(AddPlatform);
    }

    public void AddPlatform(PlatformAccount platform)
    {
        GamePlatforms?.Add(platform);
        AddGamePlatforms?.Remove(platform);
        GameAccountSettings.EnablePlatforms.Value!.Add(platform.FullName);
        GameAccountSettings.EnablePlatforms.RaiseValueChanged();
    }

    public void RemovePlatform(PlatformAccount platform)
    {
        if (platform.FullName == "Steam")
        {
            Toast.Show(string.Format("不允许删除 {0} 平台", platform.FullName));
            return;
        }
        AddGamePlatforms?.Add(platform);
        GamePlatforms?.Remove(platform);
        GameAccountSettings.EnablePlatforms.Value!.Remove(platform.FullName);
        GameAccountSettings.EnablePlatforms.RaiseValueChanged();
    }

    readonly Uri PlatformsPath = new("avares://BD.WTTS.Client.Plugins.GameAccount/UI/Assets/Platforms.json");

    IEnumerable<PlatformAccount>? GetSupportPlatforms()
    {
        if (AssetLoader.Exists(PlatformsPath))
        {
            var stream = AssetLoader.Open(PlatformsPath);
            if (stream == null) return null;

            var platforms = JsonSerializer.Deserialize<PlatformAccount[]>(stream);
            return platforms;
        }
        return null;
    }
}
