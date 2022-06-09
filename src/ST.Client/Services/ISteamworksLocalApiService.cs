using System.Application.Models;
using System.Collections.Generic;

namespace System.Application.Services
{
    /// <summary>
    /// Steamworks 本地 API 服务
    /// </summary>
    public interface ISteamworksLocalApiService
    {
        static ISteamworksLocalApiService Instance => DI.Get<ISteamworksLocalApiService>();

        protected const string TAG = "SteamworksLocalApiS";

        /// <summary>
        /// 当前平台是否支持
        /// </summary>
        bool IsSupported { get; }

        void DisposeSteamClient();

        bool Initialize();

        bool Initialize(int appid);

        long GetSteamId64();

        bool OwnsApps(uint appid);

        IEnumerable<SteamApp> OwnsApps(IEnumerable<SteamApp> apps);

        string GetAppData(uint appid, string key);

        bool IsSteamChinaLauncher();

        bool IsAppInstalled(uint appid);

        string GetAppInstallDir(uint appid);

        string GetIPCountry();

        string GetCurrentGameLanguage();

        string GetAvailableGameLanguages();

        bool GetStatValue(string name, out int value);

        bool GetStatValue(string name, out float value);

        bool GetAchievementState(string name, out bool isAchieved);

        bool GetAchievementAndUnlockTime(string name, out bool isAchieved, out long unlockTime);

        bool GetAchievementAchievedPercent(string name, out float percent);

        void AddUserStatsReceivedCallback(Action<IUserStatsReceived> action);

        bool RequestCurrentStats();

        bool ResetAllStats(bool achievementsToo);

        bool SetAchievement(string name, bool state);

        bool SetStatValue(string name, int value);

        bool SetStatValue(string name, float value);

        bool StoreStats();

        void RequestGlobalAchievementPercentages();

        void RunCallbacks(bool server);

        interface IUserStatsReceived
        {
            ulong GameId { get; }

            int Result { get; }
        }

        bool GetCloudArchiveQuota(out ulong totalBytes, out ulong availableBytes);

        List<SteamRemoteFile>? GetCloudArchiveFiles();

        int FileRead(string name, byte[] buffer);

        bool FileWrite(string name, byte[] buffer);

        bool FileForget(string name);

        bool FileDelete(string name);
    }
}