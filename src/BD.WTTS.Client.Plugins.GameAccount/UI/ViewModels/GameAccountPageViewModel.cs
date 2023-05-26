using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class GameAccountPageViewModel
{
    public GameAccountPageViewModel()
    {
        GamePlatforms = new ObservableCollection<PlatformAccount>
        {
            new PlatformAccount
            {
                FullName = "Steam",
                Icon = "Steam",
                Platform = ThirdpartyPlatform.Steam,
            },
        };

        AddGamePlatforms = new ObservableCollection<PlatformAccount>
        {
            new PlatformAccount
            {
                FullName = "Epic Games",
                Icon = "EpicGames",
                Platform = ThirdpartyPlatform.Epic,
            },
            new PlatformAccount
            {
                FullName = "Ubisoft",
                Icon = "Ubisoft",
                Platform = ThirdpartyPlatform.Uplay,
            },
            new PlatformAccount
            {
                FullName = "EA Desktop",
                Icon = "Ubisoft",
                Platform = ThirdpartyPlatform.EADesktop,
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
