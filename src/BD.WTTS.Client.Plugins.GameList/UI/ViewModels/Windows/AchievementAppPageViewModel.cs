using KeyValue = BD.SteamClient.Models.KeyValue;

namespace BD.WTTS.UI.ViewModels;

public sealed class AchievementAppPageViewModel : WindowViewModel
{
    readonly int appId;
    readonly ISteamService steamService;
    readonly ISteamworksLocalApiService steam;
    readonly StatsReply statsReply;
    readonly StatsReply storeReply;
    readonly CancellationTokenSource lifetime = new();
    Task? currentSteamOperation;
    bool sessionInitialized;
    bool statsReady;
    bool isSaving;
    List<AchievementInfo> allAchievements = [];
    List<StatInfo> allStatistics = [];
    AchievementSetOption? selectedAchievementSet;
    string? searchText;
    bool isCheckAll;

    public static string DisplayName => Constants.HARDCODED_APP_NAME + " | " + Strings.Achievement_Manage;

    public IReadOnlyList<AchievementInfo> Achievements { get; private set; } = [];
    public IReadOnlyList<StatInfo> Statistics { get; private set; } = [];
    public IReadOnlyList<AchievementSetOption> AchievementSets { get; private set; } = [];
    public int UnlockedAchievementCount { get; private set; }
    public int TotalAchievementCount => allAchievements.Count;
    public double AchievementCompletion => TotalAchievementCount == 0 ? 0 : (double)UnlockedAchievementCount / TotalAchievementCount;
    public bool HasAchievementSnapshot => statsReady;
    public bool ShowAchievementSetSelector { get; private set; }
    public bool ShowAchievementSetLabels => ShowAchievementSetSelector && SelectedAchievementSet?.Id is null;
    public bool IsLoading { get; private set; }
    public bool HasVisibleAchievements => Achievements.Count != 0;
    public bool CanRefreshStats => !IsLoading && !isSaving;
    public bool CanUpdateStats => statsReady && !IsLoading && !isSaving;
    public bool CanEditVisibleAchievements => CanUpdateStats && Achievements.Any(static item => item.CanEdit);

    public AchievementSetOption? SelectedAchievementSet
    {
        get => selectedAchievementSet;
        set
        {
            if (IsLoading || value is null || ReferenceEquals(selectedAchievementSet, value)) return;
            selectedAchievementSet = value;
            this.RaisePropertyChanged();
            this.RaisePropertyChanged(nameof(ShowAchievementSetLabels));
            UpdateAchievementFilter();
        }
    }

    public string? SearchText
    {
        get => searchText;
        set
        {
            if (searchText == value) return;
            searchText = value;
            this.RaisePropertyChanged();
            UpdateAchievementFilter();
            UpdateStatisticsFilter();
        }
    }

    public bool IsCheckAll
    {
        get => isCheckAll;
        set
        {
            if (isCheckAll == value || !CanUpdateStats) return;
            isCheckAll = value;
            foreach (var achievement in Achievements)
                if (achievement.CanEdit)
                    achievement.IsChecked = value;
            this.RaisePropertyChanged();
        }
    }

    public AchievementAppPageViewModel(int appid) : this(appid, ISteamService.Instance, ISteamworksLocalApiService.Instance)
    {
    }

    internal AchievementAppPageViewModel(int appid, ISteamService steamService, ISteamworksLocalApiService steam)
    {
        appId = appid;
        this.steamService = steamService;
        this.steam = steam;
        statsReply = new((ulong)appid);
        storeReply = new((ulong)appid);
        Title = DisplayName;
    }

