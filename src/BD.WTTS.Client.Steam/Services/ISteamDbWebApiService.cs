namespace BD.WTTS.Services;

/// <summary>
/// SteamDb WebApi 服务
/// </summary>
public interface ISteamDbWebApiService
{
    static ISteamDbWebApiService Instance => Ioc.Get<ISteamDbWebApiService>();

    Task<SteamUser> GetUserInfo(long steamId64);

    Task<List<SteamUser>> GetUserInfo(IEnumerable<long> steamId64s);

    Task<SteamApp> GetAppInfo(int appId);
}