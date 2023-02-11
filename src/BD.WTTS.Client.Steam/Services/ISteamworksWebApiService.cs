namespace BD.WTTS.Services;

/// <summary>
/// Steamworks Web API 服务
/// </summary>
public interface ISteamworksWebApiService
{
    static ISteamworksWebApiService Instance => Ioc.Get<ISteamworksWebApiService>();

    Task<string> GetAllSteamAppsString();

    Task<List<SteamApp>> GetAllSteamAppList();

    Task<SteamUser> GetUserInfo(long steamId64);

    Task<SteamMiniProfile?> GetUserMiniProfile(long steamId3);
}