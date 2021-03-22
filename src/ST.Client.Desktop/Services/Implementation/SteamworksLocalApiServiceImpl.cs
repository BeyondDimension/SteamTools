using System.Application.Models;
using System.Collections.Generic;
using static System.Application.Services.ISteamworksLocalApiService;

namespace System.Application.Services.Implementation
{
    internal sealed class SteamworksLocalApiServiceImpl : ISteamworksLocalApiService
    {
        public bool IsSupported => false;

        public bool Initialize() => default;

        public bool Initialize(int appid) => default;

        public long GetSteamId64() => default;

        public bool OwnsApps(uint appid) => default;

        public List<SteamApp> OwnsApps(List<SteamApp> apps) => new List<SteamApp>();

        public string GetAppData(uint appid, string key) => string.Empty;

        public bool IsSteamChinaLauncher() => default;

        public bool IsAppInstalled(uint appid) => default;

        public string GetAppInstallDir(uint appid) => string.Empty;

        public string GetIPCountry() => string.Empty;

        public string GetCurrentGameLanguage() => string.Empty;

        public string GetAvailableGameLanguages() => string.Empty;

        public bool GetStatValue(string name, out int value)
        {
            value = default;
            return default;
        }

        public bool GetStatValue(string name, out float value)
        {
            value = default;
            return default;
        }

        public bool GetAchievementState(string name, out bool isAchieved)
        {
            isAchieved = default;
            return default;
        }

        public bool GetAchievementAndUnlockTime(string name, out bool isAchieved, out long unlockTime)
        {
            isAchieved = default;
            unlockTime = default;
            return default;
        }

        public bool GetAchievementAchievedPercent(string name, out float percent)
        {
            percent = default;
            return default;
        }

        public void AddUserStatsReceivedCallback(Action<IUserStatsReceived> action)
        {
        }

        public bool RequestCurrentStats() => default;

        public bool ResetAllStats(bool achievementsToo) => default;

        public bool SetAchievement(string name, bool state) => default;

        public bool SetStatValue(string name, int value) => default;

        public bool SetStatValue(string name, float value) => default;

        public bool StoreStats() => default;

        public void RequestGlobalAchievementPercentages()
        {
        }

        public void RunCallbacks(bool server)
        {
        }
    }
}