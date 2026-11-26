using BD.SteamClient.Helpers;
using ValveKeyValue;

namespace BD.WTTS.Services;

sealed record ShaderCacheScanEntry(ShaderCacheLocation Location, ShaderCacheStatistics Statistics);

sealed class ShaderCacheDiscovery(ShaderCacheFileStore files)
{
    const string Tag = nameof(ShaderCacheDiscovery);

    // Applications and overlays known to populate or use driver caches while running.
    static readonly HashSet<string> graphicsProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "NVIDIA Share", "NVIDIA Overlay", "NVIDIA App", "RadeonSoftware", "AMDRSServ",
        "RTSS", "obs", "obs32", "obs64", "blender", "gamescope",
    };

    public async Task<(ShaderCacheScanEntry[] Caches, string[] SteamLibraries)> ScanAsync(CancellationToken token)
    {
        var steam = Ioc.Get_Nullable<ISteamService>();
        var steamDirectory = steam?.SteamDirPath;
        var installations = new HashSet<string>(ShaderCacheFileStore.PathComparer);
        foreach (var candidate in ShaderCacheLocations.GetSteamInstallations(steamDirectory))
        {
            token.ThrowIfCancellationRequested();
            try
            {
                if (!Directory.Exists(candidate))
                    continue;
                // Steam's standard Linux aliases may point to the real installation directory.
                var installation = Directory.ResolveLinkTarget(candidate, returnFinalTarget: true)?.FullName ?? candidate;
                installations.Add(ShaderCacheFileStore.CanonicalPath(installation));
            }
            catch (Exception ex)
            {
                Log.Error(Tag, ex, "Could not resolve Steam installation {0}.", candidate);
            }
        }

        var libraries = new HashSet<string>(ShaderCacheFileStore.PathComparer);
        foreach (var installation in installations)
        {
            token.ThrowIfCancellationRequested();
            AddLibrary(installation);
            var manifest = Path.Combine(installation, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(manifest))
                manifest = Path.Combine(installation, "config", "libraryfolders.vdf");
            if (!File.Exists(manifest))
                continue;
            try
            {
                // ISteamService uses the same VDF reader, but its library path enumeration is private.
                // The library file also lists libraries with shader caches left by uninstalled games.
                foreach (var item in VdfHelper.Read(manifest).Children)
                {
                    token.ThrowIfCancellationRequested();
                    if (!uint.TryParse(item.Name, NumberStyles.None, CultureInfo.InvariantCulture, out _))
                        continue;
                    if (item.Value is KVCollectionValue library)
                    {
                        if (library["mounted"]?.ToString() != "0")
                            AddLibrary(library["path"]?.ToString());
                    }
                    else
                    {
                        AddLibrary(item.Value?.ToString());
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(Tag, ex, "Could not read Steam libraries from {0}.", manifest);
            }
        }

        var results = ShaderCacheLocations.FindDriverCaches(files, token)
            .Concat(ShaderCacheLocations.FindSteamCaches(files, installations, libraries, token))
            .Concat(ShaderCacheLocations.FindGameCaches(installations, token))
            .DistinctBy(static location => location.Path, ShaderCacheFileStore.PathComparer)
            .Select(location => new ShaderCacheScanEntry(location, files.Scan(location, token)))
            .Where(static entry => !entry.Statistics.IsKnownEmpty)
            .ToArray();
        var unresolved = results.Select(static entry => entry.Location)
            .Where(static location => location.AppId.HasValue)
            .Select(static location => location.AppId!.Value).ToHashSet();
        var names = new Dictionary<uint, string>();
        if (steam != null && steamDirectory != null && unresolved.Count > 0)
        {
            try
            {
                foreach (var app in steam.GetDownloadingAppList())
                {
                    token.ThrowIfCancellationRequested();
                    RememberName(app);
                }
                if (unresolved.Count > 0)
                    await steam.ReadAppInfosInBatchesAsync(batch =>
                    {
                        foreach (var app in batch)
                            RememberName(app);
                    }, token).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(Tag, ex, "Could not resolve Steam shader cache app names.");
            }
        }

        for (var i = 0; i < results.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            var entry = results[i];
            if (entry.Location.AppId is not { } appId)
                continue;
            if (!names.TryGetValue(appId, out var name))
                name = ReadInstalledName(entry.Location, appId);
            if (!string.IsNullOrWhiteSpace(name))
                results[i] = entry with { Location = entry.Location with { Name = name } };
        }
        return (results, libraries.ToArray());

        void AddLibrary(string? library)
        {
            if (string.IsNullOrWhiteSpace(library) || !Path.IsPathFullyQualified(library))
                return;
            try
            {
                var steamApps = ShaderCacheFileStore.CanonicalPath(Path.Combine(library, "steamapps"));
                if (Directory.Exists(steamApps))
                    libraries.Add(steamApps);
            }
            catch (Exception ex)
            {
                Log.Error(Tag, ex, "Could not resolve Steam library {0}.", library);
            }
        }

        void RememberName(SteamApp app)
        {
            if (!string.IsNullOrWhiteSpace(app.Name) && unresolved.Remove(app.AppId))
                names.Add(app.AppId, app.Name);
        }
    }

    static string? ReadInstalledName(ShaderCacheLocation location, uint appId)
    {
        var manifest = Path.Combine(location.SteamApps!, $"appmanifest_{appId.ToString(CultureInfo.InvariantCulture)}.acf");
        try
        {
            return File.Exists(manifest) ? VdfHelper.Read(manifest)["name"]?.ToString() : null;
        }
        catch (Exception ex)
        {
            Log.Error(Tag, ex, "Could not read Steam app manifest {0}.", manifest);
            return null;
        }
    }

    public static (string[] Steam, string[] Driver, string[] GameClients) FindRunningProcesses(
        IReadOnlyCollection<ShaderCacheLocation> selected, IReadOnlyCollection<string> libraries)
    {
        var includesDriver = selected.Any(static location => location.Kind == ShaderCacheKind.Driver);
        var includesSteamTemporary = selected.Any(static location => location.Kind == ShaderCacheKind.SteamTemporary);
        var selectedLibraries = (includesSteamTemporary
                ? libraries
                : selected.Select(static location => location.SteamApps).OfType<string>())
            .Select(static library => Path.Combine(library, "common"))
            .Distinct(ShaderCacheFileStore.PathComparer).ToArray();
        var allLibraries = includesDriver
            ? libraries.Select(static library => Path.Combine(library, "common")).ToArray()
            : [];
        var steam = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
        var driver = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
        var gameClients = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
        var selectedGameClients = ShaderCacheLocations.GetGameClientProcessNames(selected);
        var battleNetAgentRoot = selectedGameClients.Contains("Agent") ? ShaderCacheLocations.GetBattleNetAgentRoot() : null;
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try
                {
                    var name = process.ProcessName;
                    var isAgent = name.Equals("Agent", StringComparison.OrdinalIgnoreCase);
                    if (selectedGameClients.Contains(name) && !isAgent)
                        gameClients.Add(name);
                    if (includesDriver && graphicsProcesses.Contains(name))
                        driver.Add(name);
                    // Steam can write its global temporary cache while the client is open.
                    if (includesSteamTemporary && (name.Equals("steam", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("steam_osx", StringComparison.OrdinalIgnoreCase)))
                        steam.Add(name);
                    if (selectedLibraries.Length == 0 && allLibraries.Length == 0 && !(isAgent && battleNetAgentRoot != null))
                        continue;

                    var processPath = GetProcessPath(process);
                    if (isAgent && ShaderCacheLocations.IsBattleNetAgentPath(processPath, battleNetAgentRoot))
                        gameClients.Add("Battle.net Agent");
                    CheckProcessPath(processPath, name);
                    if (OperatingSystem.IsLinux())
                    {
                        // Proton may run a Wine executable with the game directory as working directory.
                        var cwd = Directory.ResolveLinkTarget($"/proc/{process.Id}/cwd", returnFinalTarget: true)?.FullName;
                        CheckProcessPath(cwd, name);
                    }
                }
                catch (Exception ex) when (ex is InvalidOperationException or Win32Exception or IOException
                    or UnauthorizedAccessException or NotSupportedException)
                {
                    // Processes can exit mid-read or belong to an account this process cannot inspect.
                }
            }
        }
        return (steam.ToArray(), driver.ToArray(), gameClients.ToArray());

        void CheckProcessPath(string? path, string name)
        {
            if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
                return;
            path = Path.GetFullPath(path);
            foreach (var common in selectedLibraries)
                if (ShaderCacheFileStore.IsWithin(path, common))
                    steam.Add(DescribeProcess(name, path, common));
            foreach (var common in allLibraries)
                if (ShaderCacheFileStore.IsWithin(path, common))
                    driver.Add(DescribeProcess(name, path, common));
        }
    }

    static string? GetProcessPath(Process process) => OperatingSystem.IsLinux()
        ? File.ResolveLinkTarget($"/proc/{process.Id}/exe", returnFinalTarget: true)?.FullName
        : process.MainModule?.FileName;

    static string DescribeProcess(string name, string path, string common)
    {
        var relative = path[(common.Length + 1)..];
        var separator = relative.IndexOf(Path.DirectorySeparatorChar);
        var folder = separator < 0 ? relative : relative[..separator];
        return $"{folder} ({name})";
    }
}
