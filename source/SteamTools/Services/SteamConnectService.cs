using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Livet;
using Newtonsoft.Json;
using SteamTool.Core;
using SteamTool.Core.Common;
using SteamTool.Model;
using SteamTool.Steam.Service;
using SteamTool.Steam.Service.Local;
using SteamTool.Steam.Service.Web;
using SteamTools.Models.Settings;
using SteamTools.Properties;
using SteamTools.ViewModels;

namespace SteamTools.Services
{
    public class SteamConnectService : NotificationObject
    {
        #region static members
        public static SteamConnectService Current { get; } = new SteamConnectService();
        #endregion

        //public readonly SteamDLLService DLLService = SteamService.Instance.Get<SteamDLLService>();
        public readonly SteamworksApiService ApiService = SteamService.Instance.Get<SteamworksApiService>();

        public readonly SteamworksWebApiService SteamworksWebApiService = SteamService.Instance.Get<SteamworksWebApiService>();
        public readonly SteamDbApiService steamDbApiService = SteamService.Instance.Get<SteamDbApiService>();
        private readonly SteamToolService SteamTool = SteamToolCore.Instance.Get<SteamToolService>();

        #region Steam游戏列表
        private IReadOnlyCollection<SteamApp> _SteamApps;
        public IReadOnlyCollection<SteamApp> SteamApps
        {
            get => _SteamApps;
            set
            {
                if (_SteamApps != value)
                {
                    _SteamApps = value;
                    this.RaisePropertyChanged();
                }
            }
        }
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
        private SteamUser _CurrentSteamUser;
        public SteamUser CurrentSteamUser
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
            var t = new Task(async () =>
              {
                  Thread.CurrentThread.IsBackground = true;
                  try
                  {

                      while (true)
                      {
                          if (Process.GetProcessesByName("steam").Length > 0)
                          {
                              if (!IsConnectToSteam)
                              {
                                  if (ApiService.Initialize())
                                  {
                                      var id = ApiService.GetSteamId64();
                                      if (id == 76561197960265728)
                                      {
                                          //该64位id的steamID3等于0，是steam未获取到当前登录用户的默认返回值，所以直接重新获取，
                                          //希望这位用户不会用steam++，嗯...
                                          SteamConnectService.Current.DisposeSteamClient();
                                          continue;
                                      }
                                      IsConnectToSteam = true;
                                      CurrentSteamUser = await SteamworksWebApiService.GetUserInfo(id);
                                      CurrentSteamUser.IPCountry = ApiService.GetIPCountry();

                                      var mainViewModel = (WindowService.Current.MainWindow as MainWindowViewModel);
                                      await mainViewModel.SteamAppPage.Initialize();
#if DEBUG
                                      await mainViewModel.SystemTabItems[mainViewModel.SystemTabItems.Count - 1].Initialize();
#endif
                                      SteamConnectService.Current.DisposeSteamClient();
                                  }
                              }
                          }
                          else
                          {
                              IsConnectToSteam = false;
                              //StatusService.Current.Notify(Resources.Steam_Not_Runing);
                          }
                          await Task.Delay(1500);
                      }
                  }
                  catch (Exception ex)
                  {
                      Logger.Error(ex);
                      StatusService.Current.Notify(ex.Message);
                  }
              }, TaskCreationOptions.LongRunning);

            t.ContinueWith(s => { Logger.Error(s.Exception); WindowService.Current.ShowDialogWindow(s.Exception.Message); }, TaskContinuationOptions.OnlyOnFaulted);
            t.Start();
        }

        public void Initialize(int appid)
        {
            if (Process.GetProcessesByName("steam").Length > 0)
            {
                IsConnectToSteam = ApiService.Initialize(appid);
            }
        }

        public void Shutdown()
        {
            foreach (var app in SteamConnectService.Current.RuningSteamApps)
            {
                if (!app.Process.HasExited)
                    app.Process.Kill();
            }
        }

        public void DisposeSteamClient()
        {
            ApiService.SteamClient.Dispose();
            IsDisposedClient = true;
        }

        private bool IsRefreshGameListCache;
        public void RefreshGameListCache()
        {
            if (SteamConnectService.Current.IsConnectToSteam && IsDisposedClient == true)
            {
                if (SteamConnectService.Current.ApiService.Initialize())
                {
                    if (IsRefreshGameListCache)
                    {
                        StatusService.Current.Notify("正在下载Steam游戏数据中...");
                        return;
                    }
                    StatusService.Current.Notify("刷新Steam游戏数据");
                    IsRefreshGameListCache = true;
                    Task.Run(async () =>
                    {
                        var result = await SteamworksWebApiService.GetAllSteamAppsString();
                        if (GeneralSettings.IsSteamAppListLocalCache.Value)
                            SteamTool.UpdateAppListJson(result, Path.Combine(AppContext.BaseDirectory, Const.APP_LIST_FILE));
                        var apps = JsonConvert.DeserializeObject<SteamApps>(result).AppList.Apps;
                        apps = apps.DistinctBy(d => d.AppId).ToList();
                        //SteamConnectService.Current.SteamApps = apps;
                        SteamConnectService.Current.SteamApps = SteamConnectService.Current.ApiService.OwnsApps(apps);
                    }).ContinueWith(s =>
                    {
                        Logger.Error(s.Exception); WindowService.Current.ShowDialogWindow(s.Exception.Message);
                    }, TaskContinuationOptions.OnlyOnFaulted)
                    .ContinueWith(s =>
                    {
                        StatusService.Current.Notify("刷新Steam游戏数据完成");
                        IsRefreshGameListCache = false;
                        DisposeSteamClient();
                        s.Dispose();
                    });
                }
            }
            else
            {
                StatusService.Current.Notify("未检测到Steam运行");
            }
        }
    }
}
