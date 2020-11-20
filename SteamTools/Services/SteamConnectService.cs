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


        public void Initialize()
        {
            Task.Run(async () =>
             {
                 while (true)
                 {
                     if (Process.GetProcessesByName("steam").Length > 0)
                     {
                         if (!IsConnectToSteam)
                         {
                             IsConnectToSteam = ApiService.Initialize();
                             if (IsConnectToSteam)
                             {
                                 CurrentSteamUser = await steamDbApiService.GetUserInfo(ApiService.GetSteamId64());
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
                         StatusService.Current.Set(Resources.Steam_Not_Runing_Tips);
                     }
                     await Task.Delay(1500);
                 }
             });
        }

        public void Initialize(int appid)
        {
            if (Process.GetProcessesByName("steam").Length > 0)
            {
                IsConnectToSteam = ApiService.Initialize(appid);
                if (IsConnectToSteam)
                {

                }
            }
        }
    }
}
