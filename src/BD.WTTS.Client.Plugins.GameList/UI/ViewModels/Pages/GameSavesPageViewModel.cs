using System.Collections.ObjectModel;
using Avalonia.Threading;
using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.GameList.Saves;
using BD.WTTS.Services;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class GameSavesPageViewModel : ViewModelBase
{
    readonly GameSaveScanner scanner;
    readonly GameSaveBackupService backups;
    readonly GameSaveSettingsStore settingsStore;
    readonly Dictionary<uint, GameSaveGameItemViewModel> games = [];
    readonly ObservableCollectionExtended<GameSaveGameItemViewModel> filteredGames = [];
    GameSaveSettings settings;
    IReadOnlyList<GameSaveAccountInfo> accounts = [];
    GameSaveAccountInfo? selectedAccount;
    GameSaveAccountInfo? scannedAccount;
    ILookup<uint, GameSaveBackupSummary>? refreshedBackups;
    ScanSession? scan;
    CancellationTokenSource? accountRefresh;
    string? steamRootPath;
    string searchText = "";
    int filterIndex;
    int sortIndex;
    int selectedCount;
    int progressValue;
    int progressMaximum;
    int gamesWithDataCount;
    int installedGamesWithDataCount;
    bool settingsLoaded;
    bool hasScanned;
    bool rescanAfterMutation;
    bool updateInProgress;
    bool isScanningInstalled;
    bool isScanningBackground;
    bool isMutating;

    public GameSavesPageViewModel()
        : this(Ioc.Get<GameSaveScanner>(), Ioc.Get<GameSaveBackupService>(), Ioc.Get<GameSaveSettingsStore>())
    {
    }

    internal GameSavesPageViewModel(GameSaveScanner scanner, GameSaveBackupService backups, GameSaveSettingsStore settingsStore)
    {
        this.scanner = scanner;
        this.backups = backups;
        this.settingsStore = settingsStore;
        settings = new(settingsStore.DefaultBackupRootPath);
        FilteredGames = new(filteredGames);
    }

    public ReadOnlyObservableCollection<GameSaveGameItemViewModel> FilteredGames { get; }
    public IReadOnlyList<GameSaveAccountInfo> Accounts => accounts;
    public bool HasAccounts => accounts.Count > 0;
    public bool CanRefreshAccounts => !IsScanningInstalled && !IsMutating && accountRefresh == null;
    public bool IsScanningInstalled => isScanningInstalled;
    public bool IsScanningBackground => isScanningBackground;
    public bool IsScanning => IsScanningInstalled || IsScanningBackground;
    public bool IsMutating => isMutating;
    public bool CanMutate => !IsScanningInstalled && !IsMutating && accountRefresh == null;
    public bool CanBatch => CanMutate && HasSelectedGames;
    public bool HasSelectedGames => selectedCount > 0;
    public bool CanSelectAll => CanMutate && GamesWithSavesCount > selectedCount;
    public string SelectedGamesDisplay => string.Format(Strings.GameList_Saves_SelectedGames, selectedCount);
    public bool ShowEmptyState => !IsScanning && filteredGames.Count == 0;
    public string EmptyTitle => games.Count == 0 ? Strings.GameList_Saves_EmptyGames : Strings.GameList_Saves_EmptyFilter;
    public string EmptyHint => games.Count == 0 ? Strings.GameList_Saves_EmptyGamesHint : Strings.GameList_Saves_EmptyFilterHint;

    public long TotalSavesBytes { get; private set; }
    public long TotalScreenshotsBytes { get; private set; }
    public long TotalBackupsBytes { get; private set; }
    public int GamesWithSavesCount { get; private set; }
    public int GamesWithScreenshotsCount { get; private set; }
    public int TotalDiscoveredGamesCount { get; private set; }
    public int GamesWithDataCount => IsScanningInstalled ? installedGamesWithDataCount : gamesWithDataCount;
    public long TotalSaveFilesCount { get; private set; }
    public long TotalScreenshotsCount { get; private set; }
    public long TotalBackupsCount { get; private set; }
    public string TotalSavesSizeDisplay => FormatSize(TotalSavesBytes);
    public string TotalScreenshotsSizeDisplay => FormatSize(TotalScreenshotsBytes);
    public string TotalBackupsSizeDisplay => FormatSize(TotalBackupsBytes);
    public string TotalTrackedSizeDisplay => FormatSize(TotalSavesBytes + TotalScreenshotsBytes + TotalBackupsBytes);
    public string SavesLegendDisplay => string.Format(Strings.GameList_Saves_SavesLegendCount, GamesWithSavesCount, TotalSaveFilesCount);
    public string ScreenshotsLegendDisplay => string.Format(Strings.GameList_Saves_ScreenshotsLegendCount, GamesWithScreenshotsCount, TotalScreenshotsCount);
    public string BackupsLegendDisplay => string.Format(Strings.GameList_Saves_BackupsLegendCount, TotalBackupsCount);
    public string StorageTitle => StoragePath is { Length: > 0 } path ? path : Strings.GameList_Saves_OverviewSteam;
    public string StorageGameCountDisplay => string.Format(Strings.GameList_Saves_OverviewGameCount, TotalDiscoveredGamesCount, GamesWithDataCount);
    public string? StoragePath => scannedAccount is { } account
        ? string.Format(Strings.GameList_Saves_OverviewAccountPath, account.DisplayName, steamRootPath ?? account.UserdataPath)
        : steamRootPath;
    public string? SteamScreenshotsPath => scannedAccount is { } current && !string.IsNullOrWhiteSpace(steamRootPath)
        ? Path.Combine(steamRootPath, GameSavePathResolver.ScreenshotStorageRelativePath(current.SteamId32)) : null;
    public bool HasSteamScreenshotsPath => SteamScreenshotsPath != null;

    public int ProgressValue => progressValue;
    public int ProgressMaximum => Math.Max(1, progressMaximum);
    public bool IsProgressIndeterminate => progressMaximum == 0;
    public string InstalledProgressDisplay => string.Format(Strings.GameList_Saves_ScanInstalledProgress, progressValue, progressMaximum);
    public string BackgroundProgressDisplay => string.Format(Strings.GameList_Saves_ScanBackgroundProgress, progressValue, progressMaximum);

    public string BackupRootPath => settings.BackupRootPath;
    public bool AutoBackupBeforeRestore
    {
        get => settings.AutoBackupBeforeRestore;
        set
        {
            if (value != settings.AutoBackupBeforeRestore && CanMutate)
                _ = SetAutoBackupAsync(value);
        }
    }

    public IReadOnlyList<string> Filters { get; } =
    [
        Strings.GameList_Saves_FilterWithSavesOrScreenshots,
        Strings.GameList_Saves_FilterInstalled,
        Strings.GameList_Saves_FilterWithSaves,
        Strings.GameList_Saves_FilterWithScreenshots,
        Strings.GameList_Saves_FilterWithBackups,
        Strings.GameList_Saves_FilterAll,
    ];

    public IReadOnlyList<string> SortOptions { get; } =
    [
        Strings.GameList_Saves_SortOccupiedSize,
        Strings.GameList_Saves_SortSaveSize,
        Strings.GameList_Saves_SortScreenshotSize,
        Strings.GameList_Saves_SortRecent,
        Strings.GameList_Saves_SortNameAscending,
    ];

    public GameSaveAccountInfo? SelectedAccount
    {
        get => selectedAccount;
        set
        {
            if (IsMutating || value == null || value == selectedAccount)
                return;

            SetAccount(value);
            _ = RescanAsync();
        }
    }

    public string SearchText
    {
        get => searchText;
        set
        {
            if (searchText == (value ?? ""))
                return;

            this.RaiseAndSetIfChanged(ref searchText, value ?? "");
            ApplyFilter();
        }
    }

    public int FilterIndex
    {
        get => filterIndex;
        set
        {
            if (filterIndex == value || value < 0)
                return;

            this.RaiseAndSetIfChanged(ref filterIndex, value);
            ApplyFilter();
        }
    }

    public int SortIndex
    {
        get => sortIndex;
        set
        {
            if (sortIndex == value || value < 0)
                return;

            this.RaiseAndSetIfChanged(ref sortIndex, value);
            ApplyFilter();
        }
    }

    public Task EnsureScannedAsync()
    {
        if (hasScanned || scan != null)
            return Task.CompletedTask;
        if (IsMutating)
        {
            rescanAfterMutation = true;
            return Task.CompletedTask;
        }
        return RescanAsync(refreshAccounts: true);
    }

    public Task RescanAsync(bool refreshAccounts = false)
    {
        if (IsMutating)
            return Task.CompletedTask;

        accountRefresh?.Cancel();
        CancelScan(restoreAccount: false);
        refreshedBackups = null;
        var session = new ScanSession();
        scan = session;
        installedGamesWithDataCount = 0;
        SetDiscoveryCount(0);
        SetProgress(0, 0);
        SetScanPhase(installed: true, background: false);
        return ScanAsync(session, refreshAccounts);
    }

    public async Task RefreshAccountsAsync()
    {
        if (!CanRefreshAccounts)
            return;

        using var refresh = new CancellationTokenSource();
        SetAccountRefresh(refresh);
        var rescan = false;
        try
        {
            var discovered = await scanner.GetAccountsAsync(refresh.Token, refresh: true);
            refresh.Token.ThrowIfCancellationRequested();
            var requested = selectedAccount;
            var account = SetAccounts(discovered, requested?.SteamId64);
            rescan = account?.SteamId64 != requested?.SteamId64 ||
                !GameSaveFileSystem.PathComparer.Equals(account?.UserdataPath, requested?.UserdataPath);
            if (!rescan && scannedAccount?.SteamId64 == account?.SteamId64)
                SetStorageAccount(account, steamRootPath);
        }
        catch (OperationCanceledException) when (refresh.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            ShowError(Strings.GameList_Saves_RefreshAccountsFailed, ex);
        }
        finally
        {
            SetAccountRefresh(null);
        }

        if (rescan)
            await RescanAsync();
    }

    void SetAccountRefresh(CancellationTokenSource? refresh)
    {
        accountRefresh = refresh;
        this.RaisePropertyChanged(nameof(CanRefreshAccounts));
        this.RaisePropertyChanged(nameof(CanMutate));
        NotifySelection();
    }

    public void CancelScan() => CancelScan(restoreAccount: true);

    void CancelScan(bool restoreAccount)
    {
        var session = scan;
        if (session == null)
            return;

        scan = null;
        session.Installed.Cancel();
        session.Background.Cancel();
        if (IsScanningInstalled)
            SetDiscoveryCount(games.Count);
        SetScanPhase(installed: false, background: false);
        if (restoreAccount)
            SetAccount(scannedAccount);
    }

    public void StopBackgroundScan() => scan?.Background.Cancel();

    public void Detach()
    {
        rescanAfterMutation = false;
        accountRefresh?.Cancel();
        if (scan != null)
            hasScanned = false;

        CancelScan();
    }

    async Task ScanAsync(ScanSession session, bool refreshAccounts)
    {
        var installedCt = session.Installed.Token;
        var backgroundCt = session.Background.Token;
        var requestedAccount = selectedAccount;
        try
        {
            var loadedSettings = settingsLoaded ? settings : await Task.Run(() => settingsStore.LoadAsync(installedCt), installedCt);
            var discoveredAccounts = await scanner.GetAccountsAsync(installedCt, refreshAccounts);
            var steamRoot = Ioc.Get_Nullable<ISteamService>()?.SteamDirPath;
            installedCt.ThrowIfCancellationRequested();
            if (!settingsLoaded)
            {
                SetSettings(loadedSettings);
                settingsLoaded = true;
            }

            var account = SetAccounts(discoveredAccounts, requestedAccount?.SteamId64);

            await scanner.ScanPhasedAsync(account,
                entries => PublishScanUpdate(session, installedCt, () => PublishInstalled(session, entries, account, steamRoot)),
                entries => PublishScanUpdate(session, backgroundCt, () => PublishBackground(session, entries)),
                new ScanProgress(progress => PublishProgress(session, progress)), installedCt, backgroundCt);
        }
        catch (OperationCanceledException) when (installedCt.IsCancellationRequested || backgroundCt.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            if (scan == session)
            {
                SetAccount(scannedAccount);
                if (IsScanningInstalled)
                    SetDiscoveryCount(games.Count);
                ShowError(Strings.GameList_Saves_ScanFailed, ex);
            }
        }
        finally
        {
            if (scan == session)
            {
                scan = null;
                if (refreshedBackups is { } history)
                {
                    AddBackupOnlyRows(history);
                    NotifyMetrics();
                }
                SetScanPhase(installed: false, background: false);
            }
            session.Installed.Dispose();
            session.Background.Dispose();
        }
    }

    void PublishScanUpdate(ScanSession session, CancellationToken token, Action update)
    {
        // The scanner calls this on a worker thread. Apply each batch before accepting the next.
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (scan == session && !token.IsCancellationRequested)
                update();
        }, DispatcherPriority.Background).GetTask().GetAwaiter().GetResult();
    }

    void PublishProgress(ScanSession session, GameSavePhasedScanProgress progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (scan != session)
                return;

            if (progress.Phase == GameSaveScanPhase.Installed && IsScanningInstalled)
            {
                installedGamesWithDataCount = progress.InstalledGamesWithDataCount;
                this.RaisePropertyChanged(nameof(GamesWithDataCount));
            }
            else if (progress.Phase != GameSaveScanPhase.Uninstalled || !IsScanningBackground || session.Background.IsCancellationRequested)
                return;

            SetDiscoveryCount(progress.TotalDiscoveredGamesCount);
            SetProgress(progress.ProcessedInPhase, progress.TotalInPhase);
        }, DispatcherPriority.Background);
    }

    void PublishInstalled(ScanSession session, IReadOnlyList<GameSaveEntry> entries, GameSaveAccountInfo? account, string? steamRoot)
    {
        if (scannedAccount?.SteamId64 == account?.SteamId64)
            session.PreviousRows = new(games);

        updateInProgress = true;
        try
        {
            games.Clear();
            foreach (var entry in entries)
                games.Add(entry.AppId, ReuseOrCreateRow(session, entry));
        }
        finally
        {
            updateInProgress = false;
        }

        SetStorageAccount(account, steamRoot);
        hasScanned = true;
        SetDiscoveryCount(games.Count);
        SetProgress(0, 0);
        UpdateMetrics();
        UpdateSelection();
        ApplyFilter();
        SetScanPhase(installed: false, background: true);
    }

    void PublishBackground(ScanSession session, IReadOnlyList<GameSaveEntry> entries)
    {
        updateInProgress = true;
        try
        {
            foreach (var entry in entries)
            {
                if (session.RestoredApps.Contains(entry.AppId))
                    continue;

                if (games.TryGetValue(entry.AppId, out var row))
                {
                    AccumulateMetrics(row, -1);
                    filteredGames.Remove(row);
                    row.UpdateEntry(CurrentEntry(entry));
                }
                else
                {
                    row = ReuseOrCreateRow(session, entry);
                    games.Add(entry.AppId, row);
                }

                AccumulateMetrics(row, 1);
                if (MatchesFilter(row))
                    InsertSorted(row);
            }
        }
        finally
        {
            updateInProgress = false;
        }
        SetDiscoveryCount(Math.Max(TotalDiscoveredGamesCount, games.Count));
        NotifyMetrics();
        UpdateSelection();
        NotifyEmptyState();
    }

    GameSaveGameItemViewModel ReuseOrCreateRow(ScanSession session, GameSaveEntry entry)
    {
        var current = CurrentEntry(entry);
        if (!session.PreviousRows.Remove(entry.AppId, out var row))
            return CreateRow(current);

        row.UpdateEntry(current);
        return row;
    }

    GameSaveEntry CurrentEntry(GameSaveEntry entry) => refreshedBackups is { } history
        ? entry with { Backups = history[entry.AppId].ToArray() } : entry;

    GameSaveGameItemViewModel CreateRow(GameSaveEntry entry) => new(this, entry, UpdateSelection);

    GameSaveAccountInfo? SetAccounts(IReadOnlyList<GameSaveAccountInfo> discovered, long? requestedId)
    {
        var account = discovered.FirstOrDefault(candidate => candidate.SteamId64 == requestedId)
            ?? discovered.FirstOrDefault();
        accounts = discovered;
        this.RaisePropertyChanged(nameof(Accounts));
        this.RaisePropertyChanged(nameof(HasAccounts));
        SetAccount(account, notify: true);
        return account;
    }

    void SetStorageAccount(GameSaveAccountInfo? account, string? root)
    {
        scannedAccount = account;
        steamRootPath = root;
        this.RaisePropertyChanged(nameof(StorageTitle));
        this.RaisePropertyChanged(nameof(StoragePath));
        this.RaisePropertyChanged(nameof(HasSteamScreenshotsPath));
    }

    void SetAccount(GameSaveAccountInfo? account, bool notify = false)
    {
        if (selectedAccount == account && !notify)
            return;

        selectedAccount = account;
        this.RaisePropertyChanged(nameof(SelectedAccount));
    }

    void ApplyFilter()
    {
        var sorted = games.Values.Where(MatchesFilter).Order(Comparer<GameSaveGameItemViewModel>.Create(CompareGames));
        filteredGames.Load(sorted);
        NotifyEmptyState();
    }

    void NotifyEmptyState()
    {
        this.RaisePropertyChanged(nameof(ShowEmptyState));
        this.RaisePropertyChanged(nameof(EmptyTitle));
        this.RaisePropertyChanged(nameof(EmptyHint));
    }

    bool MatchesFilter(GameSaveGameItemViewModel game)
    {
        var query = searchText.Trim();
        return (filterIndex switch
        {
            1 => game.Entry.IsInstalled,
            2 => game.HasLocalSaves,
            3 => game.HasScreenshots,
            4 => game.HasBackups,
            5 => true,
            _ => game.HasLocalSaves || game.HasScreenshots,
        }) && (query.Length == 0 || game.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
            game.Entry.AppId.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.Ordinal));
    }

    int CompareGames(GameSaveGameItemViewModel left, GameSaveGameItemViewModel right)
    {
        var bySort = sortIndex switch
        {
            1 => right.Entry.TotalBytes.CompareTo(left.Entry.TotalBytes),
            2 => right.Entry.ScreenshotBytes.CompareTo(left.Entry.ScreenshotBytes),
            3 => Nullable.Compare(right.LastModifiedUtc, left.LastModifiedUtc),
            4 => 0,
            _ => right.Entry.TotalOccupiedBytes.CompareTo(left.Entry.TotalOccupiedBytes),
        };
        if (bySort != 0)
            return bySort;

        var byName = StringComparer.CurrentCultureIgnoreCase.Compare(left.Name, right.Name);
        return byName != 0 ? byName : left.Entry.AppId.CompareTo(right.Entry.AppId);
    }

    void InsertSorted(GameSaveGameItemViewModel row)
    {
        var start = 0;
        var end = filteredGames.Count;
        while (start < end)
        {
            var middle = (start + end) / 2;
            if (CompareGames(row, filteredGames[middle]) > 0)
                start = middle + 1;
            else
                end = middle;
        }
        filteredGames.Insert(start, row);
    }

    public void SetAllSelected(bool selected)
    {
        if (!CanMutate)
            return;

        updateInProgress = true;
        try
        {
            foreach (var game in games.Values)
                game.IsSelected = selected;
            if (scan is { } activeScan)
                foreach (var pending in activeScan.PreviousRows.Values)
                    pending.IsSelected = selected;
        }
        finally
        {
            updateInProgress = false;
        }
        UpdateSelection();
    }

    public void SetAllExpanded(bool expanded)
    {
        foreach (var game in filteredGames)
            game.IsExpanded = expanded;
    }

    void UpdateSelection()
    {
        if (updateInProgress)
            return;

        selectedCount = games.Values.Count(game => game.IsSelected);
        NotifySelection();
    }

    void NotifySelection()
    {
        this.RaisePropertyChanged(nameof(HasSelectedGames));
        this.RaisePropertyChanged(nameof(SelectedGamesDisplay));
        this.RaisePropertyChanged(nameof(CanBatch));
        this.RaisePropertyChanged(nameof(CanSelectAll));
    }

    void UpdateMetrics()
    {
        TotalSavesBytes = TotalScreenshotsBytes = TotalBackupsBytes = 0;
        TotalSaveFilesCount = TotalScreenshotsCount = TotalBackupsCount = 0;
        GamesWithSavesCount = GamesWithScreenshotsCount = gamesWithDataCount = 0;
        foreach (var game in games.Values)
            AccumulateMetrics(game, 1);
        NotifyMetrics();
    }

    void AccumulateMetrics(GameSaveGameItemViewModel game, int direction)
    {
        TotalSavesBytes += direction * game.Entry.TotalBytes;
        TotalScreenshotsBytes += direction * game.Entry.ScreenshotBytes;
        TotalBackupsBytes += direction * game.BackupBytes;
        TotalSaveFilesCount += direction * (long)game.Entry.TotalFileCount;
        TotalScreenshotsCount += direction * (long)game.Entry.ScreenshotCount;
        TotalBackupsCount += direction * (long)game.BackupCount;
        GamesWithSavesCount += game.HasLocalSaves ? direction : 0;
        GamesWithScreenshotsCount += game.HasScreenshots ? direction : 0;
        gamesWithDataCount += game.HasLocalSaves || game.HasScreenshots ? direction : 0;
    }

    void NotifyMetrics()
    {
        this.RaisePropertyChanged(nameof(TotalSavesBytes));
        this.RaisePropertyChanged(nameof(TotalScreenshotsBytes));
        this.RaisePropertyChanged(nameof(TotalBackupsBytes));
        this.RaisePropertyChanged(nameof(TotalSavesSizeDisplay));
        this.RaisePropertyChanged(nameof(TotalScreenshotsSizeDisplay));
        this.RaisePropertyChanged(nameof(TotalBackupsSizeDisplay));
        this.RaisePropertyChanged(nameof(TotalTrackedSizeDisplay));
        this.RaisePropertyChanged(nameof(SavesLegendDisplay));
        this.RaisePropertyChanged(nameof(ScreenshotsLegendDisplay));
        this.RaisePropertyChanged(nameof(BackupsLegendDisplay));
        this.RaisePropertyChanged(nameof(CanSelectAll));
        NotifyGamesWithDataCount();
    }

    void NotifyGamesWithDataCount()
    {
        this.RaisePropertyChanged(nameof(GamesWithDataCount));
        this.RaisePropertyChanged(nameof(StorageGameCountDisplay));
    }

    void SetDiscoveryCount(int count)
    {
        if (TotalDiscoveredGamesCount == count)
            return;

        TotalDiscoveredGamesCount = count;
        this.RaisePropertyChanged(nameof(TotalDiscoveredGamesCount));
        this.RaisePropertyChanged(nameof(StorageGameCountDisplay));
    }

    void SetScanPhase(bool installed, bool background)
    {
        isScanningInstalled = installed;
        isScanningBackground = background;
        this.RaisePropertyChanged(nameof(IsScanningInstalled));
        this.RaisePropertyChanged(nameof(IsScanningBackground));
        this.RaisePropertyChanged(nameof(IsScanning));
        this.RaisePropertyChanged(nameof(CanMutate));
        this.RaisePropertyChanged(nameof(CanRefreshAccounts));
        NotifyGamesWithDataCount();
        NotifySelection();
        NotifyEmptyState();
    }

    void SetProgress(int value, int maximum)
    {
        progressValue = value;
        progressMaximum = maximum;
        this.RaisePropertyChanged(nameof(ProgressValue));
        this.RaisePropertyChanged(nameof(ProgressMaximum));
        this.RaisePropertyChanged(nameof(IsProgressIndeterminate));
        this.RaisePropertyChanged(nameof(InstalledProgressDisplay));
        this.RaisePropertyChanged(nameof(BackgroundProgressDisplay));
    }

    void SetSettings(GameSaveSettings preferences)
    {
        settings = preferences;
        this.RaisePropertyChanged(nameof(BackupRootPath));
        this.RaisePropertyChanged(nameof(AutoBackupBeforeRestore));
    }

    sealed class ScanSession
    {
        internal CancellationTokenSource Installed { get; } = new();
        internal CancellationTokenSource Background { get; } = new();
        internal Dictionary<uint, GameSaveGameItemViewModel> PreviousRows { get; set; } = [];
        internal HashSet<uint> RestoredApps { get; } = [];
    }

    sealed class ScanProgress(Action<GameSavePhasedScanProgress> publish) : IProgress<GameSavePhasedScanProgress>
    {
        long lastUpdate;

        public void Report(GameSavePhasedScanProgress progress)
        {
            var now = Environment.TickCount64;
            if (progress.ProcessedInPhase != 0 && progress.ProcessedInPhase != progress.TotalInPhase && now - lastUpdate < 75)
                return;

            lastUpdate = now;
            publish(progress);
        }
    }
}
