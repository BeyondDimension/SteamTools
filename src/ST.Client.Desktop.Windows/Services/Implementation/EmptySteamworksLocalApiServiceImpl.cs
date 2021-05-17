using System.Application.Models;
using System.Collections.Generic;

namespace System.Application.Services.Implementation
{
    internal sealed class EmptySteamworksLocalApiServiceImpl : ISteamworksLocalApiService
    {
        public bool IsSupported => false;

        public void AddUserStatsReceivedCallback(Action<ISteamworksLocalApiService.IUserStatsReceived> action)
        {
            throw new NotImplementedException();
        }

        public void DisposeSteamClient()
        {
            throw new NotImplementedException();
        }

        public bool GetAchievementAchievedPercent(string name, out float percent)
        {
            throw new NotImplementedException();
        }

        public bool GetAchievementAndUnlockTime(string name, out bool isAchieved, out long unlockTime)
        {
            throw new NotImplementedException();
        }

        public bool GetAchievementState(string name, out bool isAchieved)
        {
            throw new NotImplementedException();
        }

        public string GetAppData(uint appid, string key)
        {
            throw new NotImplementedException();
        }

        public string GetAppInstallDir(uint appid)
        {
            throw new NotImplementedException();
        }

        public string GetAvailableGameLanguages()
        {
            throw new NotImplementedException();
        }

        public string GetCurrentGameLanguage()
        {
            throw new NotImplementedException();
        }

        public string GetIPCountry()
        {
            throw new NotImplementedException();
        }

        public bool GetStatValue(string name, out int value)
        {
            throw new NotImplementedException();
        }

        public bool GetStatValue(string name, out float value)
        {
            throw new NotImplementedException();
        }

        public long GetSteamId64()
        {
            throw new NotImplementedException();
        }

        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public bool Initialize(int appid)
        {
            throw new NotImplementedException();
        }

        public bool IsAppInstalled(uint appid)
        {
            throw new NotImplementedException();
        }

        public bool IsSteamChinaLauncher()
        {
            throw new NotImplementedException();
        }

        public bool OwnsApps(uint appid)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SteamApp> OwnsApps(IEnumerable<SteamApp> apps)
        {
            throw new NotImplementedException();
        }

        public bool RequestCurrentStats()
        {
            throw new NotImplementedException();
        }

        public void RequestGlobalAchievementPercentages()
        {
            throw new NotImplementedException();
        }

        public bool ResetAllStats(bool achievementsToo)
        {
            throw new NotImplementedException();
        }

        public void RunCallbacks(bool server)
        {
            throw new NotImplementedException();
        }

        public bool SetAchievement(string name, bool state)
        {
            throw new NotImplementedException();
        }

        public bool SetStatValue(string name, int value)
        {
            throw new NotImplementedException();
        }

        public bool SetStatValue(string name, float value)
        {
            throw new NotImplementedException();
        }

        public bool StoreStats()
        {
            throw new NotImplementedException();
        }
    }
}