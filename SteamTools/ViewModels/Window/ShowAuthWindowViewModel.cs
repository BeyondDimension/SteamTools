using Microsoft.Win32;
using SteamTools.Models;
using SteamTools.Models.Settings;
using SteamTools.Properties;
using SteamTools.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using SteamTool.Core.Common;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WinAuth;

namespace SteamTools.ViewModels
{
    public class ShowAuthWindowViewModel : MainWindowViewModelBase
    {
        private SteamAuthenticator _Authenticator;

        public ShowAuthWindowViewModel()
        {
            this.Title = ProductInfo.Title + " | " + Resources.Auth_DetailTitle;
        }

        public ShowAuthWindowViewModel(WinAuthAuthenticator auth)
        {
            _Authenticator = auth.AuthenticatorData as SteamAuthenticator;
            this.Title = ProductInfo.Title + " | " + Resources.Auth_DetailTitle;
        }

        public string RecoveryCode => JObject.Parse(_Authenticator.SteamData).SelectToken("revocation_code").Value<string>();

        public string SteamData => _Authenticator.SteamData;

        public string DeviceId => _Authenticator.DeviceId;

    }
}
