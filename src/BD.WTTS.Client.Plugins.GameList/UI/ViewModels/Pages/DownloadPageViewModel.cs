namespace BD.WTTS.UI.ViewModels;

public sealed class DownloadPageViewModel : ViewModelBase
{
    public static string DisplayName => Strings.GameList_SteamShutdown;

    private readonly ReadOnlyObservableCollection<SteamApp> _DownloadingApps;
    private bool? isAllCheck;
    private bool changingSelection;

    public SteamConnectService SteamConnection { get; } = SteamConnectService.Current;

    public ReadOnlyObservableCollection<SteamApp> DownloadingApps => _DownloadingApps;

    public bool? IsAllCheck
    {
        get => isAllCheck;
        set
        {
            if (isAllCheck == value || value is not bool watch) return;

            changingSelection = true;
            try
            {
                foreach (var app in DownloadingApps)
                    if (app.IsWatchDownloading != watch)
                        app.IsWatchDownloading = watch;
            }
            finally
            {
                changingSelection = false;
                UpdateSelectionState();
            }
        }
    }

    public DownloadPageViewModel()
    {
        SteamConnection.DownloadApps
           .Connect()
           .Filter(p => p.IsDownloading)
           .Sort(SortExpressionComparer<SteamApp>.Ascending(x => x.AppId).ThenBy(x => x.DisplayName))
           .ObserveOn(RxSchedulers.MainThreadScheduler)
           .Bind(out _DownloadingApps)
           .Subscribe()
           .AddTo(this);

        DownloadingApps.ToObservableChangeSet()
            .AutoRefreshOnObservable(static app => app.ObserveValue(nameof(app.IsWatchDownloading), static x => x.IsWatchDownloading).Skip(1))
            .Subscribe(changes =>
            {
                var watchedIds = SteamConnection.WatchDownloadingSteamAppIds;
                foreach (var change in changes)
                {
                    // A finished download can leave this list before the service checks the watched ID.
                    // Collection changes only affect the checkbox presentation, not the watch list.
                    if (change.Reason != ListChangeReason.Refresh) continue;
                    var app = change.Item.Current;
                    if (app.IsWatchDownloading)
                        watchedIds.Add(app.AppId);
                    else
                        watchedIds.Remove(app.AppId);
                }
                if (!changingSelection)
                    UpdateSelectionState();
            })
            .AddTo(this);
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        var selected = 0;
        foreach (var app in DownloadingApps)
            if (app.IsWatchDownloading) selected++;
        this.RaiseAndSetIfChanged(ref isAllCheck, selected == 0 ? false : selected == DownloadingApps.Count ? true : null, nameof(IsAllCheck));
    }

    public override void Activation()
    {
        base.Activation();
        Task2.InBackground(() =>
        {
            if (!SteamConnection.IsWatchSteamDownloading)
                SteamConnection.InitializeDownloadGameList();
        });
    }
}
