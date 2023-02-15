// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// Steamworks 本地 API 服务
/// </summary>
public interface ISteamworksLocalApiService
{
    static ISteamworksLocalApiService Instance => Ioc.Get<ISteamworksLocalApiService>();

    protected const string TAG = "SteamworksLocalApiS";

    /// <summary>
    /// 当前平台是否支持
    /// </summary>
    bool IsSupported => false;

    void DisposeSteamClient() { }

    bool Initialize() => default;

    bool Initialize(int appid) => default;

    long GetSteamId64() => default;

    bool OwnsApps(uint appid) => default;

    IEnumerable<SteamApp> OwnsApps(IEnumerable<SteamApp> apps) => Array.Empty<SteamApp>();

    string GetAppData(uint appid, string key) => string.Empty;

    bool IsSteamChinaLauncher() => default;

    bool IsSteamInBigPictureMode() => default;

    uint GetSecondsSinceAppActive() => default;

    uint GetServerRealTime() => default;

    bool IsAppInstalled(uint appid) => default;

    string GetAppInstallDir(uint appid) => string.Empty;

    string GetIPCountry() => string.Empty;

    string GetCurrentGameLanguage() => string.Empty;

    string GetAvailableGameLanguages() => string.Empty;

    bool GetStatValue(string name, out int value)
    {
        value = default;
        return default;
    }

    bool GetStatValue(string name, out float value)
    {
        value = default;
        return default;
    }

    bool GetAchievementState(string name, out bool isAchieved)
    {
        isAchieved = default;
        return default;
    }

    bool GetAchievementAndUnlockTime(string name, out bool isAchieved, out long unlockTime)
    {
        isAchieved = default;
        unlockTime = default;
        return default;
    }

    bool GetAchievementAchievedPercent(string name, out float percent)
    {
        percent = default;
        return default;
    }

    void AddUserStatsReceivedCallback(Action<IUserStatsReceived> action) { }

    bool RequestCurrentStats() => default;

    bool ResetAllStats(bool achievementsToo) => default;

    bool SetAchievement(string name, bool state) => default;

    bool SetStatValue(string name, int value) => default;

    bool SetStatValue(string name, float value) => default;

    bool StoreStats() => default;

    void RequestGlobalAchievementPercentages() { }

    void RunCallbacks(bool server) { }

    interface IUserStatsReceived
    {
        ulong GameId { get; }

        int Result { get; }
    }

    #region SteamRemoteStorage

    bool GetCloudArchiveQuota(out ulong totalBytes, out ulong availableBytes)
    {
        totalBytes = 0;
        availableBytes = 0;
        return false;
    }

    List<SteamRemoteFile>? GetCloudArchiveFiles() => default;

    int FileRead(string name, byte[] buffer) => default;

    bool FileWrite(string name, byte[] buffer) => default;

    bool FileForget(string name) => default;

    bool FileDelete(string name) => default;

    #endregion
}