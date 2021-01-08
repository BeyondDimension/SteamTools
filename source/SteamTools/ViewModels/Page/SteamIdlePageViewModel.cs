using SteamTool.Model;
using SteamTools.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
    public class SteamIdlePageViewModel : TabItemViewModel
    {

        public override string Name
        {
            get { return Properties.Resources.IdleCard; }
            protected set { throw new NotImplementedException(); }
        }

        public void ExtractSteamCookies()
        {
            var login_url = new Uri(Const.STEAM_LOGIN_URL);
            var container = WinInet.GetUriCookieContainer(login_url);
            var cookies = container.GetCookies(login_url);

            foreach (Cookie cookie in cookies)
            {
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
