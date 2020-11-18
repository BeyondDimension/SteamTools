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

        private const string AppInfoUrl = "https://steamdb.ml/api/v1/apps/{0}";

        private const string AppLogoUrl = "http://cdn.akamai.steamstatic.com/steamcommunity/public/images/apps/{0}/{1}.jpg";
        private const string AppIconUrl = "http://steamcdn-a.akamaihd.net/steamcommunity/public/images/apps/{0}/{1}.ico";
        private const string AppImageUrl = "https://steamcdn-a.akamaihd.net/steam/apps/{0}/{1}";
        private const string AppHeaderImageUrl = "https://cdn.cloudflare.steamstatic.com/steam/apps/{0}/header.jpg";

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
