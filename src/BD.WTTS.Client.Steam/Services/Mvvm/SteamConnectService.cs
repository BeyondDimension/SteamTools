using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public sealed class SteamConnectService : ReactiveObject, IDisposable
{
    bool disposedValue;

    public const int SteamAFKMaxCount = 32;

    public static SteamConnectService Current { get; } = new();

    readonly ISteamworksLocalApiService swLocalService = ISteamworksLocalApiService.Instance;
    readonly ISteamService stmService = ISteamService.Instance;
    readonly ISteamworksWebApiService swWebService = ISteamworksWebApiService.Instance;

    SteamConnectService()
    {
        SteamApps = new SourceCache<SteamApp, uint>(t => t.AppId);
        DownloadApps = new SourceCache<SteamApp, uint>(t => t.AppId);
        SteamUsers = new SourceCache<SteamUser, long>(t => t.SteamId64);

        DownloadApps
            .Connect()
            .Subscribe(_ =>
            {
                foreach (var app in DownloadApps.Items)
                {
                    var optional = SteamApps.Lookup(app.AppId);
                    if (optional.HasValue)
                    {
                        var value = optional.Value;
                        value.InstalledDir = app.InstalledDir;
                        value.State = app.State;
                        value.SizeOnDisk = app.SizeOnDisk;
                        value.LastOwner = app.LastOwner;
                        value.BytesToDownload = app.BytesToDownload;
                        value.BytesDownloaded = app.BytesDownloaded;
                        value.BytesToStage = app.BytesToStage;
                        value.BytesStaged = app.BytesStaged;
                        value.LastUpdated = app.LastUpdated;

                        SteamApps.Refresh(app);
                    }
                }
            });

        this.WhenValueChanged(x => x.IsWatchSteamDownloading, false)
            .Subscribe(x =>
            {
                if (x)
                {
                    InitializeDownloadGameList();
                    if (OperatingSystem2.IsLinux())
                        IPlatformService.Instance.TryGetSystemUserPassword();
                    stmService.StartWatchSteamDownloading(app =>
                    {
                        var optional = DownloadApps.Lookup(app.AppId);
                        if (!optional.HasValue)
                        {
                            DownloadApps.AddOrUpdate(app);
                        }
                        else
                        {
                            var current = optional.Value;
                            current.InstalledDir = app.InstalledDir;
                            current.State = app.State;
                            current.SizeOnDisk = app.SizeOnDisk;
                            current.LastOwner = app.LastOwner;
                            current.BytesToDownload = app.BytesToDownload;
                            current.BytesDownloaded = app.BytesDownloaded;
                            current.BytesToStage = app.BytesToStage;
                            current.BytesStaged = app.BytesStaged;
                            current.LastUpdated = app.LastUpdated;
                        }

                        if (WatchDownloadingSteamAppIds.Contains(app.AppId))
                        {
                            if (app.IsDownloading)
                            {
                                app.IsWatchDownloading = true;
                            }
                            else
                            {
                                WatchDownloadingSteamAppIds.Remove(app.AppId);
                                if (!WatchDownloadingSteamAppIds.Any())
                                {
                                    WatchDownloadingComplete();
                                }
                            }
                        }
                    }, appid =>
                    {
                        DownloadApps.RemoveKey(appid);
                    });
                }
                else
                {
                    stmService.StopWatchSteamDownloading();
                }
            });
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
        if (GameLibrarySettings.AFKAppList.Value?.Count > 0)
        {
            foreach (var item in GameLibrarySettings.AFKAppList.Value)
            {
                RuningSteamApps.TryGetValue(item.Key, out var runState);
                if (runState == null)
                    RuningSteamApps.TryAdd(item.Key, new SteamApp
                    {
                        AppId = item.Key,
                        Name = item.Value
                    });
            }
            var t = new Task(() =>
            {
                foreach (var item in RuningSteamApps.Values)
                {
                    if (item.Process == null)
                        item.StartSteamAppProcess();
                }
            });
            t.Start();
        }
    }

    public void Initialize()
    {
        if (!stmService.IsRunningSteamProcess && SteamSettings.IsAutoRunSteam.Value)
            stmService.StartSteamWithParameter();

        Task.Factory.StartNew(async () =>
        {
            Thread.CurrentThread.IsBackground = true;
            while (true)
            {
                try
                {
                    if (stmService.IsRunningSteamProcess)
                    {
                        if (!IsConnectToSteam && IsDisposedClient)
                        {
                            if (swLocalService.Initialize())
                            {
                                IsDisposedClient = false;
                                var id = swLocalService.GetSteamId64();
                                if (id <= 0 || id == SteamUser.UndefinedId)
                                {
                                    // 该 64 位 id 的 steamID3 等于0，是 steam 未获取到当前登录用户的默认返回值，所以直接重新获取
                                    DisposeSteamClient();
                                    continue;
                                }
                                IsConnectToSteam = true;

                                CurrentSteamUser = await swWebService.GetUserInfo(id);
                                CurrentSteamUser.AvatarStream = ImageChannelType.SteamAvatars.GetImageAsync(CurrentSteamUser.AvatarFull);
                                //AvatarPath = ImageSouce.TryParse(await CurrentSteamUser.AvatarStream, isCircle: true);
                                CurrentSteamUser.IPCountry = swLocalService.GetIPCountry();

                                IsSteamChinaLauncher = stmService.IsSteamChinaLauncher();
                                var steamServerTime = ((long)swLocalService.GetServerRealTime()).ToDateTimeOffsetS();
                                //var steamRunTime = swLocalService.GetSecondsSinceAppActive();

                                #region 初始化需要 Steam 启动才能使用的功能

                                if (SteamSettings.IsEnableSteamLaunchNotification.Value)
                                {
                                    INotificationService.Instance.Notify($"{AppResources.Steam_CheckStarted}{(IsSteamChinaLauncher ? AppResources.Steam_SteamChina : AppResources.Steam_SteamWorld)}{Environment.NewLine}{AppResources.Steam_CurrentUser}{CurrentSteamUser.SteamNickName}{Environment.NewLine}{AppResources.Steam_CurrentIPCountry}{CurrentSteamUser.IPCountry}{Environment.NewLine}{AppResources.Steam_ServerTime}{steamServerTime:yyyy-MM-dd HH:mm:ss}", NotificationType.Message);
                                }

                                if (GameLibrarySettings.IsAutoAFKApps.Value)
                                {
                                    RunAFKApps();
                                }

                                // 仅在有游戏数据情况下加载登录用户的游戏
                                if (SteamApps.Items.Any())
                                {
                                    var applist = swLocalService.OwnsApps(await ISteamService.Instance.GetAppInfos());
                                    if (!applist.Any_Nullable())
                                    {
                                        if (!IsDisposedClient)
                                            DisposeSteamClient();
                                        continue;
                                    }
                                    LoadGames(applist);
                                    InitializeDownloadGameList();
                                }

                                if (!SteamUsers.Lookup(id).HasValue)
                                {
                                    RefreshSteamUsers();
                                }

                                //var mainViewModel = (IWindowService.Instance.MainWindow as WindowViewModel);
                                //await mainViewModel.SteamAppPage.Initialize();
                                //await mainViewModel.AccountPage.Initialize(id);

                                #endregion
                                if (!IsDisposedClient)
                                    DisposeSteamClient();
                            }
                        }
                    }
                    else
                    {
                        IsConnectToSteam = false;
                        CurrentSteamUser = null;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(SteamConnectService), ex, "SteamConnect Task LongRunning");
                    ToastService.Current.Notify(ex.Message);
                }
                finally
                {
                    await Task.Delay(3000);
                }
            }
        }, TaskCreationOptions.LongRunning).ConfigureAwait(false);
    }

    public bool Initialize(int appid)
    {
        if (stmService.IsRunningSteamProcess)
        {
            return IsConnectToSteam = swLocalService.Initialize(appid);
        }
        return false;
    }

    private void LoadGames(IEnumerable<SteamApp>? apps)
    {
        SteamApps.Clear();
        if (apps.Any_Nullable())
        {
            SteamApps.AddOrUpdate(apps);

            var modifiedApps = ISteamService.Instance.GetModifiedApps();
            if (modifiedApps.Any_Nullable())
            {
                foreach (var modifiedApp in modifiedApps)
                {
                    modifiedApp.ReadChanges();

                    if (modifiedApp.Changes != null)
                    {
                        var optional = SteamApps.Lookup(modifiedApp.AppId);
                        if (optional.HasValue)
                        {
                            var value = optional.Value;
                            value.ExtractReaderProperty(modifiedApp.Changes);
                            value.OriginalData = modifiedApp.OriginalData;
                            value.IsEdited = true;
                            SteamApps.Refresh(value);
                        }
                    }
                }
            }
        }
    }

    public async void InitializeGameList()
    {
        SteamApps.Clear();
        LoadGames(await ISteamService.Instance.GetAppInfos());
        //UpdateGamesImage();
        InitializeDownloadGameList();
        IsLoadingGameList = false;
    }

    public void InitializeDownloadGameList()
    {
        var apps = ISteamService.Instance.GetDownloadingAppList();
        if (apps.Any_Nullable())
        {
            DownloadApps.Clear();

            if (WatchDownloadingSteamAppIds.Any())
                foreach (var app in apps)
                {
                    app.IsWatchDownloading = WatchDownloadingSteamAppIds.Contains(app.AppId);
                }

            DownloadApps.AddOrUpdate(apps);
        }
    }

    private void WatchDownloadingComplete()
    {
        var endMode = SteamSettings.DownloadCompleteSystemEndMode?.Value ?? OSExitMode.Sleep;

        INotificationService.Instance.Notify(string.Format(AppResources.GameList_SteamShutdown_DownloadCompleteTip, 30, AppResources.ResourceManager.GetString(Enum2.GetDescription(endMode) ?? "System Sleep", R.Culture)), NotificationType.Message);

        switch (endMode)
        {
            case SystemEndMode.Hibernate:
                IPlatformService.Instance.SystemHibernate();
                break;
            case SystemEndMode.Shutdown:
                IPlatformService.Instance.SystemShutdown();
                break;
            //case SystemEndMode.Lock:
            //    IPlatformService.Instance.SystemLock();
            //    break;
            default:
                IPlatformService.Instance.SystemSleep();
                break;
        }
    }

    public async Task RefreshGamesList()
    {
        if (IsLoadingGameList == false)
        {
            IsLoadingGameList = true;
            if (stmService.IsRunningSteamProcess && OperatingSystem2.IsWindows())
            {
                await Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            if (stmService.IsRunningSteamProcess && IsDisposedClient)
                            {
                                if (swLocalService.Initialize())
                                {
                                    IsDisposedClient = false;
                                    SteamApps.Clear();
                                    var apps = await ISteamService.Instance.GetAppInfos();
                                    if (apps != null)
                                    {
                                        var temps = swLocalService.OwnsApps(apps);
                                        LoadGames(temps);
                                        InitializeDownloadGameList();
                                        Toast.Show(AppResources.GameList_RefreshGamesListSucess);
                                        DisposeSteamClient();
                                        IsLoadingGameList = false;
                                        return;
                                    }
                                }
                            }
                            await Task.Delay(2000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(SteamConnectService), ex, "Task RefreshGamesList");
                        ToastService.Current.Notify(ex.Message);
                        IsLoadingGameList = false;
                        DisposeSteamClient();
                    }
                }, TaskCreationOptions.DenyChildAttach).ConfigureAwait(false);
            }
            else
            {
                await Task.Run(InitializeGameList);
                Toast.Show(AppResources.GameList_RefreshGamesListSucess);
            }
        }
    }

    public async void RefreshSteamUsers()
    {
        var list = stmService.GetRememberUserList();

        if (!list.Any_Nullable())
        {
            return;
        }
        SteamUsers.AddOrUpdate(list);

        #region 加载备注信息和 JumpList

        IReadOnlyDictionary<long, string?>? accountRemarks = SteamAccountSettings.AccountRemarks.Value;

        List<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)>? jumplistData = OperatingSystem2.IsWindows() ? new() : null;
        foreach (var user in SteamUsers.Items)
        {
            if (accountRemarks?.TryGetValue(user.SteamId64, out var remark) == true &&
                !string.IsNullOrEmpty(remark))
                user.Remark = remark;

            if (OperatingSystem.IsWindows())
            {
                var title = user.SteamNickName ?? user.SteamId64.ToString(CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(user.Remark))
                {
                    title = user.SteamNickName + "(" + user.Remark + ")";
                }

                if (!string.IsNullOrEmpty(user.AccountName)) jumplistData!.Add((
                    title: title,
                    applicationPath: IApplication.ProgramPath,
                    iconResourcePath: IApplication.ProgramPath,
                    arguments: $"-clt steam -account {user.AccountName}",
                    description: AppResources.UserChange_BtnTootlip,
                    customCategory: AppResources.UserFastChange));
            }
        }

        if (jumplistData.Any_Nullable())
        {
            MainThread2.BeginInvokeOnMainThread(async () =>
            {
                var s = IJumpListService.Instance;
                await s.AddJumpItemsAsync(jumplistData);
            });
        }

        SteamUsers.Refresh();

        #endregion

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

                if (user.MiniProfile != null && !string.IsNullOrEmpty(user.MiniProfile.AnimatedAvatar))
                {
                    user.AvatarStream = ImageChannelType.SteamAvatars.GetImageAsync(user.MiniProfile.AnimatedAvatar);
                }
                else
                {
                    user.AvatarStream = ImageChannelType.SteamAvatars.GetImageAsync(temp.AvatarFull);
                }
            }
        }

        SteamUsers.Refresh();

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

    public void DisposeSteamClient()
    {
        IsDisposedClient = true;
        swLocalService.DisposeSteamClient();
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                foreach (var app in Current.RuningSteamApps.Values)
                {
                    if (app.Process != null)
                        if (!app.Process.HasExited)
                            app.Process.KillEntireProcessTree();
                }
                DisposeSteamClient();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
