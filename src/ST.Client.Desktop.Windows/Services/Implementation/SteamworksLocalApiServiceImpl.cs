using SAM.API;
using SAM.API.Types;
using System.Application.Models;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static System.Application.Services.ISteamworksLocalApiService;
using UserStatsReceivedCallback = SAM.API.Callbacks.UserStatsReceived;

namespace System.Application.Services.Implementation
{
    internal sealed class SteamworksLocalApiServiceImpl : ISteamworksLocalApiService
    {
        public Client SteamClient { get; }

        UserStatsReceivedCallback? UserStatsReceivedCallback;

        public SteamworksLocalApiServiceImpl()
        {
            SteamClient = new Client();
        }

        public bool IsSupported => RuntimeInformation.ProcessArchitecture == (Architecture.X86 | Architecture.X64);

        public void DisposeSteamClient()
        {
            SteamClient.Dispose();
        }

        public bool Initialize()
        {
            try
            {
                SteamClient.Initialize(0);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "Initialize Fail.");
                return false;
            }
            return true;
        }

        public bool Initialize(int appid)
        {
            try
            {
                SteamClient.Initialize(appid);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "Initialize Fail.");
                return false;
            }
            return true;
        }

        public long GetSteamId64()
        {
            if (SteamClient.SteamUser == null)
                return 0L;
            return (long)SteamClient.SteamUser.GetSteamId();
        }

        public bool OwnsApps(uint appid)
        {
            if (SteamClient.SteamApps008 == null)
                return false;
            return SteamClient.SteamApps008.IsSubscribedApp(appid);
        }

        public IEnumerable<SteamApp> OwnsApps(IEnumerable<SteamApp> apps)
        {
            if (SteamClient.SteamApps008 == null || SteamClient.SteamApps001 == null)
                return new List<SteamApp>();
            return apps.Where(f => SteamClient.SteamApps008.IsSubscribedApp(f.AppId))
                .OrderBy(s => s.Name).Select((s) =>
                {
                    ////Index = i + 1,
                    //AppId = s.AppId,
                    //Name = s.Name,
                    //Type = Enum.TryParse<SteamAppType>(GetAppData(s.AppId, "type"), true, out var result) ? result : SteamAppType.Unknown,
                    ////Icon = GetAppData(s.AppId, "icon"),
                    ////Logo = GetAppData(s.AppId, "logo"),
                    //InstalledDir = GetAppInstallDir(s.AppId),
                    //IsInstalled = IsAppInstalled(s.AppId)

                    s.IsInstalled = IsAppInstalled(s.AppId);
                    s.Type = Enum.TryParse<SteamAppType>(GetAppData(s.AppId, "type"), true, out var result) ? result : SteamAppType.Unknown;
                    s.InstalledDir = GetAppInstallDir(s.AppId);
                    return s;
                });
        }

        public string GetAppData(uint appid, string key)
        {
            return SteamClient.SteamApps001.GetAppData(appid, key);
        }

        public bool IsSteamChinaLauncher()
        {
            return SteamClient.SteamUtils.IsSteamChinaLauncher();
        }

        public bool IsAppInstalled(uint appid)
        {
            return SteamClient.SteamApps008.IsAppInstalled(appid);
        }

        public string GetAppInstallDir(uint appid)
        {
            return SteamClient.SteamApps008.GetAppInstallDir(appid);
        }

        public string GetIPCountry()
        {
            return SteamClient.SteamUtils.GetIPCountry();
        }

        public string GetCurrentGameLanguage()
        {
            return SteamClient.SteamApps008.GetCurrentGameLanguage();
        }

        public string GetAvailableGameLanguages()
        {
            return SteamClient.SteamApps008.GetAvailableGameLanguages();
        }

        public bool GetStatValue(string name, out int value)
        {
            return SteamClient.SteamUserStats.GetStatValue(name, out value);
        }

        public bool GetStatValue(string name, out float value)
        {
            return SteamClient.SteamUserStats.GetStatValue(name, out value);
        }

        public bool GetAchievementState(string name, out bool isAchieved)
        {
            return SteamClient.SteamUserStats.GetAchievementState(name, out isAchieved);
        }

        public bool GetAchievementAndUnlockTime(string name, out bool isAchieved, out long unlockTime)
        {
            return SteamClient.SteamUserStats.GetAchievementAndUnlockTime(name, out isAchieved, out unlockTime);
        }

        public bool GetAchievementAchievedPercent(string name, out float percent)
        {
            return SteamClient.SteamUserStats.GetAchievementAchievedPercent(name, out percent);
        }

        public void AddUserStatsReceivedCallback(Action<IUserStatsReceived> action)
        {
            UserStatsReceivedCallback = SteamClient.CreateAndRegisterCallback<UserStatsReceivedCallback>();
            UserStatsReceivedCallback.OnRun += value =>
            {
                var valueWrapper = new UserStatsReceivedWrapper(value);
                action(valueWrapper);
            };
        }

        sealed class UserStatsReceivedWrapper : IUserStatsReceived
        {
            readonly UserStatsReceived userStatsReceived;

            public UserStatsReceivedWrapper(UserStatsReceived userStatsReceived)
            {
                this.userStatsReceived = userStatsReceived;
            }

            public ulong GameId => userStatsReceived.GameId;

            public int Result => userStatsReceived.Result;
        }

        public bool RequestCurrentStats()
        {
            return SteamClient.SteamUserStats.RequestCurrentStats();
        }

        public bool ResetAllStats(bool achievementsToo)
        {
            return SteamClient.SteamUserStats.ResetAllStats(achievementsToo);
        }

        public bool SetAchievement(string name, bool state)
        {
            return SteamClient.SteamUserStats.SetAchievement(name, state);
        }

        public bool SetStatValue(string name, int value)
        {
            return SteamClient.SteamUserStats.SetStatValue(name, value);
        }

        public bool SetStatValue(string name, float value)
        {
            return SteamClient.SteamUserStats.SetStatValue(name, value);
        }

        public bool StoreStats()
        {
            return SteamClient.SteamUserStats.StoreStats();
        }

        public void RequestGlobalAchievementPercentages()
        {
            SteamClient.SteamUserStats.RequestGlobalAchievementPercentages();
        }

        public void RunCallbacks(bool server)
        {
            SteamClient.RunCallbacks(server);
        }
    }
}