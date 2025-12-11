using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class GameListPageViewModel : TabItemViewModel
{
    public override string Name => Strings.GameList;

    [Reactive]
    public bool IsInstalledFilter { get; set; }

    [Reactive]
    public bool IsCloudArchiveFilter { get; set; }

    [Reactive]
    public bool IsAppInfoOpen { get; set; }

    [Reactive]
    public SteamApp? SelectApp { get; set; }

    private readonly ReadOnlyObservableCollection<SteamApp>? _SteamApps;

    public ReadOnlyObservableCollection<SteamApp>? SteamApps => _SteamApps;

    [Reactive]
    public string? SearchText { get; set; }

    [Reactive]
    public ObservableCollection<EnumModel<SteamAppType>>? AppTypeFiltres { get; set; }

    [Reactive]
    public IReadOnlyCollection<EnumModel<SteamAppType>>? EnableAppTypeFiltres { get; set; }

    public ICommand RefreshAppCommand { get; }

    public ICommand ShowHideAppCommand { get; }

    public ICommand AddHideAppListCommand { get; }

    public ICommand AddAFKAppListCommand { get; }

    public ICommand InstallOrStartAppCommand { get; }

    public ICommand EditAppInfoClickCommand { get; }

    public ICommand ManageCloudArchive_ClickCommand { get; }

    public ICommand UnlockAchievement_ClickCommand { get; }

    public ICommand NavAppToSteamViewCommand { get; }

    public ICommand NavAppScreenshotToSteamViewCommand { get; }

    public ICommand OpenFolderCommand { get; }

    public ICommand OpenLinkUrlCommand { get; }
}
