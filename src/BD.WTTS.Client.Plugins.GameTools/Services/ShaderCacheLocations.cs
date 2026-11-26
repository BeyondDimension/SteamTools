using System.Globalization;

namespace BD.WTTS.Services;

enum ShaderCacheKind
{
    Driver,
    SteamGame,
    SteamTemporary,
    GameClient,
    Game,
}

sealed record ShaderCacheLocation(string Path, string Root, ShaderCacheKind Kind, string Name,
    string? SteamApps = null, uint? AppId = null, IReadOnlyList<string>? ProcessNames = null);

static class ShaderCacheLocations
{
    const string Windows = "Windows";
    const string Linux = "Linux";
    const string MacOS = "macOS";
    const string Home = "%SHADER_CACHE_HOME%";

    // A wildcard denotes immediate child directories of the named parent.
    static readonly (string Platform, string Name, string Path)[] driverLocations =
    [
        (Windows, "DirectX", @"%LOCALAPPDATA%\D3DSCache"),
        (Windows, "NVIDIA DXCache", @"%LOCALAPPDATA%\NVIDIA\DXCache"),
        // https://www.epicgames.com/help/c-34254770/c-37371353/a11302262
        (Windows, "NVIDIA DXCache", @"%LOCALAPPDATA%\NVIDIA\PerDriverVersion\DXCache"),
        // https://forums.developer.nvidia.com/t/dxcache-issues-file-path/283472
        (Windows, "NVIDIA DXCache", Home + @"\AppData\LocalLow\NVIDIA\PerDriverVersion\DXCache"),
        (Windows, "NVIDIA GLCache", @"%LOCALAPPDATA%\NVIDIA\GLCache"),
        (Windows, "NVIDIA OptiX", @"%LOCALAPPDATA%\NVIDIA\OptixCache"),
        (Windows, "NVIDIA NV_Cache", @"%PROGRAMDATA%\NVIDIA Corporation\NV_Cache"),
        (Windows, "NVIDIA NV_Cache", @"%LOCALAPPDATA%\NVIDIA Corporation\NV_Cache"),
        (Windows, "AMD DxCache", @"%LOCALAPPDATA%\AMD\DxCache"),
        (Windows, "AMD DxcCache", @"%LOCALAPPDATA%\AMD\DxcCache"),
        (Windows, "AMD GLCache", @"%LOCALAPPDATA%\AMD\GLCache"),
        (Windows, "AMD VkCache", @"%LOCALAPPDATA%\AMD\VkCache"),
        (Windows, "Intel ShaderCache", @"%LOCALAPPDATA%\Intel\ShaderCache"),
        (Linux, "Mesa", Home + "/.cache/mesa_shader_cache"),
        (Linux, "NVIDIA GLCache", Home + "/.cache/nvidia/GLCache"),
        (Linux, "NVIDIA GLCache", Home + "/.nv/GLCache"),
        (Linux, "AMD", Home + "/.cache/AMD/*"),
    ];

    static readonly (string Platform, string Path)[] steamLocations =
    [
        (Linux, Home + "/.local/share/Steam"),
        (Linux, Home + "/.steam/steam"),
        (Linux, Home + "/.var/app/com.valvesoftware.Steam/.local/share/Steam"),
        (Linux, Home + "/.var/app/com.valvesoftware.Steam/.steam/steam"),
        (MacOS, Home + "/Library/Application Support/Steam"),
    ];

    static readonly string[] steamProcesses = ["steam", "steam_osx", "steamwebhelper"];
    static readonly string[] epicProcesses = ["EpicGamesLauncher", "EpicWebHelper"];
    static readonly string[] blizzardProcesses = ["Battle.net", "Battle.net Launcher", "Battle.net Helper", "Agent", "Blizzard Update Agent"];
    static readonly string[] ubisoftProcesses = ["UbisoftConnect", "upc", "Uplay", "UplayWebCore", "UbisoftGameLauncher", "UbisoftGameLauncher64"];
    static readonly string[] robloxProcesses = ["Roblox", "RobloxPlayer", "RobloxPlayerBeta", "RobloxStudio", "RobloxStudioBeta"];
    static readonly string[] heroicProcesses = ["heroic", "Heroic Games Launcher"];
    static readonly string[] itchProcesses = ["itch", "itch-setup"];
    static readonly string[] wowProcesses = ["Wow", "Wow-ARM64", "WowClassic", "WowClassic-ARM64", "World of Warcraft", "World of Warcraft Classic", .. blizzardProcesses];

