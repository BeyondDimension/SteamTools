#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using AppResources = BD.WTTS.Client.Resources.Strings;
using ISteamConnectService = BD.SteamClient.Services.Mvvm.ISteamConnectService;
using SteamServiceBaseImpl = BD.SteamClient.Services.Implementation.SteamServiceImpl;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class SteamServiceImpl2 : SteamServiceBaseImpl, ISteamConnectService
{
    readonly IPlatformService platform;
    readonly IServiceProvider serviceProvider;

    public SteamServiceImpl2(
        IServiceProvider serviceProvider,
        IPlatformService platform,
        ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        this.platform = platform;
        this.serviceProvider = serviceProvider;
    }

    bool ISteamConnectService.IsConnectToSteam
    {
        get => SteamConnectService.Current.IsConnectToSteam;
        set => SteamConnectService.Current.IsConnectToSteam = value;
    }

    string? ISteamConnectService.SteamLanguageString => ResourceService.GetCurrentCultureSteamLanguageName();

    SourceCache<SteamApp, uint> ISteamConnectService.SteamApps => SteamConnectService.Current.SteamApps;

    SourceCache<SteamApp, uint> ISteamConnectService.DownloadApps => SteamConnectService.Current.DownloadApps;

    SourceCache<SteamUser, long> ISteamConnectService.SteamUsers => SteamConnectService.Current.SteamUsers;

    SteamUser? ISteamConnectService.CurrentSteamUser => SteamConnectService.Current.CurrentSteamUser;

    public override ISteamConnectService Conn => this;

    protected override string? StratSteamDefaultParameter => SteamSettings.SteamStratParameter.Value;

    protected override bool IsRunSteamAdministrator => SteamSettings.IsRunSteamAdministrator.Value;

    protected override Dictionary<uint, string?>? HideGameList => serviceProvider.GetService<IPartialGameLibrarySettings>()?.HideGameList;

    protected override string? GetString(string name) => AppResources.ResourceManager.GetString(name);

    protected override Process? StartAsInvoker(string fileName, string? arguments = null)
    {
        return platform.StartAsInvoker(fileName, arguments);
    }

    public sealed override async ValueTask SetSteamCurrentUserAsync(string userName)
    {
#if WINDOWS
        if (DesktopBridge.IsRunningAsUwp)
        {
            string contents =
$"""
Windows Registry Editor Version 5.00
; {AssemblyInfo.Trademark} BD.WTTS.Services.Implementation.SteamServiceImpl2.SetSteamCurrentUserAsync
[HKEY_CURRENT_USER\Software\Valve\Steam]
"AutoLoginUser"="{userName}"
"RememberPassword"=dword:00000001
""";
            var path = IOPath.GetCacheFilePath(WindowsPlatformServiceImpl.CacheTempDirName, "SwitchSteamUser", FileEx.Reg);
            await WindowsPlatformServiceImpl.StartProcessRegeditAsync(path, contents);
            return;
        }
#endif
        await base.SetSteamCurrentUserAsync(userName);
    }

#if WINDOWS
    protected sealed override async ValueTask<bool> KillSteamProcess()
    {
        if (WindowsPlatformServiceImpl.IsPrivilegedProcess)
        {
            return await base.KillSteamProcess();
        }
        else
        {
            try
            {
                var processNames = steamProcess;
                if (processNames.Any_Nullable())
                {
                    var platformService = await IPlatformService.IPCRoot.Instance;
                    var r = platformService.KillProcesses(processNames);
                    return r ?? false;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "KillSteamProcess fail.");
                return false;
            }
        }
        return true;
    }
#endif
}
#endif