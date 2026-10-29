#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using BD.SteamClient.Helpers;
using AppResources = BD.WTTS.Client.Resources.Strings;
#endif

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public sealed class SteamConnectService
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
        ISteamworksWebApiService webService)
    {
        stmService = steamService;
        swLocalService = localService;
        swWebService = webService;
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

        this.WhenValueChanged(x => x.IsWatchSteamDownloading, false)
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
        var aFKAppList = Ioc.Get_Nullable<IPartialGameLibrarySettings>()?.AFKAppList;
        if (aFKAppList?.Count > 0)
        {
            foreach (var item in aFKAppList)
            {
                RuningSteamApps.TryGetValue(item.Key, out var runState);
                if (runState == null)
                    RuningSteamApps.TryAdd(item.Key, new SteamApp { AppId = item.Key, Name = item.Value, });
            }

            Toast.Show(ToastIcon.Success, $"已自动后台运行挂时长 {aFKAppList.Count} 个游戏");
            Task2.InBackground(() =>
            {
                foreach (var item in RuningSteamApps.Values)
                {
                    if (item.Process == null || item.Process.HasExited)
                        item.StartSteamAppProcess();
                }
            });
        }
    }

    public void Initialize()
    {
        if (!stmService.IsRunningSteamProcess && SteamSettings.IsAutoRunSteam.Value)
            stmService.StartSteamWithParameter();

        Task2.InBackground(() =>
        {
            var observedProcessId = stmService.GetSteamProcessPid();
            while (!disposedValue)
            {
                try
                {
                    var pathGeneration = Volatile.Read(ref steamPathGeneration);
                    var processId = stmService.GetSteamProcessPid();
                    IsRunningSteamProcess = processId != 0;
                    if (processId != observedProcessId)
                    {
                        observedProcessId = processId;
                        IsConnectToSteam = false;
                        CurrentSteamUser = null;
                        InvalidateGameList(clear: processId != 0);
                    }

                    if (processId == 0 || IsConnectToSteam || !steamClientGate.Wait(0))
                        continue;
                    long id = 0;
                    string country = string.Empty;
                    DateTimeOffset serverTime = default;

                    try
                    {
                        if (!IsDisposedClient) DisposeSteamClient();
                        if (!swLocalService.Initialize()) continue;
                        IsDisposedClient = false;
                        id = swLocalService.GetSteamId64();
                        if (id <= 0 || id == SteamIdConvert.UndefinedId) continue;
                        country = swLocalService.GetIPCountry();
                        serverTime = ((long)swLocalService.GetServerRealTime()).ToDateTimeOffsetS();
                    }
                    finally
                    {
                        try { DisposeSteamClient(); }
                        finally { steamClientGate.Release(); }
                    }

                    if (pathGeneration != Volatile.Read(ref steamPathGeneration) ||
                        processId != stmService.GetSteamProcessPid()) continue;
                    if (ShouldRefreshGameList(processId, id))
                        _ = RefreshGamesListAsync(showToast: false);

                    var user = swWebService.GetUserInfo(id).GetAwaiter().GetResult();
                    if (pathGeneration != Volatile.Read(ref steamPathGeneration) ||
                        processId != stmService.GetSteamProcessPid()) continue;
                    user.IPCountry = country;
                    CurrentSteamUser = user;
                    IsSteamChinaLauncher = stmService.IsSteamChinaLauncher();
                    IsConnectToSteam = true;

                    if (SteamSettings.IsEnableSteamLaunchNotification.Value)
                        INotificationService.Instance.Notify(
                            $"{AppResources.Steam_CheckStarted}{(IsSteamChinaLauncher ? AppResources.Steam_SteamChina : AppResources.Steam_SteamWorld)}{Environment.NewLine}{AppResources.Steam_CurrentUser}{user.SteamNickName}{Environment.NewLine}{AppResources.Steam_CurrentIPCountry}{country}{Environment.NewLine}{AppResources.Steam_ServerTime}{serverTime:yyyy-MM-dd HH:mm:ss}",
                            NotificationType.Message);

                    if (Ioc.Get_Nullable<IPartialGameLibrarySettings>()?.IsAutoAFKApps == true)
                    {
                        RunAFKApps();
                    }

                    if (!SteamUsers.Lookup(id).HasValue)
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

    public bool Initialize(int appid)
    {
        if (stmService.IsRunningSteamProcess)
        {
            return IsConnectToSteam = swLocalService.Initialize(appid);
        }

        return false;
    }

    // Steam 退出时保留缓存，启动新进程时清除旧账号的游戏。
    void InvalidateGameList(bool clear)
    {
        lock (gameListSync)
        {
            if (!gameListRequested) return;
            gameListProcessId = -1;
            gameListSteamId = -1;
            gameListCancellation?.Cancel();
            if (clear) SteamApps.Clear();
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

    public int ApplyModifiedApps(IEnumerable<ModifiedApp>? modifiedApps)
    {
        var appliedCount = 0;
        if (!modifiedApps.Any_Nullable())
        {
            return appliedCount;
        }

        SteamApps.Edit(updater =>
        {
            foreach (var modifiedApp in modifiedApps)
            {
                modifiedApp.ReadChanges();

                if (modifiedApp.Changes == null)
                    continue;

                var optional = updater.Lookup(modifiedApp.AppId);
                if (!optional.HasValue)
                    continue;

                var value = optional.Value;
                value.ExtractReaderProperty(modifiedApp.Changes);
                value.OriginalData = modifiedApp.OriginalData;
                value.IsEdited = true;
                updater.Refresh(value);
                appliedCount++;
            }
        });

        return appliedCount;
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
            await LoadGameListAsync(cancellation).ConfigureAwait(false);
            if (showToast) Toast.Show(ToastIcon.Success, AppResources.GameList_RefreshGamesListSucess);
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

    async Task LoadGameListAsync(CancellationTokenSource cancellation)
    {
        var token = cancellation.Token;
        var processId = stmService.IsRunningSteamProcess ? stmService.GetSteamProcessPid() : 0;
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
                if (!swLocalService.Initialize())
                    throw new InvalidOperationException(AppResources.Error_SteamGetUserInfo);
                IsDisposedClient = false;
                steamId = swLocalService.GetSteamId64();
                if (steamId <= 0 || steamId == SteamIdConvert.UndefinedId)
                    throw new OperationCanceledException();
                if (CurrentSteamUser is { } current && current.SteamId64 != steamId)
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
                SteamApps.Clear();
            }

            var modifications = new Dictionary<uint, ModifiedApp>();
            if (stmService.GetModifiedApps() is { } modifiedApps)
            {
                foreach (var modified in modifiedApps)
                {
                    modified.ReadChanges();
                    if (modified.Changes != null) modifications[modified.AppId] = modified;
                }
            }

            long nextSessionCheck = 0;

            void CheckSession(bool force = false)
            {
                token.ThrowIfCancellationRequested();
                var now = Environment.TickCount64;
                if (!force && now < nextSessionCheck) return;
                nextSessionCheck = now + 500;
                if (stmService.GetSteamProcessPid() != processId ||
                    (processId != 0 && swLocalService.GetSteamId64() != steamId))
                    throw new OperationCanceledException();
            }

            await stmService.ReadAppInfosInBatchesAsync(batch =>
            {
                CheckSession();
                var visible = new List<SteamApp>(batch.Count);
                foreach (var app in processId != 0 ? swLocalService.OwnsApps(batch) : batch)
                {
                    if (modifications.TryGetValue(app.AppId, out var modified))
                    {
                        app.ExtractReaderProperty(modified.Changes!);
                        app.OriginalData = modified.OriginalData;
                        app.IsEdited = true;
                    }

                    visible.Add(app);
                }

                lock (gameListSync)
                {
                    token.ThrowIfCancellationRequested();
                    if (!ReferenceEquals(gameListCancellation, cancellation))
                        throw new OperationCanceledException(token);
                    SteamApps.AddOrUpdate(visible);
                }
            }, token).ConfigureAwait(false);

            CheckSession(force: true);
            lock (gameListSync)
            {
                token.ThrowIfCancellationRequested();
                if (!ReferenceEquals(gameListCancellation, cancellation)) throw new OperationCanceledException(token);
                InitializeDownloadGameList();
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

    public void RefreshSteamUsers()
    {
        var generation = Volatile.Read(ref steamPathGeneration);
        var list = stmService.GetRememberUserList();
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
            foreach (var app in RuningSteamApps.Values)
            {
                if (app.Process != null && !app.Process.HasExited)
                    app.Process.KillEntireProcessTree();
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