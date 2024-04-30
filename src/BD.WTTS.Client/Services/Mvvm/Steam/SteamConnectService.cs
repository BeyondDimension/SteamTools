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

    bool disposedValue;

    public const int SteamAFKMaxCount = 32;

    static readonly Lazy<SteamConnectService> mCurrent = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static SteamConnectService Current => mCurrent.Value;

    readonly ISteamworksLocalApiService swLocalService = ISteamworksLocalApiService.Instance;
    readonly ISteamService stmService = ISteamService.Instance;
    readonly ISteamworksWebApiService swWebService = ISteamworksWebApiService.Instance;

    SteamConnectService()
    {
        SteamApps = new(t => t.AppId);
        DownloadApps = new(t => t.AppId);
        SteamUsers = new(t => t.SteamId64);

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
#if LINUX
                    IPlatformService.Instance.GetSystemUserPassword();
#endif
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
                    RuningSteamApps.TryAdd(item.Key, new SteamApp
                    {
                        AppId = item.Key,
                        Name = item.Value,
                    });
            }
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
            while (true)
            {
                try
                {
                    IsRunningSteamProcess = stmService.IsRunningSteamProcess;
                    if (IsRunningSteamProcess)
                    {
                        if (!IsConnectToSteam && IsDisposedClient)
                        {
                            if (swLocalService.Initialize())
                            {
                                IsDisposedClient = false;
                                var id = swLocalService.GetSteamId64();
                                if (id <= 0 || id == SteamIdConvert.UndefinedId)
                                {
                                    // 该 64 位 id 的 steamID3 等于0，是 steam 未获取到当前登录用户的默认返回值，所以直接重新获取
                                    DisposeSteamClient();
                                    continue;
                                }
                                IsConnectToSteam = true;

                                var taskUser = swWebService.GetUserInfo(id);
                                taskUser.Wait();
                                CurrentSteamUser = taskUser.Result;
                                //CurrentSteamUser.AvatarStream = ImageChannelType.SteamAvatars.GetImageAsync(CurrentSteamUser.AvatarFull);
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

                                var isAutoAFKApps = Ioc.Get_Nullable<IPartialGameLibrarySettings>()?.IsAutoAFKApps ?? default;
                                if (isAutoAFKApps)
                                {
                                    RunAFKApps();
                                }

                                // 仅在有游戏数据情况下加载登录用户的游戏
                                if (SteamApps.Items.Any())
                                {
                                    var taskAppinfo = ISteamService.Instance.GetAppInfos();
                                    taskAppinfo.Wait();
                                    var applist = swLocalService.OwnsApps(taskAppinfo.Result);
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
                                    _ = RefreshSteamUsersInfo();
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

    void LoadGames(IEnumerable<SteamApp>? apps)
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

    static void WatchDownloadingComplete()
    {
        var endMode = SteamSettings.DownloadCompleteSystemEndMode?.Value ?? OSExitMode.Sleep;
        var endModeName = EnumModel.GetLocalizationName(endMode);
        if (endModeName != null)
            AppResources.ResourceManager.GetString(endModeName);

        INotificationService.Instance.Notify(AppResources.GameList_SteamShutdown_DownloadCompleteTip.Format(30, endModeName), NotificationType.Message);

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

    public async Task RefreshGamesListAsync()
    {
        if (IsLoadingGameList == false)
        {
            IsLoadingGameList = true;
            if (stmService.IsRunningSteamProcess)
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
                                        Toast.Show(ToastIcon.Success, AppResources.GameList_RefreshGamesListSucess);
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
                        ex.LogAndShowT(nameof(SteamConnectService));
                        IsLoadingGameList = false;
                        DisposeSteamClient();
                    }
                }, TaskCreationOptions.DenyChildAttach).ConfigureAwait(false);
            }
            else
            {
                await Task.Run(InitializeGameList);
                Toast.Show(ToastIcon.Success, AppResources.GameList_RefreshGamesListSucess);
            }
        }
    }

    public void RefreshSteamUsers()
    {
        var list = stmService.GetRememberUserList();

        if (!list.Any_Nullable())
        {
            return;
        }
        SteamUsers.Clear();
        SteamUsers.AddOrUpdate(list);

        #region 加载备注信息和 JumpList

        var accountRemarks = Ioc.Get_Nullable<IPartialGameAccountSettings>()?.AccountRemarks;

#if WINDOWS
        List<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)>? jumplistData = new();
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
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                foreach (var app in Current.RuningSteamApps.Values)
                {
                    if (app.Process != null && !app.Process.HasExited)
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

#endif
}