using MetroRadiance.Platform;
using SteamTool.Core.Common;
using SteamTool.Steam.Service.Local;
using SteamTools.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
    public class DebugPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => Properties.Resources.Debug;
            protected set { throw new NotImplementedException(); }
        }

        private string _DebugText;
        public string DebugText
        {
            get => this._DebugText;
            set
            {
                if (this._DebugText != value)
                {
                    this._DebugText = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public DebugPageViewModel()
        {

        }

        internal override void Initialize()
        {
            Task.Run(() =>
            {
                DebugText += SteamConnectService.Current.IsConnectToSteam;
                DebugText += Environment.NewLine;
                DebugText += SteamConnectService.Current.ApiService.GetSteamId64();
                DebugText += Environment.NewLine;
                DebugText += SteamConnectService.Current.ApiService.SteamClient.SteamUtils.GetAppId();
                DebugText += Environment.NewLine;
                DebugText += SteamConnectService.Current.ApiService.SteamClient.SteamApps008.IsAppInstalled(730);
                DebugText += Environment.NewLine;
                DebugText += SteamConnectService.Current.ApiService.SteamClient.SteamApps008.IsSubscribedFromFamilySharing();
                DebugText += Environment.NewLine;
                DebugText += SteamConnectService.Current.ApiService.SteamClient.SteamApps008.GetAppInstallDir(730);
                DebugText += Environment.NewLine;
            }).ContinueWith(s => { Logger.Error(s.Exception); WindowService.Current.ShowDialogWindow(s.Exception.Message); }, TaskContinuationOptions.OnlyOnFaulted).ContinueWith(s => s.Dispose());
        }

        public void Test_OnClick()
        {
        }
    }
}
