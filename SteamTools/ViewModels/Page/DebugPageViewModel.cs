using MetroRadiance.Platform;
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
            //var test = SteamNetService.Current.APIService.Initialize();
            //DebugText += SteamNetService.Current.APIService.IsConnectToSteam;
            //DebugText += Environment.NewLine;
            //DebugText += SteamNetService.Current.APIService.GetSteamId64();
            //DebugText += Environment.NewLine;
        }
    }
}
