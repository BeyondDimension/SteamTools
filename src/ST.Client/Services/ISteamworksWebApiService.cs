using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// Steamworks Web API 服务
    /// </summary>
    public interface ISteamworksWebApiService : IService<ISteamworksWebApiService>
    {
        Task<string> GetAllSteamAppsString();

        Task<List<SteamApp>> GetAllSteamAppList();

        Task<SteamUser> GetUserInfo(long steamId64);

        Task<SteamMiniProfile?> GetUserMiniProfile(long steamId3);
    }
}