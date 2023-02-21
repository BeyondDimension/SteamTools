#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using SAM.API;
using SAM.API.Types;
using static BD.WTTS.Services.ISteamworksLocalApiService;
using SAMAPIClient = SAM.API.Client;
using UserStatsReceivedCallback = SAM.API.Callbacks.UserStatsReceived;
#endif

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class SteamworksLocalApiServiceImpl : ISteamworksLocalApiService
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public SAMAPIClient SteamClient { get; }

    UserStatsReceivedCallback? UserStatsReceivedCallback;

    public SteamworksLocalApiServiceImpl(IPlatformService platformService)
    {
        Steam.GetInstallPathDelegate = platformService.GetSteamDynamicLinkLibraryPath;
        SteamClient = new SAMAPIClient();
    }

    public bool IsSupported => RuntimeInformation.ProcessArchitecture.IsX86OrX64();

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
            Log.Error(TAG, ex, "Initialize fail.");
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
            Log.Error(TAG, ex, "Initialize fail.");
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
        if (!apps.Any_Nullable())
            return new List<SteamApp>();
        if (SteamClient.SteamApps008 == null || SteamClient.SteamApps001 == null)
            return apps;
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
                //s.IsInstalled = IsAppInstalled(s.AppId);
                //s.InstalledDir = GetAppInstallDir(s.AppId);
                s.State = IsAppInstalled(s.AppId) ? 4 : s.State;
                s.InstalledDir = string.IsNullOrEmpty(s.InstalledDir) ? GetAppInstallDir(s.AppId) : s.InstalledDir;
                s.Type = Enum.TryParse<SteamAppType>(GetAppData(s.AppId, "type"), true, out var result) ? result : SteamAppType.Unknown;
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

    public bool IsSteamInBigPictureMode()
    {
        return SteamClient.SteamUtils.IsSteamInBigPictureMode();
    }

    public uint GetSecondsSinceAppActive()
    {
        return SteamClient.SteamUtils.GetSecondsSinceAppActive();
    }

    public uint GetServerRealTime()
    {
        return SteamClient.SteamUtils.GetServerRealTime();
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

    #region SteamUserStats
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
    #endregion

    public void RunCallbacks(bool server)
    {
        SteamClient.RunCallbacks(server);
    }

    #region SteamRemoteStorage

    public bool GetCloudArchiveQuota(out ulong totalBytes, out ulong availableBytes)
    {
        return SteamClient.SteamRemoteStorage.GetQuota(out totalBytes, out availableBytes);
    }

    public List<SteamRemoteFile>? GetCloudArchiveFiles()
    {
        List<SteamRemoteFile> files = new();

        int fileCount = SteamClient.SteamRemoteStorage.GetFileCount();
        for (int i = 0; i < fileCount; ++i)
        {
            string name = SteamClient.SteamRemoteStorage.GetFileNameAndSize(i, out int length);
            var file = new SteamRemoteFile(name, length, SteamClient.SteamRemoteStorage.FileExists(name),
                SteamClient.SteamRemoteStorage.FilePersisted(name), SteamClient.SteamRemoteStorage.GetFileTimestamp(name))
            {
                SyncPlatforms = (SteamKit2.ERemoteStoragePlatform)SteamClient.SteamRemoteStorage.GetSyncPlatforms(name),
            };
            files.Add(file);
        }

        return files;
    }

    public int FileRead(string name, byte[] buffer)
    {
        return SteamClient.SteamRemoteStorage.FileRead(name, buffer);
    }

    public bool FileWrite(string name, byte[] buffer)
    {
        return SteamClient.SteamRemoteStorage.FileWrite(name, buffer);
    }

    public bool FileForget(string name)
    {
        return SteamClient.SteamRemoteStorage.FileForget(name);
    }

    public bool FileDelete(string name)
    {
        return SteamClient.SteamRemoteStorage.FileDelete(name);
    }

    #endregion

#endif
}