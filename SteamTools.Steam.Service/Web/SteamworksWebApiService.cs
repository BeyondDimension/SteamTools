using Newtonsoft.Json;
using SteamTool.Core;
using SteamTool.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteamTool.Steam.Service.Web
{
    public class SteamworksWebApiService
    {
        public const string AppListUrl = "https://api.steampowered.com/ISteamApps/GetAppList/v2";

        private readonly HttpServices httpServices = SteamToolCore.Instance.Get<HttpServices>();

        public async Task<string> GetAllSteamAppsString()
        {
            var r = await httpServices.Get(AppListUrl);
            return r;
        }

        public async Task<List<SteamApp>> GetAllSteamAppList()
        {
            var r = await httpServices.Get(AppListUrl);
            var apps = JsonConvert.DeserializeObject<SteamApps>(r);
            return apps.AppList.Apps;
        }

    }
}
