#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using BD.SteamClient.Helpers;
using AppResources = BD.WTTS.Client.Resources.Strings;
#endif

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public sealed partial class SteamConnectService
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    : ReactiveObject, IDisposable
#endif
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    volatile bool disposedValue;

    public const int SteamAFKMaxCount = 32;

    static readonly Lazy<SteamConnectService> mCurrent = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static SteamConnectService Current => mCurrent.Value;

    internal static void NotifySteamProgramPathChanged()
    {
        if (mCurrent.IsValueCreated)
            mCurrent.Value.OnSteamProgramPathChanged();
    }

    readonly ISteamworksLocalApiService swLocalService;
    readonly ISteamService stmService;
    readonly ISteamworksWebApiService swWebService;
    readonly SemaphoreSlim steamClientGate = new(1, 1);
    readonly SemaphoreSlim gameListGate = new(1, 1);
    readonly object gameListSync = new();
    CancellationTokenSource? gameListCancellation;
    bool gameListRequested;
    int gameListProcessId;
    long gameListSteamId;
    int steamPathGeneration;

    SteamConnectService() : this(ISteamService.Instance, ISteamworksLocalApiService.Instance,
        ISteamworksWebApiService.Instance)
    {
    }

    internal SteamConnectService(ISteamService steamService, ISteamworksLocalApiService localService,
        ISteamworksWebApiService webService, Func<SteamApp, Process?>? startIdleProcess = null)
    {
        stmService = steamService;
        swLocalService = localService;
        swWebService = webService;
        this.startIdleProcess = startIdleProcess ?? (static app => app.StartSteamAppProcess());
        SteamApps = new(t => t.AppId);
        DownloadApps = new(t => t.AppId);
        SteamUsers = new(t => t.SteamId64);

        DownloadApps
            .Connect()
            .Subscribe(changes => SteamApps.Edit(updater =>
            {
                // Refresh only affected games, once per download changeset.
                foreach (var change in changes)
                {
                    if (change.Reason == ChangeReason.Remove)
                        continue;

                    var optional = updater.Lookup(change.Key);
                    if (optional.HasValue && CopyDownloadState(optional.Value, change.Current))
                        updater.Refresh(optional.Value);
                }
            }));

        this.ObserveValue(nameof(IsWatchSteamDownloading), static x => x.IsWatchSteamDownloading).Skip(1)
            .Subscribe(x =>
            {
                if (x)
                {
                    InitializeDownloadGameList();
#if LINUX
                    IPlatformService.Instance.GetSystemUserPassword();
#endif
                    stmService.StartWatchSteamDownloading(OnDownloadingAppChanged, OnDownloadingAppRemoved);
                }
                else
                {
                    stmService.StopWatchSteamDownloading();
                }
            });
    }

    void OnDownloadingAppChanged(SteamApp app)
    {
        var optional = DownloadApps.Lookup(app.AppId);
        if (!optional.HasValue)
            DownloadApps.AddOrUpdate(app);
        else if (CopyDownloadState(optional.Value, app))
            DownloadApps.Refresh(optional.Value);

        if (WatchDownloadingSteamAppIds.Contains(app.AppId))
        {
            if (app.IsDownloading)
                app.IsWatchDownloading = true;
            else
            {
                WatchDownloadingSteamAppIds.Remove(app.AppId);
                if (!WatchDownloadingSteamAppIds.Any()) WatchDownloadingComplete();
            }
        }
    }

    void OnDownloadingAppRemoved(uint appId) => DownloadApps.RemoveKey(appId);

    internal void OnSteamProgramPathChanged()
    {
        if (disposedValue) return;
        Interlocked.Increment(ref steamPathGeneration);
        bool refresh;
        lock (gameListSync)
        {
            refresh = gameListRequested;
            gameListProcessId = -1;
            gameListSteamId = -1;
            gameListCancellation?.Cancel();
            SteamUsers.Clear();
            SteamApps.Clear();
        }

        WatchDownloadingSteamAppIds.Clear();
        DownloadApps.Clear();
        CurrentSteamUser = null;
        IsConnectToSteam = false;

        if (IsWatchSteamDownloading)
        {
            try
            {
                stmService.StopWatchSteamDownloading();
                InitializeDownloadGameList();
                stmService.StartWatchSteamDownloading(OnDownloadingAppChanged, OnDownloadingAppRemoved);
            }
            catch (Exception ex)
            {
                IsWatchSteamDownloading = false;
                ex.LogAndShowT(nameof(SteamConnectService));
            }
        }

        if (refresh)
            _ = Task.Run(() => RefreshGamesListAsync(showToast: false));
    }

    static bool CopyDownloadState(SteamApp target, SteamApp source)
    {
        if (target.InstalledDir == source.InstalledDir && target.State == source.State &&
            target.SizeOnDisk == source.SizeOnDisk && target.LastOwner == source.LastOwner &&
            target.BytesToDownload == source.BytesToDownload && target.BytesDownloaded == source.BytesDownloaded &&
            target.BytesToStage == source.BytesToStage && target.BytesStaged == source.BytesStaged &&
            target.LastUpdated == source.LastUpdated)
            return false;

        target.InstalledDir = source.InstalledDir;
        target.State = source.State;
        target.SizeOnDisk = source.SizeOnDisk;
        target.LastOwner = source.LastOwner;
        target.BytesToDownload = source.BytesToDownload;
        target.BytesDownloaded = source.BytesDownloaded;
        target.BytesToStage = source.BytesToStage;
        target.BytesStaged = source.BytesStaged;
        target.LastUpdated = source.LastUpdated;
        return true;
    }

    #region Steam 游戏列表

    public SourceCache<SteamApp, uint> SteamApps { get; }

    public SourceCache<SteamApp, uint> DownloadApps { get; }

    #endregion

    #region 运行中的游戏列表

    //ConcurrentDictionary<uint, SteamApp> _RuningSteamApps = new();

    public ConcurrentDictionary<uint, SteamApp> RuningSteamApps { get; } = new();

    #endregion

    #region 当前 Steam 登录用户

    SteamUser? _CurrentSteamUser;

    public SteamUser? CurrentSteamUser
    {
        get => _CurrentSteamUser;
        set
        {
            if (_CurrentSteamUser != value)
            {
                _CurrentSteamUser = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public SourceCache<SteamUser, long> SteamUsers { get; }

    #endregion

    #region 连接 SteamClient 是否成功

    bool _IsConnectToSteam;

    public bool IsConnectToSteam
    {
        get => _IsConnectToSteam;
        set
        {
            if (_IsConnectToSteam != value)
            {
                _IsConnectToSteam = value;
                this.RaisePropertyChanged();
            }
        }
    }

    bool _IsRunningSteamProcess;

    public bool IsRunningSteamProcess
    {
        get => _IsRunningSteamProcess;
        set
        {
            if (_IsRunningSteamProcess != value)
            {
                _IsRunningSteamProcess = value;
                this.RaisePropertyChanged();
            }
        }
    }

    bool _IsSteamChinaLauncher;

    public bool IsSteamChinaLauncher
    {
        get => _IsSteamChinaLauncher;
        set
        {
            if (_IsSteamChinaLauncher != value)
            {
                _IsSteamChinaLauncher = value;
                this.RaisePropertyChanged();
            }
        }
    }

    bool _IsDisposedClient = true;

    /// <summary>
    /// 是否已经释放 SteamClient
    /// </summary>
    public bool IsDisposedClient
    {
        get => _IsDisposedClient;
        set
        {
            if (_IsDisposedClient != value)
            {
                _IsDisposedClient = value;
                this.RaisePropertyChanged();
            }
        }
    }

    #endregion

    bool _IsLoadingGameList;

    public bool IsLoadingGameList
    {
        get => _IsLoadingGameList;
        set => this.RaiseAndSetIfChanged(ref _IsLoadingGameList, value);
    }

    #region 启用监听 Steam 下载

    bool _IsWatchSteamDownloading;

    public bool IsWatchSteamDownloading
    {
        get => _IsWatchSteamDownloading;
        set => this.RaiseAndSetIfChanged(ref _IsWatchSteamDownloading, value);
    }

    public HashSet<uint> WatchDownloadingSteamAppIds { get; } = new();

    #endregion

    public void RunAFKApps()
    {
        var configured = Ioc.Get_Nullable<IPartialGameLibrarySettings>()?.AFKAppList;
        if (configured is not { Count: > 0 }) return;
        var snapshot = configured.ToArray();
        Task2.InBackground(() =>
        {
            var started = StartConfiguredIdleApps(snapshot);
            if (started > 0)
                Toast.Show(ToastIcon.Success, string.Format(Strings.L10nCore_SteamIdleStarted, started));
        });
    }

    public void Initialize()
    {
        ObjectDisposedException.ThrowIf(disposedValue, this);
        if (Interlocked.CompareExchange(ref connectionMonitorStarted, 1, 0) != 0) return;
        if (!stmService.IsRunningSteamProcess && SteamSettings.IsAutoRunSteam.Value)
            stmService.StartSteamWithParameter();

        Task2.InBackground(() =>
        {
            var observedProcessId = stmService.GetSteamProcessPid();
            while (!disposedValue)
            {
                try
                {
                    if (!TryInitializeSteamConnection(ref observedProcessId, out var connection))
                        continue;
                    var (user, country, serverTime) = connection;

                    if (SteamSettings.IsEnableSteamLaunchNotification.Value)
                        INotificationService.Instance.Notify(
                            $"{AppResources.Steam_CheckStarted}{(IsSteamChinaLauncher ? AppResources.Steam_SteamChina : AppResources.Steam_SteamWorld)}{Environment.NewLine}{AppResources.Steam_CurrentUser}{user.SteamNickName}{Environment.NewLine}{AppResources.Steam_CurrentIPCountry}{country}{Environment.NewLine}{AppResources.Steam_ServerTime}{serverTime:yyyy-MM-dd HH:mm:ss}",
                            NotificationType.Message);

                    if (Ioc.Get_Nullable<IPartialGameLibrarySettings>()?.IsAutoAFKApps == true)
                    {
                        RunAFKApps();
                    }

                    if (!SteamUsers.Lookup(user.SteamId64).HasValue)
                    {
                        RefreshSteamUsers();
                        _ = RefreshSteamUsersInfo();
                    }
                }
                catch (Exception ex)
                {
                    ex.LogAndShowT(nameof(SteamConnectService));
                }
                finally
                {
                    Thread.Sleep(3000);
                }
            }
        }, true);
    }

    // A successful native session is independent of optional launcher command-line metadata.
    // The monitor alone performs launch effects when this returns true for a new connection.
    internal bool TryInitializeSteamConnection(ref int observedProcessId,
        out (SteamUser User, string Country, DateTimeOffset ServerTime) connection)
    {
        connection = default;
        if (disposedValue) return false;
        var pathGeneration = Volatile.Read(ref steamPathGeneration);
        var processId = stmService.GetSteamProcessPid();
        IsRunningSteamProcess = processId != 0;
        if (processId != observedProcessId)
        {
            observedProcessId = processId;
            IsConnectToSteam = false;
            CurrentSteamUser = null;
            IsSteamChinaLauncher = false;
            if (InvalidateGameList())
                _ = RefreshGamesListAsync(showToast: false);
        }

        if (processId == 0 || IsConnectToSteam || !steamClientGate.Wait(0))
            return false;

        long id;
        string country;
        DateTimeOffset serverTime;
        try
        {
            if (!IsDisposedClient) DisposeSteamClient();
            if (!swLocalService.Initialize()) return false;
            IsDisposedClient = false;
            id = swLocalService.GetSteamId64();
            if (id <= 0 || id == SteamIdConvert.UndefinedId) return false;
            country = swLocalService.GetIPCountry();
            serverTime = ((long)swLocalService.GetServerRealTime()).ToDateTimeOffsetS();
        }
        finally
        {
            try { DisposeSteamClient(); }
            finally { steamClientGate.Release(); }
        }

        if (pathGeneration != Volatile.Read(ref steamPathGeneration) ||
            processId != stmService.GetSteamProcessPid()) return false;
        if (ShouldRefreshGameList(processId, id))
            _ = RefreshGamesListAsync(showToast: false);

        var user = swWebService.GetUserInfo(id).GetAwaiter().GetResult();
        var isSteamChinaLauncher = false;
        try
        {
            isSteamChinaLauncher = stmService.IsSteamChinaLauncher();
        }
        catch (Exception ex)
        {
            // Classification is optional; report its failure without retrying a valid native session.
            Log.Warn(nameof(SteamConnectService), ex, "Steam launcher classification unavailable.");
        }

        if (disposedValue || pathGeneration != Volatile.Read(ref steamPathGeneration) ||
            processId != stmService.GetSteamProcessPid()) return false;
        user.IPCountry = country;
        CurrentSteamUser = user;
        IsSteamChinaLauncher = isSteamChinaLauncher;
        IsConnectToSteam = true;
        connection = (user, country, serverTime);
        return true;
    }

    public bool Initialize(int appid)
    {
        if (stmService.IsRunningSteamProcess)
        {
            return IsConnectToSteam = swLocalService.Initialize(appid);
        }

        return false;
    }

    // A Steam process change invalidates the old load, even before a new account is available.
    bool InvalidateGameList()
    {
        lock (gameListSync)
        {
            if (!gameListRequested) return false;
            gameListProcessId = -1;
            gameListSteamId = -1;
            gameListCancellation?.Cancel();
            return true;
        }
    }

    // 仅在已经加载过列表时，新的 Steam 会话才需要重新读取；未加载过则由页面激活负责首次加载。
    bool ShouldRefreshGameList(int processId, long steamId)
    {
        lock (gameListSync)
        {
            if (!gameListRequested || gameListCancellation is { IsCancellationRequested: false }) return false;
            return gameListProcessId != processId || gameListSteamId != steamId;
        }
    }

    public async Task SaveGameAppEditAsync(SteamApp app, SteamAppEdit edit)
    {
        await gameListGate.WaitAsync();
        try
        {
            // The cached app can change while its editor is open (account or path switch).
            var current = SteamApps.Lookup(app.AppId);
            if (!current.HasValue || !ReferenceEquals(current.Value, app))
                throw new InvalidOperationException("The Steam library changed while editing this game.");

            await stmService.SaveLocalAppInfoEditsAsync(new Dictionary<uint, SteamAppEdit> { [app.AppId] = edit });
            app.ApplyAppInfoEdit(edit);
            SteamApps.Refresh(app);
        }
        finally
        {
            gameListGate.Release();
        }
    }

    public async Task<int> ImportGameAppEditsAsync(IReadOnlyDictionary<uint, SteamAppEdit> edits)
    {
        await gameListGate.WaitAsync();
        try
        {
            var changes = new Dictionary<uint, SteamAppEdit>();
            foreach (var (id, edit) in edits)
            {
                var app = SteamApps.Lookup(id);
                if (!app.HasValue || edit.IsEmpty)
                    continue;
                var original = app.Value.GetOriginalEditValues();
                changes[id] = SteamAppEdit.Capture(app.Value).WithOverrides(edit).Diff(original);
            }

            if (changes.Count == 0)
                return 0;
            await stmService.SaveLocalAppInfoEditsAsync(changes);
            SteamApps.Edit(updater =>
            {
                foreach (var (id, edit) in changes)
                {
                    var current = updater.Lookup(id);
                    if (!current.HasValue) continue;
                    current.Value.ApplyAppInfoEdit(edit);
                    updater.Refresh(current.Value);
                }
            });
            return changes.Count;
        }
        finally
        {
            gameListGate.Release();
        }
    }


    public void InitializeDownloadGameList()
    {
        var apps = stmService.GetDownloadingAppList();
        foreach (var app in apps)
            app.IsWatchDownloading = WatchDownloadingSteamAppIds.Contains(app.AppId);

        DownloadApps.Edit(updater =>
        {
            updater.Clear();
            updater.AddOrUpdate(apps);
        });
    }

    static void WatchDownloadingComplete()
    {
        var endMode = SteamSettings.DownloadCompleteSystemEndMode?.Value ?? OSExitMode.Sleep;
        var endModeName = EnumModel.GetLocalizationName(endMode);
        if (endModeName != null)
            AppResources.ResourceManager.GetString(endModeName);

        INotificationService.Instance.Notify(
            AppResources.GameList_SteamShutdown_DownloadCompleteTip.Format(30, endModeName), NotificationType.Message);

        switch (endMode)
        {
            case OSExitMode.Hibernate:
                IPlatformService.Instance.SystemHibernate();
                break;
            case OSExitMode.Shutdown:
                IPlatformService.Instance.SystemShutdown();
                break;
            //case OSExitMode.Lock:
            //    IPlatformService.Instance.SystemLock();
            //    break;
            default:
                IPlatformService.Instance.SystemSleep();
                break;
        }
    }

    public Task RefreshGamesListAsync() => RefreshGamesListAsync(showToast: true);

    internal async Task RefreshGamesListAsync(bool showToast)
    {
        var cancellation = new CancellationTokenSource();
        lock (gameListSync)
        {
            if (disposedValue)
            {
                cancellation.Dispose();
                return;
            }

            gameListRequested = true;
            gameListCancellation?.Cancel();
            gameListCancellation = cancellation;
            IsLoadingGameList = true;
        }

        var acquired = false;
        try
        {
            await gameListGate.WaitAsync(cancellation.Token).ConfigureAwait(false);
            acquired = true;
            var refreshed = await LoadGameListAsync(cancellation).ConfigureAwait(false);
            if (showToast && refreshed && !cancellation.IsCancellationRequested)
                Toast.Show(ToastIcon.Success, AppResources.GameList_RefreshGamesListSucess);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (OperationCanceledException)
        {
            var sessionChanged = false;
            lock (gameListSync)
            {
                if (ReferenceEquals(gameListCancellation, cancellation))
                {
                    gameListProcessId = -1;
                    gameListSteamId = -1;
                    SteamApps.Clear();
                    sessionChanged = true;
                }
            }

            if (sessionChanged)
            {
                IsConnectToSteam = false;
                CurrentSteamUser = null;
            }
        }
        catch (Exception ex)
        {
            ex.LogAndShowT(nameof(SteamConnectService));
        }
        finally
        {
            if (acquired) gameListGate.Release();
            lock (gameListSync)
            {
                if (ReferenceEquals(gameListCancellation, cancellation))
                {
                    gameListCancellation = null;
                    IsLoadingGameList = false;
                }
            }

            cancellation.Dispose();
        }
    }

    async Task<bool> LoadGameListAsync(CancellationTokenSource cancellation)
    {
        var token = cancellation.Token;
        var processId = stmService.GetSteamProcessPid();
        var clientAcquired = false;
        var attemptedInitialize = false;
        long steamId = 0;
        try
        {
            if (processId != 0)
            {
                await steamClientGate.WaitAsync(token).ConfigureAwait(false);
                clientAcquired = true;
                if (!IsDisposedClient) DisposeSteamClient();
                attemptedInitialize = true;
                if (swLocalService.Initialize())
                {
                    IsDisposedClient = false;
                    var id = swLocalService.GetSteamId64();
                    if (id > 0 && id != SteamIdConvert.UndefinedId)
                        steamId = id;
                }

                if (steamId == 0 || (CurrentSteamUser is { } current && current.SteamId64 != steamId))
                {
                    CurrentSteamUser = null;
                    IsConnectToSteam = false;
                }
            }

            lock (gameListSync)
            {
                token.ThrowIfCancellationRequested();
                if (!ReferenceEquals(gameListCancellation, cancellation)) throw new OperationCanceledException(token);
                gameListProcessId = processId;
                gameListSteamId = steamId;
            }

            var modifications = stmService.GetAppInfoEdits();
            // Steam can start before it has an account session. Use the local cache in that case.
            var hasLiveSession = steamId != 0;
            var replaceOldGames = true;

            long nextSessionCheck = 0;

            void CheckSession(bool force = false)
            {
                token.ThrowIfCancellationRequested();
                var now = Environment.TickCount64;
                if (!force && now < nextSessionCheck) return;
                nextSessionCheck = now + 500;
                if (stmService.GetSteamProcessPid() != processId ||
                    (hasLiveSession && swLocalService.GetSteamId64() != steamId))
                    throw new OperationCanceledException();
            }

            await stmService.ReadAppInfosInBatchesAsync(batch =>
            {
                CheckSession();
                var owned = hasLiveSession ? new List<SteamApp>(batch.Count) : null;
                foreach (var app in hasLiveSession ? swLocalService.OwnsApps(batch) : batch)
                {
                    if (modifications.TryGetValue(app.AppId, out var edit))
                        app.ApplyAppInfoEdit(edit);
                    owned?.Add(app);
                }

                lock (gameListSync)
                {
                    token.ThrowIfCancellationRequested();
                    if (!ReferenceEquals(gameListCancellation, cancellation))
                        throw new OperationCanceledException(token);
                    var games = (IReadOnlyList<SteamApp>?)owned ?? batch;
                    if (games.Count == 0) return;
                    if (replaceOldGames)
                    {
                        SteamApps.Edit(updater =>
                        {
                            updater.Clear();
                            updater.AddOrUpdate(games);
                        });
                        replaceOldGames = false;
                    }
                    else
                    {
                        SteamApps.AddOrUpdate(games);
                    }
                }
            }, token).ConfigureAwait(false);

            CheckSession(force: true);
            lock (gameListSync)
            {
                token.ThrowIfCancellationRequested();
                if (!ReferenceEquals(gameListCancellation, cancellation)) throw new OperationCanceledException(token);
                // An empty disk-cache read does not establish that the game library is empty.
                // Only a live Steam session can replace the current list with zero owned games.
                if (replaceOldGames && hasLiveSession) SteamApps.Clear();
                InitializeDownloadGameList();
                return !replaceOldGames || hasLiveSession;
            }
        }
        finally
        {
            if (clientAcquired)
            {
                try
                {
                    if (attemptedInitialize) DisposeSteamClient();
                }
                finally { steamClientGate.Release(); }
            }
        }
    }

    public void RefreshSteamUsers(bool requireReadSuccess = false)
    {
        var generation = Volatile.Read(ref steamPathGeneration);
        List<SteamUser> list;
        if (requireReadSuccess)
        {
            if (!stmService.TryGetRememberUserList(out list))
                throw new IOException("Unable to read Steam's saved account list.");
        }
        else
        {
            list = stmService.GetRememberUserList();
        }
        lock (gameListSync)
        {
            if (generation != Volatile.Read(ref steamPathGeneration)) return;
            SteamUsers.Clear();
            if (!list.Any_Nullable()) return;
            SteamUsers.AddOrUpdate(list);
        }

        #region 加载备注信息和 JumpList

        var accountRemarks = Ioc.Get_Nullable<IPartialGameAccountSettings>()?.AccountRemarks;

#if WINDOWS
        List<(string title, string applicationPath, string iconResourcePath, string arguments, string description,
            string customCategory)>? jumplistData = new();
#endif
        foreach (var user in SteamUsers.Items)
        {
            if (accountRemarks?.TryGetValue("Steam-" + user.SteamId64, out var remark) == true &&
                !string.IsNullOrEmpty(remark))
                user.Remark = remark;

#if WINDOWS
            {
                var title = user.SteamNickName ?? user.SteamId64.ToString(CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(user.Remark))
                {
                    title = user.SteamNickName + "(" + user.Remark + ")";
                }

                var processPath = Environment.ProcessPath;
                processPath.ThrowIsNull();
                if (!string.IsNullOrEmpty(user.AccountName))
                {
                    jumplistData!.Add((title,
                        applicationPath: processPath,
                        iconResourcePath: processPath,
                        arguments: $"-clt steam -account {user.AccountName}",
                        description: AppResources.UserChange_BtnTootlip,
                        customCategory: AppResources.UserFastChange));
                }
            }
#endif
        }

#if WINDOWS
        if (jumplistData.Any())
        {
            MainThread2.BeginInvokeOnMainThread(async () =>
            {
                var s = IJumpListService.Instance;
                await s.AddJumpItemsAsync(jumplistData);
            });
        }
#endif

        SteamUsers.Refresh();

        #endregion

        #region 通过 WebApi 加载头像图片用户信息

        //foreach (var user in SteamUsers.Items)
        //{
        //    var temp = await swWebService.GetUserInfo(user.SteamId64);
        //    if (!string.IsNullOrEmpty(temp.SteamID))
        //    {
        //        user.SteamID = temp.SteamID;
        //        user.OnlineState = temp.OnlineState;
        //        user.MemberSince = temp.MemberSince;
        //        user.VacBanned = temp.VacBanned;
        //        user.Summary = temp.Summary;
        //        user.PrivacyState = temp.PrivacyState;
        //        user.AvatarIcon = temp.AvatarIcon;
        //        user.AvatarMedium = temp.AvatarMedium;
        //        user.AvatarFull = temp.AvatarFull;
        //        user.MiniProfile = temp.MiniProfile;

        //        //if (user.MiniProfile != null && !string.IsNullOrEmpty(user.MiniProfile.AnimatedAvatar))
        //        //{
        //        //    user.AvatarStream = ImageChannelType.SteamAvatars.GetImageAsync(user.MiniProfile.AnimatedAvatar);
        //        //}
        //        //else
        //        //{
        //        //    user.AvatarStream = ImageChannelType.SteamAvatars.GetImageAsync(temp.AvatarFull);
        //        //}
        //    }
        //}

        //SteamUsers.Refresh();

        #endregion

        #region 加载动态头像头像框数据

        //foreach (var item in _SteamUsersSourceList.Items)
        //{
        //    item.MiniProfile = await webApiService.GetUserMiniProfile(item.SteamId3_Int);
        //    var miniProfile = item.MiniProfile;
        //    if (miniProfile != null)
        //    {
        //        if (!string.IsNullOrEmpty(miniProfile.AnimatedAvatar))
        //            item.AvatarStream = httpService.GetImageAsync(miniProfile.AnimatedAvatar, ImageChannelType.SteamAvatars);

        //        if (!string.IsNullOrEmpty(miniProfile.AvatarFrame))
        //            miniProfile.AvatarFrameStream = httpService.GetImageAsync(miniProfile.AvatarFrame, ImageChannelType.SteamAvatars);

        //        //item.Level = miniProfile.Level;
        //    }
        //}
        //_SteamUsersSourceList.Refresh();

        #endregion
    }

    public async Task RefreshSteamUsersInfo()
    {
        #region 通过 WebApi 加载头像图片用户信息

        foreach (var user in SteamUsers.Items)
        {
            var temp = await swWebService.GetUserInfo(user.SteamId64);
            if (!string.IsNullOrEmpty(temp.SteamID))
            {
                user.SteamID = temp.SteamID;
                user.OnlineState = temp.OnlineState;
                user.MemberSince = temp.MemberSince;
                user.VacBanned = temp.VacBanned;
                user.Summary = temp.Summary;
                user.PrivacyState = temp.PrivacyState;
                user.AvatarIcon = temp.AvatarIcon;
                user.AvatarMedium = temp.AvatarMedium;
                user.AvatarFull = temp.AvatarFull;
                user.MiniProfile = temp.MiniProfile;

                //if (user.MiniProfile != null && !string.IsNullOrEmpty(user.MiniProfile.AnimatedAvatar))
                //{
                //    user.AvatarStream = ImageChannelType.SteamAvatars.GetImageAsync(user.MiniProfile.AnimatedAvatar);
                //}
                //else
                //{
                //    user.AvatarStream = ImageChannelType.SteamAvatars.GetImageAsync(temp.AvatarFull);
                //}
            }
        }

        SteamUsers.Refresh();

        #endregion
    }

    public void DisposeSteamClient()
    {
        IsDisposedClient = true;
        swLocalService.DisposeSteamClient();
    }

    void Dispose(bool disposing)
    {
        if (disposedValue) return;
        disposedValue = true;
        if (disposing)
        {
            lock (gameListSync) gameListCancellation?.Cancel();
            lock (idleProcessGate)
            {
                foreach (var app in RuningSteamApps.Values)
                    app.StopSteamAppProcess();
                RuningSteamApps.Clear();
            }

            steamClientGate.Wait();
            try
            {
                if (!IsDisposedClient) DisposeSteamClient();
            }
            finally { steamClientGate.Release(); }
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

#endif
}