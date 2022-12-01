using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    internal sealed class SteamDbWebApiServiceImpl : ISteamDbWebApiService
    {
        readonly IHttpService s;

        public SteamDbWebApiServiceImpl(IHttpService s)
        {
            this.s = s;
        }

        public async Task<SteamUser> GetUserInfo(long steamId64)
        {
            var requestUri = string.Format(SteamApiUrls.STEAMDB_USERINFO_URL, steamId64);
            var rsp = await s.GetAsync<SteamUser>(requestUri);
            return rsp ?? new SteamUser() { SteamId64 = steamId64 };
        }

        public async Task<List<SteamUser>> GetUserInfo(IEnumerable<long> steamId64s)
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
            var requestUri = string.Format(SteamApiUrls.STEAMDB_APPINFO_URL, appId);
            var rsp = await s.GetAsync<SteamApp>(requestUri);
            return rsp ?? new SteamApp() { AppId = (uint)appId };
        }
    }
}