    // Only exact regenerated cache directories. Browser profiles, downloads and diagnostics can include user data.
    // Launcher path references: https://github.com/AdventDevInc/kudu/tree/4fb0dbdc51f65362b966d327c781dd2a874e0f00/rules
    // https://github.com/PatrickJnr/crapcleaner/blob/069ce952ff1532c7621c9de38ccf3bba7a0a37f9/crapcleaner/categories/gaming.py
    // https://www.epicgames.com/help/c-TechnicalSupport_PcSupport/a20673770
    // https://www.ubisoft.com/en-gb/help/connectivity-and-performance/article/emptying-the-cache-of-ubisoft-connect-pc/000061966?origin=search
    // https://support.activision.com/articles/clearing-cache-deleting-files
    // https://en.help.roblox.com/hc/en-us/articles/41932984382356-Roblox-Error-Failed-to-Load-Library
    static readonly GameCacheRule[] gameCacheRules =
    [
        new(Windows, "Steam", Environment.SpecialFolder.LocalApplicationData,
            ["Steam/htmlcache/Default/Cache", "Steam/htmlcache/Default/Code Cache", "Steam/htmlcache/Default/GPUCache",
             "Steam/htmlcache/GrShaderCache", "Steam/htmlcache/ShaderCache"], steamProcesses),
        new(Windows, "Epic Games Launcher", Environment.SpecialFolder.LocalApplicationData,
            [.. BrowserCacheLeaves("EpicGamesLauncher/Saved/webcache", "EpicGamesLauncher/Saved/webcache_4147", "EpicGamesLauncher/Saved/webcache_4430")], epicProcesses),
        new(MacOS, "Epic Games Launcher", Environment.SpecialFolder.UserProfile,
            [.. BrowserCacheLeaves("Library/Caches/com.epicgames.EpicGamesLauncher/webcache")], epicProcesses),
        new(Windows, "Ubisoft Connect", Environment.SpecialFolder.LocalApplicationData,
            ["Ubisoft Game Launcher/cache"], ubisoftProcesses),
        new(Windows, "Ubisoft Connect", Environment.SpecialFolder.ProgramFilesX86,
            ["Ubisoft/Ubisoft Game Launcher/cache"], ubisoftProcesses, InstallMarker: "Ubisoft/Ubisoft Game Launcher/UbisoftConnect.exe"),
        new(Windows, "EA app", Environment.SpecialFolder.LocalApplicationData,
            ["Electronic Arts/EA Desktop/Cache", "EADesktop/cache"], ["EADesktop", "EALauncher", "EACefSubProcess"]),
        new(Windows, "Battle.net", Environment.SpecialFolder.LocalApplicationData,
            ["Battle.net/Cache", "Blizzard Entertainment/Battle.net/Cache"], blizzardProcesses),
        new(Windows, "Battle.net", Environment.SpecialFolder.CommonApplicationData,
            ["Blizzard Entertainment/Battle.net/Cache"], blizzardProcesses),
        new(Windows, "GOG GALAXY", Environment.SpecialFolder.CommonApplicationData,
            [.. BrowserCacheLeaves("GOG.com/Galaxy/webcache", "GOG.com/Galaxy/webcache/common")],
            ["GalaxyClient", "GalaxyClient Helper"]),
        new(Windows, "itch.io", Environment.SpecialFolder.ApplicationData,
            ["itch/Cache/Cache_Data"], itchProcesses),
        new(Linux, "itch.io", Environment.SpecialFolder.ApplicationData,
            ["itch/Cache/Cache_Data"], itchProcesses),
        new(MacOS, "itch.io", Environment.SpecialFolder.UserProfile,
            ["Library/Application Support/itch/Cache/Cache_Data"], itchProcesses),
        new(Linux, "Heroic Games Launcher", Environment.SpecialFolder.ApplicationData,
            ["heroic/Cache/Cache_Data", "heroic/GPUCache"], heroicProcesses),
        new(MacOS, "Heroic Games Launcher", Environment.SpecialFolder.UserProfile,
            ["Library/Application Support/heroic/Cache/Cache_Data", "Library/Application Support/heroic/GPUCache"], heroicProcesses),
        new(Windows, "Roblox", Environment.SpecialFolder.LocalApplicationData,
            ["Roblox/Cache"], robloxProcesses, ShaderCacheKind.Game),
        new(MacOS, "Roblox", Environment.SpecialFolder.UserProfile,
            ["Library/Caches/com.roblox.RobloxPlayer"], robloxProcesses, ShaderCacheKind.Game),
    ];

