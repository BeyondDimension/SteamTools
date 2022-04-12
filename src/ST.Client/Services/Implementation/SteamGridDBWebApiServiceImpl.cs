using System.Application.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    internal sealed class SteamGridDBWebApiServiceImpl : ISteamGridDBWebApiServiceImpl
    {
        readonly IHttpService s;

        const string ApiKey = "ae93db7411cac53190aa5a9b633bf5e2";

        public SteamGridDBWebApiServiceImpl(IHttpService s)
        {
            this.s = s;
        }

        public async Task<SteamGridApp?> GetSteamGridAppBySteamAppId(long appId)
        {
            var url = string.Format(SteamGridDBApiUrls.RetrieveGameBySteamAppId_Url, appId);
            var rsp = await s.SendAsync<SteamGridAppData>
                (url, () =>
                 {
                     var rq = new HttpRequestMessage(HttpMethod.Get, url);
                     rq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                     return rq;
                 }, MediaTypeNames.JSON, default);

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
            url += $"nsfw=any&humor=any";
            var rsp = await s.SendAsync<SteamGridItemData>
                            (url, () =>
                            {
                                var rq = new HttpRequestMessage(HttpMethod.Get, url);
                                rq.Headers.Authorization = new Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
                                return rq;
                            }, MediaTypeNames.JSON, default);

            if (rsp != null)
            {
                if (rsp.Success == true)
                {
                    foreach (var item in rsp.Data)
                    {
                        item.GridType = type;
                    }
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