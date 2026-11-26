using BD.SteamClient.Models;
using static BD.WTTS.GameList.Saves.GameSaveFileSystem;

namespace BD.WTTS.GameList.Saves;

public sealed partial record GameSaveEnvironment
{
    public OSPlatform Platform { get; init; }
    public string? UserProfile { get; init; }
    public string? ApplicationData { get; init; }
    public string? LocalApplicationData { get; init; }
    public string? LocalApplicationDataLow { get; init; }
    public string? MyDocuments { get; init; }
    public string? SavedGames { get; init; }
    public string? CommonDocuments { get; init; }
    public string? XdgDataHome { get; init; }
    public string? XdgConfigHome { get; init; }
    public string? SteamDirPath { get; init; }
    public long SteamId64 { get; init; }
    public uint SteamId32 { get; init; }

    public static GameSaveEnvironment FromSystem(string? steamDirPath, long steamId64 = 0, uint steamId32 = 0)
    {
        var windows = OperatingSystem.IsWindows();
        return new()
        {
            Platform = windows ? OSPlatform.Windows : OperatingSystem.IsMacOS() ? OSPlatform.OSX : OSPlatform.Linux,
            UserProfile = GetFolder(Environment.SpecialFolder.UserProfile),
            ApplicationData = GetFolder(Environment.SpecialFolder.ApplicationData),
            LocalApplicationData = GetFolder(Environment.SpecialFolder.LocalApplicationData),
            LocalApplicationDataLow = windows ? GetKnownFolder(new("A520A1A4-1780-4FF6-BD18-167343C5AF16")) : null,
            MyDocuments = GetFolder(Environment.SpecialFolder.MyDocuments),
            SavedGames = windows ? GetKnownFolder(new("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4")) : null,
            CommonDocuments = GetFolder(Environment.SpecialFolder.CommonDocuments),
            XdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME"),
            XdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME"),
            SteamDirPath = steamDirPath,
            SteamId64 = steamId64,
            SteamId32 = steamId32,
        };
    }

    static string GetFolder(Environment.SpecialFolder folder) =>
        Environment.GetFolderPath(folder, Environment.SpecialFolderOption.DoNotVerify);

    static string? GetKnownFolder(Guid id)
    {
        const uint dontVerify = 0x4000;
        var hr = SHGetKnownFolderPath(in id, dontVerify, IntPtr.Zero, out var path);
        try
        {
            return hr == 0 ? Marshal.PtrToStringUni(path) : null;
        }
        finally
        {
            Marshal.FreeCoTaskMem(path);
        }
    }

    [LibraryImport("shell32.dll")]
    private static partial int SHGetKnownFolderPath(in Guid id, uint flags, IntPtr token, out IntPtr path);
}

public sealed class GameSavePathResolver(GameSaveEnvironment environment)
{
    internal const uint ScreenshotStorageAppId = 760;

    public IReadOnlyList<GameSaveLocationInfo> ResolveLocations(SteamApp app)
        => ResolveLocations(app, app.InstalledDir);

    internal IReadOnlyList<GameSaveLocationInfo> ResolveLocations(SteamApp app, string? installedDirectory)
    {
        var results = new List<GameSaveLocationInfo>();
        var directories = new Dictionary<string, Dictionary<string, int>>(PathComparer);
        if (app.AppId != ScreenshotStorageAppId && !string.IsNullOrWhiteSpace(environment.SteamDirPath) && environment.SteamId32 > 0)
        {
            var remote = new GameSaveLocationInfo($"{app.AppId}:remote", app.AppId,
                GameSaveLocationKind.SteamUserdataRemote, "SteamUserData", null, "*", true, null);
            Add(Resolve(remote, () => ResolveUserdataRoot(app.AppId), ""));
        }

        if (app.SaveFiles is { } rules)
        {
            var index = 0;
            foreach (var rule in rules)
                Add(ResolveRule(app, rule, installedDirectory, index++));
        }
        return results;

        void Add(GameSaveLocationInfo location)
        {
            if (location.ResolvedDirectory is not { } directory)
            {
                results.Add(location);
                return;
            }

            if (!directories.TryGetValue(directory, out var rulesAtDirectory))
            {
                rulesAtDirectory = new(PathComparer);
                directories.Add(directory, rulesAtDirectory);
            }
            if (rulesAtDirectory.TryGetValue(location.Pattern, out var existingIndex))
            {
                if (location.Recursive && !results[existingIndex].Recursive)
                    results[existingIndex] = location;
                return;
            }
            rulesAtDirectory.Add(location.Pattern, results.Count);
            results.Add(location);
        }
    }

