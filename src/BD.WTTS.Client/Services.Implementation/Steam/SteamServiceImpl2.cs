#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using System.Globalization;
using AppResources = BD.WTTS.Client.Resources.Strings;
using ISteamConnectService = BD.SteamClient.Services.Mvvm.ISteamConnectService;
using SteamServiceBaseImpl = BD.SteamClient.Services.Implementation.SteamServiceImpl;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class SteamServiceImpl2 : SteamServiceBaseImpl, ISteamConnectService, ISteamService
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

    protected override string? ConfiguredSteamProgramPath => SteamSettings.SteamProgramPath.Value;

    protected override string? StratSteamDefaultParameter => SteamSettings.SteamStratParameter.Value;

    protected override bool IsRunSteamAdministrator => SteamSettings.IsRunSteamAdministrator.Value;

    protected override Dictionary<uint, string?>? HideGameList => serviceProvider.GetService<IPartialGameLibrarySettings>()?.HideGameList;

    Task<bool> ISteamService.SaveAppInfosToSteam()
    {
        var steamDirectory = SteamDirPath;
        if (steamDirectory == null)
            return Task.FromResult(false);

        bool CanWrite() => !IsRunningSteamProcess && string.Equals(steamDirectory, SteamDirPath, StringComparison.Ordinal);
        return Task.Run(async () =>
        {
            if (!CanWrite()) return false;
            var edits = GetAppInfoEdits();
            if (edits.Count == 0) return false;
            if (RewriteAppInfoCache(steamDirectory, edits, CanWrite)) return true;
            if (!CanWrite()) return false;

            // The writer also returns false when there are no bytes left to replace.
            // Verify the on-disk edits, including hidden games and resets, before reporting success.
            var persisted = await GetAppInfos(isSaveProperties: true).ConfigureAwait(false);
            return CanWrite() && AreAppEditsAlreadyApplied(edits, persisted);
        });
    }

    internal static bool AreAppEditsAlreadyApplied(IReadOnlyDictionary<uint, SteamAppEdit> edits, IEnumerable<SteamApp> apps)
    {
        if (edits.Count == 0) return false;
        var matched = new HashSet<uint>();
        foreach (var app in apps)
        {
            if (!edits.TryGetValue(app.AppId, out var edit)) continue;
            // On a freshly parsed row, IsEdited reflects the edit marker in Steam's cache.
            var shouldBeEdited = !edit.IsEmpty;
            var appInfo = app.ChangesData?.GetPropertyValue<SteamAppPropertyTable>("appinfo");
            if (appInfo == null || app.IsEdited != shouldBeEdited) return false;
            var stored = new SteamAppEdit
            {
                Name = appInfo.GetPropertyValue<string?>(null, "common", "name"),
                SortAs = appInfo.GetPropertyValue<string?>(null, "common", "sortas"),
                Developer = appInfo.GetPropertyValue<string?>(null, "extended", "developer"),
                Publisher = appInfo.GetPropertyValue<string?>(null, "extended", "publisher"),
            };
            if (!stored.WithOverrides(edit with { LaunchItems = null }).Diff(stored).IsEmpty ||
                (edit.LaunchItems is { } launchItems && !AreLaunchItemsAlreadyApplied(appInfo, launchItems)))
                return false;
            matched.Add(app.AppId);
        }
        return matched.Count == edits.Count;
    }

    static bool AreLaunchItemsAlreadyApplied(SteamAppPropertyTable appInfo, SteamLaunchEdit[] launchItems)
    {
        var stored = appInfo.GetPropertyValue<SteamAppPropertyTable?>(null, "config", "launch");
        if (stored == null || stored.Count != launchItems.Length) return false;

        static string? Optional(string? value) => string.IsNullOrEmpty(value) ? null : value;
        for (var i = 0; i < launchItems.Length; i++)
        {
            var item = stored.GetPropertyValue<SteamAppPropertyTable>(i.ToString(CultureInfo.InvariantCulture));
            var edit = launchItems[i];
            if (item == null || item.GetPropertyValue<string>("executable") != (edit.Executable ?? string.Empty) ||
                item.GetPropertyValue<string>("description") != Optional(edit.Label) ||
                item.GetPropertyValue<string>("arguments") != Optional(edit.Arguments) ||
                item.GetPropertyValue<string>("workingdir") != Optional(edit.WorkingDir) ||
                item.GetPropertyValue<string?>(null, "config", "oslist") != Optional(edit.Platform))
                return false;
        }
        return true;
    }

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