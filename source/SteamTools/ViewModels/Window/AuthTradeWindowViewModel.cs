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
using System.Net;
using SteamTools.Win32;
using SteamTool.Model;

namespace SteamTools.ViewModels
{
    public class AuthTradeWindowViewModel : MainWindowViewModelBase
    {
        private SteamAuthenticator _Authenticator;

        public AuthTradeWindowViewModel(WinAuthAuthenticator auth)
        {
            _Authenticator = auth.AuthenticatorData as SteamAuthenticator;
            this.Title = ProductInfo.Title + " | " + Resources.Auth_TradeTitle;

            //var steam = _Authenticator.GetClient();

            //if (steam.IsLoggedIn() == false)
            //{
            //    ExtractSteamCookies(steam);
            //}
        }

        public void ExtractSteamCookies()
        {
            var login_url = new Uri(Const.STEAM_LOGIN_URL);
            var container = WinInet.GetUriCookieContainer(login_url);
            var cookies = container.GetCookies(login_url);
            var steam = _Authenticator.GetClient();

            foreach (Cookie cookie in cookies)
            {
                steam.Session.Cookies.Add(login_url, cookie);
                //if (cookie.Name == "sessionid")
                //{
                //    steam.Session.Cookies.Add(login_url, cookie);
                //}
                //else if (cookie.Name == "steamLogin")
                //{
                //    steam.Session.Cookies.Add(login_url, cookie);
                //    //Settings.Default.steamLogin = cookie.Value;
                //    //Settings.Default.myProfileURL = SteamProfile.GetSteamUrl();
                //}
                //else if (cookie.Name == "steamLoginSecure")
                //{
                //    steam.Session.Cookies.Add(login_url, cookie);
                //    //Settings.Default.myProfileURL = SteamProfile.GetSteamUrl();
                //}
                //else if (cookie.Name == "steamparental")
                //{
                //    //Settings.Default.steamparental = cookie.Value;
                //}
                //else if (cookie.Name == "steamRememberLogin")
                //{
                //    //Settings.Default.steamRememberLogin = cookie.Value;
                //}
            }


        }
    }
}
