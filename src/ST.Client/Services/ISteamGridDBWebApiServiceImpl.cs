using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// SteamGridDB Web API 服务
    /// </summary>
    public interface ISteamGridDBWebApiServiceImpl
    {
        static ISteamGridDBWebApiServiceImpl Instance => DI.Get<ISteamGridDBWebApiServiceImpl>();

        Task<SteamGridApp?> GetSteamGridAppBySteamAppId(long appId);
        Task<List<SteamGridItem>?> GetSteamGridItemsByGameId(long gameId, SteamGridItemType type = SteamGridItemType.Grid);
    }
}