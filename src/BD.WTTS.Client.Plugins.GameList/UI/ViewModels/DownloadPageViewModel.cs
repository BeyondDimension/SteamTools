namespace BD.WTTS.UI.ViewModels;

public sealed class DownloadPageViewModel : ViewModelBase
{
    public static string DisplayName => Strings.GameList_SteamShutdown;

    private readonly ReadOnlyObservableCollection<SteamApp>? _DownloadingApps;

    public ReadOnlyObservableCollection<SteamApp>? DownloadingApps => _DownloadingApps;

    [Reactive]
    public bool? IsAllCheck { get; set; }

    public DownloadPageViewModel()
    {
        SteamConnectService.Current.DownloadApps
           .Connect()
           .Filter(p => p.IsDownloading)
           .Sort(SortExpressionComparer<SteamApp>.Ascending(x => x.AppId).ThenBy(x => x.DisplayName))
           .ObserveOn(RxApp.MainThreadScheduler)
           .Bind(out _DownloadingApps)
           .Subscribe();

        this.WhenAnyValue(x => x.DownloadingApps)
            .Subscribe(items => items?
                    .ToObservableChangeSet()
                    //.DistinctUntilChanged()
                    .AutoRefresh(x => x.IsWatchDownloading)
                    .WhenValueChanged(x => x.IsWatchDownloading)
                    .Subscribe(_ =>
                    {
                        bool? b = null;
                        var ids = items.Where(s => s.IsWatchDownloading).Select(s => s.AppId).ToArray();
                        if (!items.Any_Nullable() || ids.Length == 0)
                            b = false;
                        else if (ids.Length == items.Count)
                            b = true;

                        if (this.IsAllCheck != b)
                        {
                            this.IsAllCheck = b;
                        }

                        SteamConnectService.Current.WatchDownloadingSteamAppIds.Clear();
                        foreach (var id in ids)
                            SteamConnectService.Current.WatchDownloadingSteamAppIds.Add(id);
                    }));

        this.WhenValueChanged(x => x.IsAllCheck, false)
            .Subscribe(x =>
            {
                if (DownloadingApps != null)
                    if (x == true)
                    {
                        foreach (var item in DownloadingApps)
                            if (!item.IsWatchDownloading)
                                item.IsWatchDownloading = true;
                    }
                    else if (x == false)
                    {
                        foreach (var item in DownloadingApps)
                            if (item.IsWatchDownloading)
                                item.IsWatchDownloading = false;
                    }
            });
    }

    public override void Activation()
    {
        base.Activation();
        Task2.InBackground(() =>
        {
            if (!SteamConnectService.Current.IsWatchSteamDownloading)
            {
                SteamConnectService.Current.InitializeDownloadGameList();
            }
        });
    }
}