    public async void RefreshStats_Click()
    {
        if (Disposed || IsLoading || isSaving) return;
        var token = lifetime.Token;
        IsLoading = true;
        statsReady = false;
        this.RaisePropertyChanged(nameof(IsLoading));
        RaiseEditAvailability();

        try
        {
            var load = Task.Run(() => LoadSnapshotAsync(token), token);
            currentSteamOperation = load;
            var snapshot = await load;
            if (token.IsCancellationRequested) return;
            Title = Constants.HARDCODED_APP_NAME + " | " + snapshot.GameName;
            statsReady = true;
            ApplySnapshot(snapshot.Achievements, snapshot.Statistics);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            if (token.IsCancellationRequested) return;
            Log.Error(nameof(AchievementAppPageViewModel), ex, "Could not load Steam achievements and stats.");
            ApplySnapshot([], []);
            Toast.Show(ToastIcon.Error, ex.Message);
        }
        finally
        {
            currentSteamOperation = null;
            IsLoading = false;
            if (!token.IsCancellationRequested)
            {
                this.RaisePropertyChanged(nameof(IsLoading));
                RaiseEditAvailability();
            }
        }
    }

    async Task<Snapshot> LoadSnapshotAsync(CancellationToken token)
    {
        if (!sessionInitialized)
        {
            if (!steamService.IsRunningSteamProcess || !steam.Initialize(appId))
                throw new InvalidOperationException(Strings.Achievement_Warning_1);
            sessionInitialized = true;
            steam.AddUserStatsReceivedCallback(statsReply.Receive);
            steam.AddUserStatsStoredCallback(storeReply.Receive);
        }

        steam.RunCallbacks(false);
        statsReply.Result = null;
        if (!steam.RequestCurrentStats())
            throw new InvalidOperationException(Strings.Achievement_RequestStatsFailed);
        var result = await WaitForReplyAsync(statsReply, Strings.Achievement_RequestStatsFailed, token).ConfigureAwait(false);
        if (result != 1)
            throw new InvalidOperationException(Strings.Achievement_Warning_2.Format(result));

        var appKey = appId.ToString(CultureInfo.InvariantCulture);
        var directory = steamService.SteamDirPath;
        var schema = string.IsNullOrWhiteSpace(directory) ? null : KeyValue.LoadAsBinary(
            Path.Combine(directory, "appcache", "stats", $"UserGameStatsSchema_{appKey}.bin"));
        var game = schema?[appKey];
        if (game is not { Valid: true } || game["stats"].Children is not { } statDefinitions)
            throw new InvalidDataException(Strings.Achievement_LoadSchemaFailed);

        var language = ResourceService.GetCurrentCultureSteamLanguageName();
        var steamLanguage = steam.GetCurrentGameLanguage();
        var entries = AchievementSchema.ReadAchievements(game, appId, language, steamLanguage);
        var access = new AchievementAccess(steam);
        entries.RemoveAll(achievement =>
        {
            token.ThrowIfCancellationRequested();
            if (!access.OwnsRequiredDlc(achievement) ||
                !steam.GetAchievementAndUnlockTime(achievement.Id!, out var unlocked, out var unlockTime) ||
                (achievement.IsArchived && !unlocked))
                return true;

            achievement.IsAchieved = unlocked;
            achievement.IsChecked = unlocked;
            if (unlocked && unlockTime is > 0 and <= uint.MaxValue)
            {
                achievement.UnlockTimeUnix = unlockTime;
                achievement.UnlockTime = DateTimeOffset.FromUnixTimeSeconds(unlockTime).LocalDateTime;
            }
            return false;
        });

        var statistics = new List<StatInfo>();
        foreach (var stat in statDefinitions)
        {
            token.ThrowIfCancellationRequested();
            if (stat["name"].AsString("") is not { Length: > 0 } id) continue;
            var type = AchievementSchema.GetStatType(stat);
            StatInfo? entry = type switch
            {
                UserStatType.Integer when steam.GetStatValue(id, out int value) => new IntStatInfo
                {
                    IntValue = value,
                    OriginalValue = value,
                },
                UserStatType.Float or UserStatType.AverageRate when steam.GetStatValue(id, out float value) => new FloatStatInfo
                {
                    FloatValue = value,
                    OriginalValue = value,
                },
                _ => null,
            };
            if (entry is null) continue;
            entry.Id = id;
            entry.DisplayName = AchievementSchema.GetLocalizedString(stat["display"]["name"], language, steamLanguage, id);
            entry.IsIncrementOnly = stat["incrementonly"].AsBoolean(false);
            entry.Permission = stat["permission"].AsInteger(0);
            statistics.Add(entry);
        }
        statistics.Sort(static (a, b) => string.CompareOrdinal(b.Id, a.Id));

        var name = steam.GetAppData((uint)appId, "name");
        return new(string.IsNullOrWhiteSpace(name) ? appKey : name, entries, statistics);
    }

