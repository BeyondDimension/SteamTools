using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    internal sealed class SteamworksWebApiServiceImpl : ISteamworksWebApiService
    {
        readonly IHttpService s;

        public SteamworksWebApiServiceImpl(IHttpService s)
        {
            this.s = s;
        }

        public async Task<string> GetAllSteamAppsString()
        {
            var rsp = await s.GetAsync<string>(SteamApiUrls.STEAMAPP_LIST_URL);
            return rsp ?? string.Empty;
        }

        public async Task<List<SteamApp>> GetAllSteamAppList()
        {
            var rsp = await s.GetAsync<SteamApps>(SteamApiUrls.STEAMAPP_LIST_URL);
            return rsp?.AppList?.Apps ?? new List<SteamApp>();
        }

        public async Task<SteamUser> GetUserInfo(long steamId64)
        {
            var requestUri = string.Format(SteamApiUrls.STEAM_USERINFO_XML_URL, steamId64);
            var rsp = await s.GetAsync<SteamUser>(requestUri);
            return rsp ?? new SteamUser() { SteamId64 = steamId64 };
        }

        /// <summary>
        /// 获取mini资料
        /// </summary>
        /// <param name="steamId3"></param>
        /// <returns></returns>
        public async Task<SteamMiniProfile?> GetUserMiniProfile(long steamId3)
        {
            var requestUri = string.Format(SteamApiUrls.STEAM_MINIPROFILE_URL, steamId3);
            var rsp = await s.GetAsync<SteamMiniProfile>(requestUri);
            return rsp;
        }
    }
}