using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAM.API;
using SAM.API.Callbacks;
using SteamTool.Core.Common;
using SteamTool.Model;

namespace SteamTool.Steam.Service.Local
{
    public class SteamworksApiService
    {
        public Client SteamClient { get; } = new Client();
        private UserStatsReceived UserStatsReceivedCallback;
        public bool Initialize()
        {
            try
            {
                SteamClient.Initialize(0);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex);
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
                Logger.Warn(ex);
                return false;
            }
            return true;
        }

        public long GetSteamId64()
        {
            if (SteamClient.SteamUser == null)
                return 0;
            return (long)SteamClient.SteamUser.GetSteamId();
        }

        public bool OwnsApps(uint appid)
        {
            if (SteamClient.SteamApps008 == null)
                return false;
            return SteamClient.SteamApps008.IsSubscribedApp(appid);
        }

        public List<SteamApp> OwnsApps(List<SteamApp> apps)
        {
            if (SteamClient.SteamApps008 == null || SteamClient.SteamApps001 == null)
                return null;
            return apps.FindAll(f => SteamClient.SteamApps008.IsSubscribedApp(f.AppId))
                .OrderBy(s => s.Name).Select((s, i) => new SteamApp
                {
                    Index = i + 1,
                    AppId = s.AppId,
                    Name = s.Name,
                    Type = Enum.TryParse<SteamAppTypeEnum>(GetAppData(s.AppId, "type"), true, out var result) ? result : SteamAppTypeEnum.Unknown,
                    Icon = GetAppData(s.AppId, "icon"),
                    Logo = GetAppData(s.AppId, "logo"),
                    InstalledDir = GetAppInstallDir(s.AppId),
                    IsInstalled = IsAppInstalled(s.AppId)
                }).ToList();
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

        public void AddUserStatsReceivedCallback(Callback<SAM.API.Types.UserStatsReceived>.CallbackFunction action)
        {
            UserStatsReceivedCallback = SteamClient.CreateAndRegisterCallback<UserStatsReceived>();
            UserStatsReceivedCallback.OnRun += action;
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