    internal GameScreenshotInfo? ResolveScreenshots(uint appId)
    {
        if (environment.SteamId32 == 0 || string.IsNullOrWhiteSpace(environment.SteamDirPath))
            return null;

        string? directory = null;
        try
        {
            var relative = Path.Combine(ScreenshotStorageRelativePath(environment.SteamId32), Number(appId), "screenshots");
            directory = ResolveRelative(CanonicalPath(environment.SteamDirPath), relative);
            var status = CheckPath(directory, directory: true, allowMissing: true)
                ? GameSaveLocationStatus.Empty : GameSaveLocationStatus.NotFound;
            return new(directory, status, 0, 0, null, []);
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            var status = directory == null ? GameSaveLocationStatus.Unresolved : GameSaveLocationStatus.AccessDenied;
            return new(directory ?? "", status, 0, 0, null, [], ex.Message);
        }
    }

    internal static string ScreenshotStorageRelativePath(uint steamId32) =>
        Path.Combine("userdata", Number(steamId32), Number(ScreenshotStorageAppId), "remote");

    public GameSaveLocationInfo ResolveRule(SteamApp app, SteamAppSaveFile rule, int index = 0)
        => ResolveRule(app, rule, app.InstalledDir, index);

    GameSaveLocationInfo ResolveRule(SteamApp app, SteamAppSaveFile rule, string? installedDirectory, int index)
    {
        var location = new GameSaveLocationInfo($"{app.AppId}:ufs:{index}", app.AppId,
            GameSaveLocationKind.UfsRule, rule.Root, rule.Path,
            string.IsNullOrWhiteSpace(rule.Pattern) ? "*" : rule.Pattern, rule.Recursive, null);
        try
        {
            if (!MatchesPlatform(rule.Platforms))
                return location with { ErrorMessage = "This Steam save rule is for another platform." };

            var root = rule.Root?.ToUpperInvariant();
            var relative = ExpandTokens(rule.Path ?? "");
            location = location with { Pattern = ExpandTokens(location.Pattern) };
            if (root is "STEAMUSERDATA" or "STEAMCLOUDDOCUMENTS")
            {
                relative = RemoveLeadingComponent(relative, Number(app.AppId));
                relative = RemoveLeadingComponent(relative, "remote");
            }
            return Resolve(location, () => GetRoot(root, app.AppId, installedDirectory), relative);
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            return location with { ErrorMessage = ex.Message };
        }
    }

    static GameSaveLocationInfo Resolve(GameSaveLocationInfo location, Func<string?> getRoot, string relative)
    {
        string? directory = null;
        try
        {
            var root = getRoot();
            if (string.IsNullOrWhiteSpace(root))
                return location with { ErrorMessage = "The Steam save root is unavailable on this platform or account." };
            if (!IsValidPattern(location.Pattern))
                return location with { ErrorMessage = "The Steam save pattern must be a filename pattern." };
            directory = ResolveRelative(CanonicalPath(root), relative, allowParents: true, allowRoot: true);
            var exists = CheckPath(directory, directory: true, allowMissing: true);
            return location with
            {
                ResolvedDirectory = directory,
                Status = exists ? GameSaveLocationStatus.Empty : GameSaveLocationStatus.NotFound,
            };
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            return location with
            {
                ResolvedDirectory = directory,
                Status = directory != null ? GameSaveLocationStatus.AccessDenied : GameSaveLocationStatus.Unresolved,
                ErrorMessage = ex.Message,
            };
        }
    }

