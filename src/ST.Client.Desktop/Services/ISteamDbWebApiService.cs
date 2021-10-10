using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface ISteamDbWebApiService
    {
        public static ISteamDbWebApiService Instance => DI.Get<ISteamDbWebApiService>();

        Task<SteamUser> GetUserInfo(long steamId64);

        Task<List<SteamUser>> GetUserInfo(IEnumerable<long> steamId64s);

        Task<SteamApp> GetAppInfo(int appId);
    }
}