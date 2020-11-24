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

    }
}
