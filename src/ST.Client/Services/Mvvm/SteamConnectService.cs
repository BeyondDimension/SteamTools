using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Settings;
using System.Application.UI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Application.Services
{
    public sealed class SteamConnectService : ReactiveObject, IDisposable
    {
        static SteamConnectService? mCurrent;
        public static SteamConnectService Current => mCurrent ?? new();

        readonly ISteamworksLocalApiService ApiService = ISteamworksLocalApiService.Instance;
        //readonly ISteamworksWebApiService SteamworksWebApiService = ISteamworksWebApiService.Instance;
        //readonly ISteamDbWebApiService steamDbApiService = ISteamDbWebApiService.Instance;
        readonly ISteamService SteamTool = ISteamService.Instance;
        public const int SteamAFKMaxCount = 32;

        private SteamConnectService()
        {
            mCurrent = this;

            SteamApps = new SourceCache<SteamApp, uint>(t => t.AppId);
            DownloadApps = new SourceCache<SteamApp, uint>(t => t.AppId);

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
                        if (OperatingSystem2.IsLinux)
                            IPlatformService.Instance.TryGetSystemUserPassword();
                        SteamTool.StartWatchSteamDownloading(app =>
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
                        SteamTool.StopWatchSteamDownloading();
                    }
                });
        }

        #region Steam游戏列表
        public SourceCache<SteamApp, uint> SteamApps { get; }
        public SourceCache<SteamApp, uint> DownloadApps { get; }
        #endregion

        #region 运行中的游戏列表
        //private ConcurrentDictionary<uint, SteamApp> _RuningSteamApps = new();
        public ConcurrentDictionary<uint, SteamApp> RuningSteamApps { get; } = new ConcurrentDictionary<uint, SteamApp>();
        #endregion

        #region 当前steam登录用户
        private SteamUser? _CurrentSteamUser;
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

        #endregion

        #region 连接steamclient是否成功
        private bool _IsConnectToSteam;
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

        private bool _IsSteamChinaLauncher;
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

        private bool _IsDisposedClient = true;
        /// <summary>
        /// 是否已经释放SteamClient
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

        private bool _IsLoadingGameList;
        public bool IsLoadingGameList
        {
            get => _IsLoadingGameList;
            set => this.RaiseAndSetIfChanged(ref _IsLoadingGameList, value);
        }

        #region 启用监听Steam下载

        private bool _IsWatchSteamDownloading;
        public bool IsWatchSteamDownloading
        {
            get => _IsWatchSteamDownloading;
            set => this.RaiseAndSetIfChanged(ref _IsWatchSteamDownloading, value);
        }

        public HashSet<uint> WatchDownloadingSteamAppIds { get; } = new HashSet<uint>();
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
            if (!SteamTool.IsRunningSteamProcess && SteamSettings.IsAutoRunSteam.Value)
                SteamTool.StartSteam(SteamSettings.SteamStratParameter.Value);

            Task.Factory.StartNew(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    try
                    {
                        if (SteamTool.IsRunningSteamProcess)
                        {
                            if (!IsConnectToSteam && IsDisposedClient)
                            {
                                if (ApiService.Initialize())
                                {
                                    IsDisposedClient = false;
                                    var id = ApiService.GetSteamId64();
                                    if (id == SteamUser.UndefinedId)
                                    {
                                        //该64位id的steamID3等于0，是steam未获取到当前登录用户的默认返回值，所以直接重新获取
                                        DisposeSteamClient();
                                        continue;
                                    }
                                    IsConnectToSteam = true;
                                    CurrentSteamUser = await DI.Get<ISteamworksWebApiService>().GetUserInfo(id);
                                    CurrentSteamUser.AvatarStream = IHttpService.Instance.GetImageAsync(CurrentSteamUser.AvatarFull, ImageChannelType.SteamAvatars);
                                    //AvatarPath = ImageSouce.TryParse(await CurrentSteamUser.AvatarStream, isCircle: true);

                                    CurrentSteamUser.IPCountry = ApiService.GetIPCountry();
                                    IsSteamChinaLauncher = ApiService.IsSteamChinaLauncher();

                                    #region 初始化需要steam启动才能使用的功能
                                    if (SteamSettings.IsEnableSteamLaunchNotification.Value)
                                    {
                                        INotificationService.Instance.Notify($"{AppResources.Steam_CheckStarted}{(IsSteamChinaLauncher ? AppResources.Steam_SteamChina : AppResources.Steam_SteamWorld)}{Environment.NewLine}{AppResources.Steam_CurrentUser}{CurrentSteamUser.SteamNickName}{Environment.NewLine}{AppResources.Steam_CurrentIPCountry}{CurrentSteamUser.IPCountry}", NotificationType.Message);
                                    }

                                    if (GameLibrarySettings.IsAutoAFKApps.Value)
                                    {
                                        RunAFKApps();
                                    }

                                    //仅在有游戏数据情况下加载登录用户的游戏
                                    if (SteamApps.Items.Any())
                                    {
                                        LoadGames(ApiService.OwnsApps(await ISteamService.Instance.GetAppInfos()));
                                        InitializeDownloadGameList();
                                    }
                                    //var mainViewModel = (IWindowService.Instance.MainWindow as WindowViewModel);
                                    //await mainViewModel.SteamAppPage.Initialize();
                                    //await mainViewModel.AccountPage.Initialize(id);
                                    #endregion
                                    if (!IsDisposedClient)
                                    {
                                        DisposeSteamClient();
                                    }
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
            if (SteamTool.IsRunningSteamProcess)
            {
                return IsConnectToSteam = ApiService.Initialize(appid);
            }
            return false;
        }

        private void LoadGames(IEnumerable<SteamApp>? apps)
        {
            SteamApps.Clear();
            if (apps.Any_Nullable())
                SteamApps.AddOrUpdate(apps!);
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
            var endMode = SteamSettings.DownloadCompleteSystemEndMode?.Value ?? SystemEndMode.Sleep;

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

        public async void RefreshGamesList()
        {
            if (IsLoadingGameList == false)
            {
                IsLoadingGameList = true;
                if (SteamTool.IsRunningSteamProcess)
                {
                    await Task.Factory.StartNew(async () =>
                     {
                         try
                         {
                             while (true)
                             {
                                 if (SteamTool.IsRunningSteamProcess && IsDisposedClient)
                                 {
                                     if (ApiService.Initialize())
                                     {
                                         IsDisposedClient = false;
                                         SteamApps.Clear();
                                         var apps = await ISteamService.Instance.GetAppInfos();
                                         if (apps.Any())
                                         {
                                             var temps = ApiService.OwnsApps(apps);
                                             LoadGames(temps);
                                             InitializeDownloadGameList();
                                             Toast.Show(AppResources.GameList_RefreshGamesListSucess);
                                             DisposeSteamClient();
                                             IsLoadingGameList = false;
                                             return;
                                         }
                                     }
                                     DisposeSteamClient();
                                 }
                                 await Task.Delay(2000);
                             }
                         }
                         catch (Exception ex)
                         {
                             Log.Error(nameof(SteamConnectService), ex, "Task RefreshGamesList");
                             ToastService.Current.Notify(ex.Message);
                         }
                     }, TaskCreationOptions.DenyChildAttach).Forget().ConfigureAwait(false);
                }
                else
                {
                    await Task.Run(InitializeGameList);
                    Toast.Show(AppResources.GameList_RefreshGamesListSucess);
                }
            }
        }

        public void DisposeSteamClient()
        {
            ApiService.DisposeSteamClient();
            IsDisposedClient = true;
        }

        public void Dispose()
        {
            foreach (var app in Current.RuningSteamApps.Values)
            {
                if (app.Process != null)
                    if (!app.Process.HasExited)
                        app.Process.Kill();
            }
            DisposeSteamClient();
        }
    }
}
