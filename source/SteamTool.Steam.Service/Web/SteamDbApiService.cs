using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using SteamTool.Core;
using SteamTool.Model;

namespace SteamTool.Steam.Service.Web
{
    public class SteamDbApiService
    {
        private readonly HttpServices httpServices = SteamToolCore.Instance.Get<HttpServices>();


        public async Task<SteamUser> GetUserInfo(long steamId64)
        {
            var r = await httpServices.Get(string.Format(Const.STEAMDB_USERINFO_URL, steamId64));
            if (!string.IsNullOrEmpty(r))
            {
                var userInfo = JsonConvert.DeserializeObject<SteamUser>(r);
                return userInfo;
            }
            return new SteamUser() { SteamId64 = steamId64 };
        }

        public async Task<List<SteamUser>> GetUserInfo(long[] steamId64s)
        {
            var users = new List<SteamUser>();
            foreach (var i in steamId64s)
            {
                users.Add(await GetUserInfo(i));
            }
            return users;
        }


        public async Task<SteamApp> GetAppInfo(int appId)
        {
            var r = await httpServices.Get(string.Format(Const.STEAMDB_APPINFO_URL, appId));
            if (!string.IsNullOrEmpty(r))
            {
                var steamApp = JsonConvert.DeserializeObject<SteamApp>(r);
                return steamApp;
            }
            return new SteamApp() { AppId = (uint)appId };
        }

    }
}