    async Task<int> WaitForReplyAsync(StatsReply reply, string timeoutMessage, CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
        var start = Stopwatch.GetTimestamp();
        while (true)
        {
            token.ThrowIfCancellationRequested();
            steam.RunCallbacks(false);
            if (reply.Result is { } result) return result;
            if (Stopwatch.GetElapsedTime(start) >= TimeSpan.FromSeconds(15))
                throw new TimeoutException(timeoutMessage);
            await timer.WaitForNextTickAsync(token).ConfigureAwait(false);
        }
    }

    async Task<bool> StoreStatsAsync(CancellationToken token, bool resetStats = false)
    {
        var store = Task.Run(async () =>
        {
            steam.RunCallbacks(false);
            storeReply.Result = null;
            if (!(resetStats ? steam.ResetAllStats(false) : steam.StoreStats())) return false;
            var result = await WaitForReplyAsync(storeReply, Strings.Achievement_StoreFailed, token).ConfigureAwait(false);
            if (result == 1) return true;
            Log.Warn(nameof(AchievementAppPageViewModel), $"Steam rejected the achievement or stats write (EResult {result}).");
            return false;
        }, token);
        currentSteamOperation = store;
        try
        {
            return await store;
        }
        finally
        {
            currentSteamOperation = null;
        }
    }

    void ApplySnapshot(List<AchievementInfo> achievements, List<StatInfo> statistics)
    {
        allAchievements = achievements;
        allStatistics = statistics;

        var sets = new List<AchievementSetOption>();
        var unlocked = 0;
        for (var index = 0; index < achievements.Count;)
        {
            var set = achievements[index].Set;
            var first = index;
            var setUnlocked = 0;
            do
            {
                if (achievements[index].IsAchieved) setUnlocked++;
                index++;
            }
            while (index < achievements.Count && achievements[index].Set?.Id == set?.Id);
            sets.Add(new(set?.Id ?? 0, set?.Name ?? Strings.Achievement_SetBaseGame, setUnlocked, index - first, set is { DlcAppId: > 0 }));
            unlocked += setUnlocked;
        }

        var previousSet = selectedAchievementSet?.Id;
        ShowAchievementSetSelector = sets.Any(static set => set.Id is > 0);
        UnlockedAchievementCount = unlocked;
        sets.Insert(0, new(null, Strings.Achievement_SetAll, unlocked, achievements.Count, false));
        AchievementSets = sets;
        this.RaisePropertyChanged(nameof(UnlockedAchievementCount));
        this.RaisePropertyChanged(nameof(TotalAchievementCount));
        this.RaisePropertyChanged(nameof(AchievementCompletion));
        this.RaisePropertyChanged(nameof(AchievementSets));
        // Replacing ItemsSource can reset the ComboBox selection. Restore it by ID after publishing the new options.
        selectedAchievementSet = sets.Find(set => set.Id == previousSet) ?? sets[0];
        this.RaisePropertyChanged(nameof(SelectedAchievementSet));
        this.RaisePropertyChanged(nameof(ShowAchievementSetSelector));
        this.RaisePropertyChanged(nameof(ShowAchievementSetLabels));
        UpdateAchievementFilter();
        UpdateStatisticsFilter();
    }

