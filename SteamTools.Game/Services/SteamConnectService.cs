using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteamTool.Core.Common;
using SteamTool.Model;
using SteamTool.Steam.Service;
using SteamTool.Steam.Service.Local;
using SteamTool.Steam.Service.Web;

namespace SteamTools.Game.Services
{
    public class SteamConnectService
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
                }
            }
        }


        public void Initialize(int appid)
        {
            if (Process.GetProcessesByName("steam").Length > 0)
            {
                if (!IsConnectToSteam)
                {
                    IsConnectToSteam = ApiService.Initialize(appid);
                    if (IsConnectToSteam)
                    {

                    }
                }
            }
            else
            {
                IsConnectToSteam = false;
            }
        }

    }
}
