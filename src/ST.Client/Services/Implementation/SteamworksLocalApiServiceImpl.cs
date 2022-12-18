using System.Application.Models;
using System.Collections.Generic;
using static System.Application.Services.ISteamworksLocalApiService;

namespace System.Application.Services.Implementation
{
    internal sealed class SteamworksLocalApiServiceImpl : ISteamworksLocalApiService
    {
        public bool IsSupported => false;

        public void DisposeSteamClient() { }

        public bool Initialize() => default;

        public bool Initialize(int appid) => default;

        public long GetSteamId64() => default;

        public bool OwnsApps(uint appid) => default;

        public IEnumerable<SteamApp> OwnsApps(IEnumerable<SteamApp> apps) => apps;

        public string GetAppData(uint appid, string key) => string.Empty;

        public bool IsSteamChinaLauncher() => default;

        public bool IsSteamInBigPictureMode() => default;

        public uint GetSecondsSinceAppActive() => default;

        public uint GetServerRealTime() => default;

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

        #region SteamRemoteStorage

        public bool GetCloudArchiveQuota(out ulong totalBytes, out ulong availableBytes)
        {
            totalBytes = 0;
            availableBytes = 0;
            return false;
        }

        public List<SteamRemoteFile>? GetCloudArchiveFiles()
        {
            return null;
        }

        public int FileRead(string name, byte[] buffer)
        {
            return 0;
        }

        public bool FileWrite(string name, byte[] buffer)
        {
            return false;
        }

        public bool FileForget(string name)
        {
            return false;
        }

        public bool FileDelete(string name)
        {
            return false;
        }

        #endregion
    }
}