    string? GetRoot(string? root, uint appId, string? installedDirectory) => root switch
    {
        "GAMEINSTALL" => installedDirectory,
        "STEAMINSTALL" => environment.SteamDirPath,
        "STEAMUSERDATA" or "STEAMCLOUDDOCUMENTS" => ResolveUserdataRoot(appId),
        _ when environment.Platform == OSPlatform.Windows => root switch
        {
            "WINAPPDATAROAMING" => environment.ApplicationData,
            "WINAPPDATALOCAL" => environment.LocalApplicationData,
            "WINAPPDATALOCALLOW" => environment.LocalApplicationDataLow ?? Home("AppData", "LocalLow"),
            "WINMYDOCUMENTS" => environment.MyDocuments,
            "WINSAVEDGAMES" => environment.SavedGames ?? Home("Saved Games"),
            "WINPUBLICDOCUMENTS" or "PUBLICDOCUMENTS" => environment.CommonDocuments,
            _ => null,
        },
        _ when environment.Platform == OSPlatform.Linux => root switch
        {
            "LINUXHOME" => environment.UserProfile,
            "LINUXXDGDATAHOME" => XdgRoot(environment.XdgDataHome) ?? Home(".local", "share"),
            "LINUXXDGCONFIGHOME" => XdgRoot(environment.XdgConfigHome) ?? Home(".config"),
            _ => null,
        },
        _ when environment.Platform == OSPlatform.OSX => root switch
        {
            "MACHOME" => environment.UserProfile,
            "MACAPPSUPPORT" => Home("Library", "Application Support"),
            "MACDOCUMENTS" => Home("Documents"),
            _ => null,
        },
        _ => null,
    };

    string? Home(params string[] parts) => string.IsNullOrWhiteSpace(environment.UserProfile)
        ? null
        : Path.Combine(environment.UserProfile, Path.Combine(parts));

    static string? XdgRoot(string? value) => value != null && Path.IsPathFullyQualified(value) ? value : null;

    string? ResolveUserdataRoot(uint appId) =>
        string.IsNullOrWhiteSpace(environment.SteamDirPath) || environment.SteamId32 == 0
            ? null
            : Path.Combine(CanonicalPath(environment.SteamDirPath), "userdata", Number(environment.SteamId32), Number(appId), "remote");

    string ExpandTokens(string value)
    {
        value = ReplaceToken(value, "{64BitSteamID}", environment.SteamId64);
        value = ReplaceToken(value, "{Steam3AccountID}", environment.SteamId32);
        return ReplaceToken(value, "{SteamID}", environment.SteamId64);
    }

    static string ReplaceToken(string value, string token, long id)
    {
        if (!value.Contains(token, StringComparison.OrdinalIgnoreCase))
            return value;
        if (id <= 0)
            throw new IOException($"No Steam account is available for {token}.");
        return value.Replace(token, id.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
    }

    static string RemoveLeadingComponent(string path, string component)
    {
        var separator = path.IndexOfAny(['/', '\\']);
        return path.AsSpan(0, separator < 0 ? path.Length : separator).Equals(component, StringComparison.OrdinalIgnoreCase)
            ? separator < 0 ? "" : path[(separator + 1)..]
            : path;
    }

    bool MatchesPlatform(string? platforms)
    {
        if (string.IsNullOrWhiteSpace(platforms))
            return true;
        var platform = environment.Platform == OSPlatform.OSX ? "macos" : environment.Platform.ToString();
        return platforms.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Any(value => string.Equals(value, platform, StringComparison.OrdinalIgnoreCase) ||
                (environment.Platform == OSPlatform.OSX && string.Equals(value, "osx", StringComparison.OrdinalIgnoreCase)));
    }

    internal static string Number(uint value) => value.ToString(CultureInfo.InvariantCulture);
}
