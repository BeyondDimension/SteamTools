using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    internal sealed class SteamGridDBWebApiServiceImpl : ISteamGridDBWebApiServiceImpl
    {
        readonly IHttpService s;

        public SteamGridDBWebApiServiceImpl(IHttpService s)
        {
            this.s = s;
        }

        public async Task<SteamGridApp?> GetSteamGridAppBySteamAppId(long appId)
        {
            var rsp = await s.GetAsync<SteamGridAppData>(String.Format(SteamGridDBApiUrls.RetrieveGameBySteamAppId_Url, appId));

            if (rsp != null)
            {
                if (rsp.Success == true)
                {
                    return rsp.Data;
                }
                else
                {
                    Log.Error(nameof(GetSteamGridAppBySteamAppId), string.Join(",", rsp.Errors));
                }
            }
            return null;
        }

        public async Task<List<SteamGridItem>?> GetSteamGridItemsByGameId(long gameId, SteamGridItemType type = SteamGridItemType.Grid)
        {
            var url = type switch
            {
                SteamGridItemType.Hero => string.Format(SteamGridDBApiUrls.RetrieveHeros_Url, gameId),
                SteamGridItemType.Logo => string.Format(SteamGridDBApiUrls.RetrieveLogos_Url, gameId),
                SteamGridItemType.Icon => string.Format(SteamGridDBApiUrls.RetrieveIcons_Url, gameId),
                _ => string.Format(SteamGridDBApiUrls.RetrieveGrids_Url, gameId),
            };

            var rsp = await s.GetAsync<SteamGridItemData>(url);

            if (rsp != null)
            {
                if (rsp.Success == true)
                {
                    return rsp.Data;
                }
                else
                {
                    Log.Error(nameof(GetSteamGridAppBySteamAppId), string.Join(",", rsp.Errors));
                }
            }
            return null;
        }

    }
}