    sealed record GameCacheRule(string Platform, string Name, Environment.SpecialFolder BaseFolder,
        string[] CachePaths, string[] ProcessNames, ShaderCacheKind Kind = ShaderCacheKind.GameClient, string? InstallMarker = null);

    static string? Platform => OperatingSystem.IsWindows() ? Windows
        : OperatingSystem.IsLinux() ? Linux
        : OperatingSystem.IsMacOS() ? MacOS
        : null;

    public static bool IsSupported => Platform != null;

    public static IEnumerable<ShaderCacheLocation> FindDriverCaches(ShaderCacheFileStore files, CancellationToken token)
    {
        foreach (var (platform, name, template) in driverLocations)
        {
            token.ThrowIfCancellationRequested();
            if (platform != Platform || Expand(template) is not { } path)
                continue;

            if (!template.EndsWith('*'))
            {
                if (Directory.Exists(path))
                    yield return new(path, path, ShaderCacheKind.Driver, name);
                continue;
            }

            var parent = Path.GetDirectoryName(path)!;
            foreach (var child in files.FindDirectories(parent, "*", token))
                yield return new(child, parent, ShaderCacheKind.Driver, $"{name} {Path.GetFileName(child)}");
        }
    }

    public static IEnumerable<string> GetSteamInstallations(string? configuredSteam)
    {
        if (!string.IsNullOrEmpty(configuredSteam))
            yield return configuredSteam;

        foreach (var (platform, template) in steamLocations)
            if (platform == Platform && Expand(template) is { } path)
                yield return path;
    }

    public static IEnumerable<ShaderCacheLocation> FindSteamCaches(ShaderCacheFileStore files,
        IEnumerable<string> installations, IEnumerable<string> librarySteamApps, CancellationToken token)
    {
        foreach (var steamApps in librarySteamApps)
        {
            var root = Path.Combine(steamApps, "shadercache");
            foreach (var path in files.FindDirectories(root, "*", token))
            {
                var name = Path.GetFileName(path);
                if (uint.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var appId) && appId != 0)
                    yield return new(path, root, ShaderCacheKind.SteamGame, name, steamApps, appId);
            }
        }

