using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public partial class GameListPageViewModel : TabItemViewModel
{
    public override string Name => Strings.GameList;

    [Reactive]
    public bool IsOpenFilter { get; set; }

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

    public bool IsSteamAppsEmpty => !SteamConnectService.Current.SteamApps.Items.Any_Nullable() && !SteamConnectService.Current.IsLoadingGameList;

    [Reactive]
    public ObservableCollection<EnumModel<SteamAppType>>? AppTypeFiltres { get; set; }

    [Reactive]
    public IReadOnlyCollection<EnumModel<SteamAppType>>? EnableAppTypeFiltres { get; set; }

    public ICommand? RefreshAppCommand { get; }

    public ICommand? HideAppCommand { get; }

    public ICommand? IdleAppCommand { get; }

    public ICommand? SteamShutdownCommand { get; }

    public ICommand? SaveEditedAppInfoCommand { get; }
}
