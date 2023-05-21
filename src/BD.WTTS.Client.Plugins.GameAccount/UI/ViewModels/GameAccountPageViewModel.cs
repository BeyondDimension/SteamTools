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
            },
            new PlatformAccount
            {
                FullName = "Epic Games",
                Icon = "EpicGames",
            },
            new PlatformAccount
            {
                FullName = "Ubisoft",
                Icon = "Ubisoft",
            },
        };
    }
}
