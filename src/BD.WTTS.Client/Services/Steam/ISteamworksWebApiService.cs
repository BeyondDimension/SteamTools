// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// Steamworks Web API 服务
/// </summary>
public interface ISteamworksWebApiService
{
    static ISteamworksWebApiService Instance => Ioc.Get<ISteamworksWebApiService>();

    Task<string> GetAllSteamAppsString();

    Task<List<SteamApp>> GetAllSteamAppList();

    /// <summary>
    /// 获取 Steam 个人资料
    /// </summary>
    /// <param name="steamId64"></param>
    /// <returns></returns>
    Task<SteamUser> GetUserInfo(long steamId64);

    /// <summary>
    /// 获取 Mini 资料
    /// </summary>
    /// <param name="steamId3"></param>
    /// <returns></returns>
    Task<SteamMiniProfile?> GetUserMiniProfile(long steamId3);
}