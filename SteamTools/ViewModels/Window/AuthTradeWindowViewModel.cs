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
    public class AuthTradeWindowViewModel : MainWindowViewModelBase
    {
        private SteamAuthenticator _Authenticator;

        public AuthTradeWindowViewModel()
        {
            this.Title = ProductInfo.Title + " | " + Resources.Auth_TradeTitle;
        }

        public AuthTradeWindowViewModel(WinAuthAuthenticator auth)
        {
            _Authenticator = auth.AuthenticatorData as SteamAuthenticator;
            this.Title = ProductInfo.Title + " | " + Resources.Auth_TradeTitle;
        }


    }
}
