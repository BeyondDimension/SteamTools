using BD.WTTS.Client.Resources;
using System.Reactive;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class GameAccountPageViewModel : TabItemViewModel
{
    static readonly Uri PlatformsPath = new("avares://BD.WTTS.Client.Plugins.GameAccount/UI/Assets/Platforms.json");

    public override string Name => Strings.UserFastChange;

    [Reactive]
    public ObservableCollection<PlatformAccount>? GamePlatforms { get; set; }

    [Reactive]
    public ObservableCollection<PlatformAccount>? AddGamePlatforms { get; set; }

    [Reactive]
    public PlatformAccount? SelectedPlatform { get; set; }

    public bool IsSelectedSteam => SelectedPlatform?.FullName == nameof(ThirdpartyPlatform.Steam) == true;

    public ICommand AddPlatformCommand { get; }

    public ICommand LoginNewCommand { get; }

    public ICommand SaveCurrentUserCommand { get; }

    public ICommand RefreshCommand { get; }

    public ICommand ShareManageCommand { get; }
}