        foreach (var installation in installations)
            foreach (var path in files.FindDirectories(installation, "shader_cache_temp_dir*", token))
                yield return new(path, installation, ShaderCacheKind.SteamTemporary, "Steam");
    }

    public static IEnumerable<ShaderCacheLocation> FindGameCaches(IEnumerable<string> steamInstallations, CancellationToken token) =>
        FindGameCaches(Platform, static folder => Environment.GetFolderPath(folder, Environment.SpecialFolderOption.DoNotVerify),
            steamInstallations, token);

    internal static IEnumerable<ShaderCacheLocation> FindGameCaches(string? platform, Func<Environment.SpecialFolder, string> getFolder,
        IEnumerable<string> steamInstallations, CancellationToken token)
    {
        foreach (var rule in gameCacheRules)
        {
            token.ThrowIfCancellationRequested();
            if (rule.Platform != platform)
                continue;
            var root = getFolder(rule.BaseFolder);
            if (!Path.IsPathFullyQualified(root))
                continue;
            if (rule.InstallMarker is { } marker && !File.Exists(CombineKnownPath(root, marker)))
                continue;
            foreach (var knownPath in rule.CachePaths)
                if (ExistingCache(root, knownPath, rule.Kind, rule.Name, rule.ProcessNames) is { } location)
                    yield return location;
        }

        foreach (var root in steamInstallations)
        {
            token.ThrowIfCancellationRequested();
            if (ExistingCache(root, "appcache/httpcache", ShaderCacheKind.GameClient, "Steam", steamProcesses) is { } location)
                yield return location;
        }

        // Blizzard documents the version-specific Cache directory. Require an installed executable in the default game root.
        // https://us.support.blizzard.com/en/article/7549
        var wowRoot = platform switch
        {
            Windows => getFolder(Environment.SpecialFolder.ProgramFilesX86),
            MacOS => "/Applications",
            _ => string.Empty,
        };
        if (!Path.IsPathFullyQualified(wowRoot))
            yield break;
        foreach (var (edition, classic) in new[] { ("_retail_", false), ("_classic_", true), ("_classic_era_", true) })
        {
            token.ThrowIfCancellationRequested();
            var editionRoot = Path.Combine(wowRoot, "World of Warcraft", edition);
            var executable = platform == Windows
                ? (classic ? "WowClassic.exe" : "Wow.exe")
                : (classic ? "World of Warcraft Classic.app/Contents/MacOS/World of Warcraft Classic"
                    : "World of Warcraft.app/Contents/MacOS/World of Warcraft");
            var installed = File.Exists(CombineKnownPath(editionRoot, executable)) ||
                (platform == Windows && File.Exists(Path.Combine(editionRoot, classic ? "WowClassic-ARM64.exe" : "Wow-ARM64.exe")));
            if (installed &&
                ExistingCache(editionRoot, "Cache", ShaderCacheKind.Game, "World of Warcraft", wowProcesses) is { } location)
                yield return location;
        }
    }

    public static HashSet<string> GetGameClientProcessNames(IEnumerable<ShaderCacheLocation> selected) =>
        selected.Where(static location => location.Kind is ShaderCacheKind.GameClient or ShaderCacheKind.Game)
            .SelectMany(static location => location.ProcessNames ?? []).ToHashSet(StringComparer.OrdinalIgnoreCase);

    // A bare Agent process name is shared by unrelated applications.
    public static string? GetBattleNetAgentRoot() => Platform switch
    {
        Windows => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Battle.net", "Agent"),
        MacOS => "/Users/Shared/Battle.net/Agent",
        _ => null,
    };

    internal static bool IsBattleNetAgentPath(string? path, string? root) =>
        path != null && root != null && Path.IsPathFullyQualified(path) && Path.IsPathFullyQualified(root) &&
        Path.GetFileNameWithoutExtension(path).Equals("Agent", StringComparison.OrdinalIgnoreCase) &&
        ShaderCacheFileStore.IsWithin(Path.GetFullPath(path), Path.GetFullPath(root));

    static ShaderCacheLocation? ExistingCache(string root, string relativePath, ShaderCacheKind kind, string name, string[] processNames)
    {
        var path = CombineKnownPath(root, relativePath);
        return Directory.Exists(path) ? new(path, path, kind, name, ProcessNames: processNames) : null;
    }

    static string CombineKnownPath(string root, string relativePath) => Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));

    static IEnumerable<string> BrowserCacheLeaves(params string[] profiles)
    {
        foreach (var profile in profiles)
            foreach (var leaf in new[] { "Cache", "Code Cache", "GPUCache" })
            {
                yield return $"{profile}/{leaf}";
                yield return $"{profile}/Default/{leaf}";
            }
    }

    static string? Expand(string template)
    {
        if (template.StartsWith(Home, StringComparison.Ordinal))
        {
            var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrWhiteSpace(profile))
                return null;
            template = profile + template[Home.Length..];
        }
        return Environment.ExpandEnvironmentVariables(template);
    }
}
