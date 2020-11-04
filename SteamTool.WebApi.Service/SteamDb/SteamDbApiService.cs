using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using SteamTool.Core;
using SteamTool.Model;

namespace SteamTool.WebApi.Service.SteamDb
{
    public class SteamDbApiService
    {
        private const string UserInfoUrl = "https://steamdb.ml/api/v1/users/{0}";
        private const string AppInfoUrl = "https://steamdb.ml/api/v1/apps/{0}";

        private const string AppLogoUrl = "http://cdn.akamai.steamstatic.com/steamcommunity/public/images/apps/{0}/{1}.jpg";
        private const string AppIconUrl = "http://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.ico";
        private const string AppImageUrl = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/{1}";


        private readonly HttpServices httpServices = SteamToolCore.Instance.Get<HttpServices>();


        public SteamUser GetUserInfo(long steamId64)
        {
            var r = httpServices.Get(string.Format(UserInfoUrl, steamId64));
            var userInfo = JsonConvert.DeserializeObject<SteamUser>(r.Result);
            return userInfo;
        }

        public List<SteamUser> GetUserInfo(long[] steamId64s)
        {
            var users = new List<SteamUser>();
            foreach (var i in steamId64s) 
            {
                var r = httpServices.Get(string.Format(UserInfoUrl, i));
                var userInfo = JsonConvert.DeserializeObject<SteamUser>(r.Result);
                users.Add(userInfo);
            }
            return users;
        }


        public SteamApp GetAppInfo(long appId) 
        {
            var r = httpServices.Get(string.Format(AppInfoUrl, appId));
            var steamApp = JsonConvert.DeserializeObject<SteamApp>(r.Result);
            return steamApp;
        }

    }
}
