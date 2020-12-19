using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Livet;
using SteamTool.Core.Common;
using SteamTool.Model;
using SteamTool.Steam.Service;
using SteamTool.Steam.Service.Local;
using SteamTool.Steam.Service.Web;
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

        public void Initialize()
        {

            Task.Run(async () =>
             {
                 Thread.CurrentThread.IsBackground = true;
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
                                     //该64位id的steamID3等于0，是steam未获取到当前登录用户的默认返回值，所以直接重新获取
                                     continue;
                                 }
                                 IsConnectToSteam = true;
                                 CurrentSteamUser = await steamDbApiService.GetUserInfo(id);
                                 var mainViewModel = (WindowService.Current.MainWindow as MainWindowViewModel);
                                 mainViewModel.SteamAppPage.Initialize();
#if DEBUG
                                 mainViewModel.SystemTabItems[mainViewModel.SystemTabItems.Count - 1].Initialize();
#endif
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
             }).ContinueWith(s => { Logger.Error(s.Exception); WindowService.Current.ShowDialogWindow(s.Exception.Message); }, TaskContinuationOptions.OnlyOnFaulted);
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
    }
}