    void UpdateAchievementFilter()
    {
        var setId = selectedAchievementSet?.Id;
        var text = searchText?.Trim();
        Achievements = setId is null && string.IsNullOrEmpty(text) ? allAchievements : allAchievements.FindAll(item =>
            (setId is null || (item.Set?.Id ?? 0) == setId) &&
            (string.IsNullOrEmpty(text) || Matches(item.Name, text) || Matches(item.Description, text) || Matches(item.Set?.Name, text)));
        if (isCheckAll)
        {
            isCheckAll = false;
            this.RaisePropertyChanged(nameof(IsCheckAll));
        }
        this.RaisePropertyChanged(nameof(Achievements));
        this.RaisePropertyChanged(nameof(HasVisibleAchievements));
        this.RaisePropertyChanged(nameof(CanEditVisibleAchievements));
    }

    void UpdateStatisticsFilter()
    {
        var text = searchText?.Trim();
        Statistics = string.IsNullOrEmpty(text) ? allStatistics : allStatistics.FindAll(item =>
            Matches(item.DisplayName, text) || Matches(item.Id, text));
        this.RaisePropertyChanged(nameof(Statistics));
    }

    static bool Matches(string? value, string text) => value?.Contains(text, StringComparison.OrdinalIgnoreCase) == true;

    bool StageAchievements(List<(AchievementInfo Info, bool IsChecked)> changes)
    {
        // The display snapshot is not a license check: query Steam again before staging anything.
        var result = new AchievementAccess(steam).StageChanges(changes, out var rejected);
        if (result == AchievementWriteResult.Staged) return true;
        Toast.Show(ToastIcon.Error, result == AchievementWriteResult.Restricted ? Strings.Achievement_DlcOwnershipChanged :
            Strings.Achievement_Warning_3.Format(rejected?.Name ?? rejected?.Id));
        return false;
    }

    int StageStatistics()
    {
        var count = 0;
        foreach (var stat in Statistics)
        {
            if (!stat.IsModified) continue;
            var stored = stat switch
            {
                IntStatInfo { Id: { Length: > 0 } id } value => steam.SetStatValue(id, value.IntValue),
                FloatStatInfo { Id: { Length: > 0 } id } value => steam.SetStatValue(id, value.FloatValue),
                _ => false,
            };
            if (stored)
            {
                count++;
                continue;
            }
            Toast.Show(ToastIcon.Error, Strings.Achievement_Warning_4.Format(stat.Id));
            return -1;
        }
        return count;
    }

