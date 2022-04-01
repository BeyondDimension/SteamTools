using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    internal sealed class SteamGridDBWebApiServiceImpl
    {
        readonly IHttpService s;

        public SteamGridDBWebApiServiceImpl(IHttpService s)
        {
            this.s = s;
        }

        public async Task<string> GetAllSteamAppsString()
        {
            var rsp = await s.GetAsync<string>(SteamApiUrls.STEAMAPP_LIST_URL);
            return rsp ?? string.Empty;
        }

    }
}