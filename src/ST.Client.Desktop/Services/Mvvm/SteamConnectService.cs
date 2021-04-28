using DynamicData;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public class SteamConnectService : ReactiveObject
    {
        #region static members
        public static SteamConnectService Current { get; } = new();
        #endregion

        private readonly ISteamworksLocalApiService ApiService = ISteamworksLocalApiService.Instance;
        private readonly ISteamworksWebApiService SteamworksWebApiService = ISteamworksWebApiService.Instance;
        private readonly ISteamDbWebApiService steamDbApiService = ISteamDbWebApiService.Instance;
        private readonly ISteamService SteamTool = ISteamService.Instance;

        public SteamConnectService()
        {
            SteamApps = new SourceCache<SteamApp, uint>(t => t.AppId);
        }

        #region Steam游戏列表
        public SourceCache<SteamApp, uint> SteamApps { get; }
        #endregion

        #region 运行中的游戏列表
        private IList<SteamApp> _RuningSteamApps = new List<SteamApp>();
        public IList<SteamApp> RuningSteamApps
        {
            get => _RuningSteamApps;
            set
            {
                if (_RuningSteamApps != value)
                {
                    _RuningSteamApps = value;
                    this.RaisePropertyChanged();
                }
            }
        }
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
        #endregion

        #region 是否已经释放SteamClient
        private bool _IsDisposedClient;
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


        public void Initialize()
        {
            if (!SteamTool.IsRunningSteamProcess && SteamSettings.IsAutoRunSteam.Value)
                SteamTool.StartSteam(SteamSettings.SteamStratParameter);

            Task.Run(InitializeGameList).ForgetAndDispose();

            var t = new Task(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    while (true)
                    {
                        if (SteamTool.IsRunningSteamProcess)
                        {
                            if (!IsConnectToSteam)
                            {
                                if (ApiService.Initialize())
                                {
                                    var id = ApiService.GetSteamId64();
                                    if (id == SteamUser.UndefinedId)
                                    {
                                        //该64位id的steamID3等于0，是steam未获取到当前登录用户的默认返回值，所以直接重新获取
                                        Current.DisposeSteamClient();
                                        continue;
                                    }
                                    IsConnectToSteam = true;
                                    CurrentSteamUser = await SteamworksWebApiService.GetUserInfo(id);
                                    CurrentSteamUser.IPCountry = ApiService.GetIPCountry();
                                    IsSteamChinaLauncher = ApiService.IsSteamChinaLauncher();

                                    #region 初始化需要steam启动才能使用的功能
                                    InitializeGameList();

                                    while (true)
                                    {
                                        if (SteamApps.Items.Any())
                                        {
                                            LoadGames(ApiService.OwnsApps(SteamApps.Items));
                                            break;
                                        }
                                    }

                                    //var mainViewModel = (IWindowService.Instance.MainWindow as WindowViewModel);
                                    //await mainViewModel.SteamAppPage.Initialize();
                                    //await mainViewModel.AccountPage.Initialize(id);
                                    #endregion

                                    DisposeSteamClient();
                                }
                            }
                        }
                        else
                        {
                            IsConnectToSteam = false;
                        }
                        Thread.Sleep(2000);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(SteamConnectService), ex, "Task LongRunning");
                    ToastService.Current.Notify(ex.Message);
                }
            }, TaskCreationOptions.LongRunning);

            //t.Forget();
            t.Start();
        }

        public void Initialize(int appid)
        {
            if (SteamTool.IsRunningSteamProcess)
            {
                IsConnectToSteam = ApiService.Initialize(appid);
            }
        }

        private void LoadGames(IEnumerable<SteamApp> apps)
        {
            SteamApps.Clear();
            if (apps.Any())
                SteamApps.AddOrUpdate(apps);
        }

        public async void InitializeGameList()
        {
            LoadGames(await ISteamService.Instance.GetAppInfos());
            //UpdateGamesImage();
        }

        public void UpdateGamesImage()
        {
#if DEBUG
            if (BuildConfig.IsDebuggerAttached)
            {
                return;
            }
#endif
            if (SteamApps.Items.Any())
            {
                Parallel.ForEach(SteamApps.Items, new ParallelOptions
                {
                    MaxDegreeOfParallelism = (Environment.ProcessorCount / 2) + 1
                }, async app =>
                {
                    await ISteamService.Instance.LoadAppImageAsync(app);
                    //app.LibraryLogoStream = await IHttpService.Instance.GetImageAsync(app.LibraryLogoUrl, ImageChannelType.SteamGames);
                    //app.LibraryHeaderStream = await IHttpService.Instance.GetImageAsync(app.LibraryHeaderUrl, ImageChannelType.SteamGames);
                    //app.LibraryNameStream = await IHttpService.Instance.GetImageAsync(app.LibraryNameUrl, ImageChannelType.SteamGames);
                    //app.HeaderLogoStream = await IHttpService.Instance.GetImageAsync(app.HeaderLogoUrl, ImageChannelType.SteamGames);
                });
            }
        }

        public /*async*/ void RefreshGamesList()
        {
            var t = new Task(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    while (true)
                    {
                        if (SteamTool.IsRunningSteamProcess)
                        {
                            if (ApiService.Initialize())
                            {
                                while (true)
                                {
                                    InitializeGameList();
                                    if (SteamApps.Items.Any())
                                    {
                                        LoadGames(ApiService.OwnsApps(SteamApps.Items));
                                        //UpdateGamesImage();
                                        Toast.Show(AppResources.GameList_RefreshGamesListSucess);
                                        DisposeSteamClient();
                                        return;
                                    }
                                }
                            }
                        }
                        Thread.Sleep(2000);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(SteamConnectService), ex, "Task RefreshGamesList");
                    ToastService.Current.Notify(ex.Message);
                }
            }, TaskCreationOptions.LongRunning);
            t.Start();
        }

        public void Dispose()
        {
            foreach (var app in Current.RuningSteamApps)
            {
                if (app.Process != null)
                    if (!app.Process.HasExited)
                        app.Process.Kill();
            }
            DisposeSteamClient();
        }

        public void DisposeSteamClient()
        {
            ApiService.DisposeSteamClient();
            IsDisposedClient = true;
        }
    }
}