    public async void SaveChange_Click()
    {
        if (!CanUpdateStats || Disposed) return;
        var token = lifetime.Token;
        SetSaving(true);
        var refresh = false;
        try
        {
            var changes = Achievements.Where(static item => item.CanEdit && item.IsAchieved != item.IsChecked)
                .Select(static item => (Info: item, item.IsChecked)).ToList();
            var lockCount = changes.Count(static change => !change.IsChecked);
            if (lockCount > 0 && !(await MessageBox.ShowAsync(Strings.Achievement_LockWarning.Format(lockCount),
                DisplayName, MessageBox.Button.OKCancel, MessageBox.Image.Warning)).IsOK())
                return;
            if (Disposed || (changes.Count == 0 && !Statistics.Any(static stat => stat.IsModified))) return;

            refresh = true;
            if (!StageAchievements(changes)) return;
            var stats = StageStatistics();
            if (stats < 0) return;
            var saved = await StoreStatsAsync(token);
            if (!token.IsCancellationRequested)
            {
                if (saved)
                    Toast.Show(ToastIcon.Success, Strings.Achievement_EditSucces.Format(changes.Count, stats));
                else
                    Toast.Show(ToastIcon.Error, Strings.Achievement_StoreFailed);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            Log.Error(nameof(AchievementAppPageViewModel), ex, "Could not save Steam achievement or stat changes.");
            if (!Disposed) Toast.Show(ToastIcon.Error, Strings.Achievement_StoreFailed);
        }
        finally
        {
            SetSaving(false);
            if (refresh) RefreshStats_Click();
        }
    }

    public async void ResetAllStats_Click()
    {
        if (!CanUpdateStats || Disposed) return;
        var token = lifetime.Token;
        SetSaving(true);
        var refresh = false;
        try
        {
            if (!(await MessageBox.ShowAsync(Strings.Achievement_ResetWaring_1, DisplayName, MessageBox.Button.OKCancel)).IsOK())
                return;
            var resetAchievements = (await MessageBox.ShowAsync(Strings.Achievement_ResetWaring_2, DisplayName, MessageBox.Button.OKCancel)).IsOK();
            if (!(await MessageBox.ShowAsync(Strings.Achievement_ResetWaring_3, DisplayName, MessageBox.Button.OKCancel)).IsOK() || Disposed)
                return;

            refresh = true;
            var saved = await StoreStatsAsync(token, resetStats: true);
            if (token.IsCancellationRequested) return;
            if (!saved)
            {
                Toast.Show(ToastIcon.Error, Strings.Achievement_StoreFailed);
                return;
            }
            if (resetAchievements)
            {
                // Never pass true to the game-wide reset: it cannot honor per-achievement restrictions.
                var changes = allAchievements.Where(static item => item.CanEdit && item.IsAchieved)
                    .Select(static item => (Info: item, IsChecked: false)).ToList();
                if (changes.Count != 0 && StageAchievements(changes) && !await StoreStatsAsync(token) && !token.IsCancellationRequested)
                    Toast.Show(ToastIcon.Error, Strings.Achievement_StoreFailed);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            Log.Error(nameof(AchievementAppPageViewModel), ex, "Could not reset Steam achievements or stats.");
            if (!Disposed) Toast.Show(ToastIcon.Error, Strings.Achievement_StoreFailed);
        }
        finally
        {
            SetSaving(false);
            if (refresh) RefreshStats_Click();
        }
    }

    void SetSaving(bool value)
    {
        isSaving = value;
        if (!Disposed) RaiseEditAvailability();
    }

    void RaiseEditAvailability()
    {
        this.RaisePropertyChanged(nameof(CanRefreshStats));
        this.RaisePropertyChanged(nameof(HasAchievementSnapshot));
        this.RaisePropertyChanged(nameof(CanUpdateStats));
        this.RaisePropertyChanged(nameof(CanEditVisibleAchievements));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !Disposed)
        {
            lifetime.Cancel();
            if (currentSteamOperation is { IsCompleted: false } operation)
                _ = operation.ContinueWith(_ => ReleaseSession(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            else
                ReleaseSession();
        }
        base.Dispose(disposing);
    }

    void ReleaseSession()
    {
        try
        {
            if (sessionInitialized) steam.DisposeSteamClient();
        }
        catch (Exception ex)
        {
            Log.Warn(nameof(AchievementAppPageViewModel), ex, "Could not release the Steam achievement session.");
        }
        finally
        {
            lifetime.Dispose();
        }
    }

    sealed class StatsReply(ulong gameId)
    {
        public int? Result { get; set; }

        public void Receive(ISteamworksLocalApiService.IUserStatsReceived reply) => Receive(reply.GameId, reply.Result);

        public void Receive(ulong responseGameId, int result)
        {
            if (responseGameId == gameId) Result = result;
        }
    }

    sealed record Snapshot(string GameName, List<AchievementInfo> Achievements, List<StatInfo> Statistics);
}

public sealed record AchievementSetOption(int? Id, string Name, int UnlockedCount, int TotalCount, bool IsDlc)
{
    public string CountText { get; } = $"{UnlockedCount:N0} / {TotalCount:N0}";

    public override string ToString() => $"{Name} · {CountText}";
}
