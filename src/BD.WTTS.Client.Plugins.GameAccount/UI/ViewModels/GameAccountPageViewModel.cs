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

        AddGamePlatforms = new ObservableCollection<PlatformAccount>
        {
            new PlatformAccount(ThirdpartyPlatform.Epic)
            {
                FullName = "Epic Games",
                Icon = "EpicGames",
            },
            new PlatformAccount(ThirdpartyPlatform.Uplay)
            {
                FullName = "Ubisoft",
                Icon = "Ubisoft",
            },
            new PlatformAccount(ThirdpartyPlatform.EADesktop)
            {
                FullName = "EA Desktop",
                Icon = "Ubisoft",
            },
        };

        AddPlatformCommand = ReactiveCommand.Create<PlatformAccount>(AddPlatform);
    }

    void AddPlatform(PlatformAccount platform)
    {
        GamePlatforms?.Add(platform);
        AddGamePlatforms?.Remove(platform);
    }
}
