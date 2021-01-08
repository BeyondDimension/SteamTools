using Newtonsoft.Json;
using SteamTool.Core;
using SteamTool.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SteamTool.Steam.Service.Web
{
    public class SteamworksWebApiService
    {

        private readonly HttpServices httpServices = SteamToolCore.Instance.Get<HttpServices>();

        public async Task<string> GetAllSteamAppsString()
        {
            var r = await httpServices.Get(Const.STEAMAPP_LIST_URL);
            return r;
        }

        public async Task<List<SteamApp>> GetAllSteamAppList()
        {
            var r = await httpServices.Get(Const.STEAMAPP_LIST_URL);
            var apps = JsonConvert.DeserializeObject<SteamApps>(r);
            return apps.AppList.Apps;
        }

        public async Task<SteamUser> GetUserInfo(long steamId64)
        {
            var r = await httpServices.Get(string.Format(Const.STEAM_USERINFO_XML_URL, steamId64));
            if (!string.IsNullOrEmpty(r))
            {
                using (StringReader sr = new StringReader(r))
                {
                    var xmldes = new XmlSerializer(typeof(SteamUser));
                    return xmldes.Deserialize(sr) as SteamUser;
                }
            }
            return new SteamUser() { SteamId64 = steamId64 };
        }

    }
}
