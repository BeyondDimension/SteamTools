using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface ISteamworksWebApiService
    {
        public static ISteamworksWebApiService Instance => DI.Get<ISteamworksWebApiService>();

        Task<string> GetAllSteamAppsString();

        Task<List<SteamApp>> GetAllSteamAppList();

        Task<SteamUser> GetUserInfo(long steamId64);

        Task<SteamMiniProfile> GetUserMiniProfile(long steamId3);
    }
}