using BD.SteamClient.Enums;
using BD.SteamClient.Helpers;
using BD.SteamClient.Models;
using BD.SteamClient.Services;
using BD.WTTS.Services;
using ValveKeyValue;
using static BD.WTTS.GameList.Saves.GameSaveFileSystem;

namespace BD.WTTS.GameList.Saves;

public sealed class GameSaveScanner
{
    const long individualSteamIdBase = 76561197960265728;
    const int uninstalledBatchSize = 12;
    readonly GameSaveEnvironment? injectedEnvironment;
    readonly IReadOnlyList<SteamApp> injectedApps = [];
    readonly GameSaveSettingsStore? settings;
    readonly GameSaveBackupService? backups;

    public GameSaveScanner(GameSaveSettingsStore settings, GameSaveBackupService backups)
    {
        this.settings = settings;
        this.backups = backups;
    }

    public GameSaveScanner(GameSaveEnvironment environment) => injectedEnvironment = environment;

    internal GameSaveScanner(GameSaveEnvironment environment, IReadOnlyList<SteamApp> apps) : this(environment) =>
        injectedApps = apps;

    public Task<IReadOnlyList<GameSaveAccountInfo>> GetAccountsAsync(CancellationToken ct = default, bool refresh = false) =>
        Task.Run(() => GetAccounts(ct, refresh), ct);

    public IReadOnlyList<GameSaveAccountInfo> GetAccounts(CancellationToken ct = default, bool refresh = false)
    {
        ct.ThrowIfCancellationRequested();
        if (injectedEnvironment is { } environment)
        {
            return environment.SteamId32 == 0 ? [] :
                [CreateAccount(environment.SteamDirPath, environment.SteamId64, environment.SteamId32, null, true)];
        }

        var steam = ISteamService.Instance;
        var connection = SteamConnectService.Current;
        var cached = connection.SteamUsers.Items.ToArray();
        if (refresh)
        {
            try
            {
                var nicknames = cached.ToDictionary(user => user.SteamId64, GetNickname);
                connection.RefreshSteamUsers(requireReadSuccess: true);
                cached = connection.SteamUsers.Items.ToArray();
                // Refresh reloads loginusers.vdf and remarks. Keep nicknames already loaded for the same accounts.
                foreach (var user in cached)
                    if (GetNickname(user) == null && nicknames.GetValueOrDefault(user.SteamId64) is { } nickname)
                        user.SteamID = nickname;
            }
            catch (Exception ex) when (IsFileError(ex))
            {
                Log.Error(nameof(GameSaveScanner), ex, "Could not refresh Steam's saved accounts.");
            }
        }
        ct.ThrowIfCancellationRequested();
        return DiscoverAccounts(steam.SteamDirPath, connection.CurrentSteamUser, cached, steam.GetRememberUserList(), ct);
    }

    internal static IReadOnlyList<GameSaveAccountInfo> DiscoverAccounts(string? steamDirectory, SteamUser? current,
        IEnumerable<SteamUser> cached, IEnumerable<SteamUser> remembered, CancellationToken ct)
    {
        var accounts = new Dictionary<uint, (long SteamId64, string? Name, int Rank, string? Remark, bool IsCurrent)>();
        if (current != null)
            Add(current, true);
        foreach (var user in cached.Concat(remembered))
            Add(user, false);
        foreach (var id in FindNumericDirectories(steamDirectory, "userdata", ct))
            accounts.TryAdd(id, (individualSteamIdBase + id, null, 0, null, false));

        return accounts.Select(pair =>
        {
            ct.ThrowIfCancellationRequested();
            var (id64, name, _, remark, isCurrent) = pair.Value;
            name ??= ReadLocalPersonaName(steamDirectory, id64, pair.Key);
            return CreateAccount(steamDirectory, id64, pair.Key, name, isCurrent, remark);
        }).OrderByDescending(account => account.IsCurrent).ToArray();

        void Add(SteamUser user, bool isCurrent)
        {
            ct.ThrowIfCancellationRequested();
            var id = unchecked((uint)user.SteamId64);
            if (id == 0 || user.SteamId64 <= 0)
                return;

            var nickname = GetNickname(user);
            var persona = NormalizeName(user.PersonaName, user.SteamId64, id);
            var login = NormalizeName(user.AccountName, user.SteamId64, id);
            var rank = nickname != null ? 3 : persona != null ? 2 : login != null ? 1 : 0;
            var existing = accounts.GetValueOrDefault(id);
            var useNewName = rank > existing.Rank;
            var remark = string.IsNullOrWhiteSpace(user.Remark) ? null : user.Remark.Trim();
            accounts[id] = (user.SteamId64, useNewName ? nickname ?? persona ?? login : existing.Name,
                Math.Max(rank, existing.Rank), existing.Remark ?? remark, existing.IsCurrent || isCurrent);
        }
    }

    public Task<IReadOnlyList<GameSaveEntry>> ScanAsync(GameSaveAccountInfo? account = null,
        IProgress<GameSaveScanProgress>? progress = null, CancellationToken ct = default) =>
        Task.Run(async () =>
        {
            account ??= GetAccounts(ct).FirstOrDefault();
            var environment = GetEnvironment(account);
            var knownBackups = (await ListBackupsAsync(null, ct).ConfigureAwait(false))
                .ToLookup(x => x.AppId);
            var apps = await DiscoverGamesAsync(environment, knownBackups, ct).ConfigureAwait(false);
            return ScanGames(apps, environment, knownBackups,
                (processed, total, name, _) => progress?.Report(new(processed, total, name)), ct);
        }, ct);

    public Task<IReadOnlyList<GameSaveEntry>> ScanAsync(IEnumerable<SteamApp> apps,
        IProgress<GameSaveScanProgress>? progress = null, CancellationToken ct = default) =>
        Task.Run(() => ScanGames(apps.Select(x => new GameToScan(x, x.IsInstalled, x.InstalledDir)).ToArray(),
            GetEnvironment(injectedEnvironment == null ? GetAccounts(ct).FirstOrDefault() : null),
            Enumerable.Empty<GameSaveBackupSummary>().ToLookup(x => x.AppId),
            (processed, total, name, _) => progress?.Report(new(processed, total, name)), ct), ct);

    /// <summary>Publishes installed results once, then streams uninstalled results on a worker thread.</summary>
    /// <remarks>Cancelling the background token stops the uninstalled phase without retracting published results.</remarks>
    public Task ScanPhasedAsync(GameSaveAccountInfo? account,
        Action<IReadOnlyList<GameSaveEntry>> onInstalledCompleted,
        Action<IReadOnlyList<GameSaveEntry>> onUninstalledBatch,
        IProgress<GameSavePhasedScanProgress>? progress = null,
        CancellationToken installedCt = default,
        CancellationToken backgroundCt = default) =>
        Task.Run(async () =>
        {
            account ??= GetAccounts(installedCt).FirstOrDefault();
            var environment = GetEnvironment(account);
            var knownBackups = (await ListBackupsAsync(null, installedCt).ConfigureAwait(false)).ToLookup(x => x.AppId);
            var discovery = DiscoverKnownGames(environment);
            var installedGames = GetGamesToScan(discovery, installedOnly: true);
            GameToScan[]? allGames = null;
            if (injectedEnvironment == null && installedGames.Any(game =>
                !discovery.Games.TryGetValue(game.App.AppId, out var cached) || cached.SaveFiles == null && cached.BaseName == null))
            {
                // Load missing UFS metadata before publishing installed games. Otherwise this disk read can wait.
                allGames = await DiscoverGamesAsync(environment, knownBackups, installedCt, discovery).ConfigureAwait(false);
                installedGames = allGames.Where(game => game.IsInstalled).ToArray();
            }

            var installed = ScanGames(installedGames, environment, knownBackups,
                (processed, total, name, gamesWithData) => progress?.Report(
                    new(GameSaveScanPhase.Installed, processed, total, name, processed) { InstalledGamesWithDataCount = gamesWithData }),
                installedCt);
            installedCt.ThrowIfCancellationRequested();
            onInstalledCompleted(installed);

            try
            {
                backgroundCt.ThrowIfCancellationRequested();
                allGames ??= await DiscoverGamesAsync(environment, knownBackups, backgroundCt, discovery).ConfigureAwait(false);
                var uninstalled = allGames.Where(game => !game.IsInstalled).ToArray();
                ScanUninstalledGames(uninstalled, environment, knownBackups, installed.Count, onUninstalledBatch, progress, backgroundCt);
                backgroundCt.ThrowIfCancellationRequested();
                var total = installed.Count + uninstalled.Length;
                progress?.Report(new(GameSaveScanPhase.Completed, total, total, null, total));
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == backgroundCt && backgroundCt.IsCancellationRequested)
            {
                // Installed and any published uninstalled results remain available.
            }
        }, installedCt);

    public Task<GameSaveEntry> RescanGameAsync(SteamApp app, GameSaveAccountInfo? account = null, CancellationToken ct = default) =>
        Task.Run(async () =>
        {
            account ??= GetAccounts(ct).FirstOrDefault();
            var environment = GetEnvironment(account);
            var installed = GetInstalledApps(environment).FirstOrDefault(x => x.AppId == app.AppId);
            var game = new GameToScan(app, installed?.IsInstalled == true || app.IsInstalled,
                installed?.InstalledDir ?? app.InstalledDir);
            var knownBackups = await ListBackupsAsync(app.AppId, ct).ConfigureAwait(false);
            return ScanGame(game, new(environment), environment, knownBackups, ct);
        }, ct);

    GameDiscovery DiscoverKnownGames(GameSaveEnvironment environment)
    {
        var installed = Enumerable.DistinctBy(GetInstalledApps(environment).Where(app => app.AppId != 0),
            app => app.AppId).ToDictionary(app => app.AppId);
        var games = injectedEnvironment != null
            ? injectedApps.Where(app => app.AppId != 0).ToDictionary(app => app.AppId)
            : SteamConnectService.Current.SteamApps.Items.Where(app => app.AppId != 0 && IsGame(app))
                .ToDictionary(app => app.AppId);
        return new(installed, games);
    }

    async Task<GameToScan[]> DiscoverGamesAsync(GameSaveEnvironment environment,
        ILookup<uint, GameSaveBackupSummary> knownBackups, CancellationToken ct, GameDiscovery? discovery = null)
    {
        ct.ThrowIfCancellationRequested();
        discovery ??= DiscoverKnownGames(environment);
        var (installed, games) = discovery;

        var localIds = environment.SteamId32 > 0
            ? FindNumericDirectories(environment.SteamDirPath, Path.Combine("userdata",
                GameSavePathResolver.Number(environment.SteamId32)), ct)
            : new HashSet<uint>();
        localIds.Remove(GameSavePathResolver.ScreenshotStorageAppId);
        if (environment.SteamId32 > 0)
            localIds.UnionWith(FindNumericDirectories(environment.SteamDirPath,
                GameSavePathResolver.ScreenshotStorageRelativePath(environment.SteamId32), ct));
        var cachedGamesEmpty = games.Count == 0;
        if (injectedEnvironment == null && !string.IsNullOrWhiteSpace(environment.SteamDirPath) &&
            (cachedGamesEmpty || games.Values.Any(x => x.SaveFiles == null && x.BaseName == null) ||
                installed.Keys.Concat(localIds).Any(x => !games.ContainsKey(x))))
        {
            try
            {
                await ISteamService.Instance.ReadAppInfosInBatchesAsync(batch =>
                {
                    foreach (var app in batch)
                    {
                        ct.ThrowIfCancellationRequested();
                        if (games.TryGetValue(app.AppId, out var existing))
                        {
                            if (existing.SaveFiles == null && app.SaveFiles is { Count: > 0 })
                            {
                                // Keep the live name and installation data without mutating the shared app cache.
                                games[app.AppId] = new SteamApp(app.AppId)
                                {
                                    Name = existing.Name ?? app.Name,
                                    Type = existing.Type,
                                    State = existing.State,
                                    InstalledDir = existing.InstalledDir,
                                    SaveFiles = app.SaveFiles,
                                };
                            }
                        }
                        else if ((cachedGamesEmpty && IsGame(app)) ||
                            installed.ContainsKey(app.AppId) || localIds.Contains(app.AppId))
                        {
                            games.Add(app.AppId, app);
                        }
                    }
                }, ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (IsFileError(ex) || ex is InvalidDataException)
            {
                Log.Error(nameof(GameSaveScanner), ex, "Could not read Steam's local AppInfo cache.");
            }
        }

        foreach (var id in localIds)
        {
            if (!installed.ContainsKey(id))
                games.TryAdd(id, new SteamApp(id));
        }
        foreach (var group in knownBackups)
        {
            if (installed.ContainsKey(group.Key))
                continue;
            var app = new SteamApp(group.Key) { Name = group.First().GameName };
            if (!games.TryAdd(app.AppId, app) && string.IsNullOrEmpty(games[app.AppId].Name) &&
                games[app.AppId].SaveFiles == null)
                games[app.AppId] = app;
        }

        return GetGamesToScan(discovery);
    }

    static GameToScan[] GetGamesToScan(GameDiscovery discovery, bool installedOnly = false)
    {
        var (installed, games) = discovery;
        var apps = Enumerable.DistinctBy(games.Values.Concat(installed.Values), app => app.AppId);
        return apps.Select(app =>
        {
            installed.TryGetValue(app.AppId, out var localApp);
            return new GameToScan(app, localApp?.IsInstalled == true || app.IsInstalled,
                localApp?.InstalledDir ?? app.InstalledDir);
        }).Where(game => !installedOnly || game.IsInstalled)
            .OrderBy(game => game.App.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(game => game.App.AppId).ToArray();
    }

    IEnumerable<SteamApp> GetInstalledApps(GameSaveEnvironment environment) =>
        injectedEnvironment != null || string.IsNullOrWhiteSpace(environment.SteamDirPath)
            ? []
            : ISteamService.Instance.GetDownloadingAppList();

    async Task<IReadOnlyList<GameSaveBackupSummary>> ListBackupsAsync(uint? appId, CancellationToken ct)
    {
        if (settings == null || backups == null)
            return [];
        try
        {
            var preferences = await settings.LoadAsync(ct).ConfigureAwait(false);
            return await backups.ListBackupsAsync(preferences.BackupRootPath, appId, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (IsFileError(ex) || ex is System.Text.Json.JsonException)
        {
            Log.Error(nameof(GameSaveScanner), ex, "Could not load game save backup preferences.");
            return [];
        }
    }

    static IReadOnlyList<GameSaveEntry> ScanGames(GameToScan[] apps, GameSaveEnvironment environment,
        ILookup<uint, GameSaveBackupSummary> backups, Action<int, int, string?, int> report, CancellationToken ct)
    {
        report(0, apps.Length, null, 0);
        var resolver = new GameSavePathResolver(environment);
        var results = new List<GameSaveEntry>(apps.Length);
        var gamesWithData = 0;
        foreach (var app in apps)
        {
            ct.ThrowIfCancellationRequested();
            var entry = ScanGame(app, resolver, environment, backups[app.App.AppId].ToArray(), ct);
            results.Add(entry);
            if (entry.HasLocalSaves || entry.HasScreenshots)
                gamesWithData++;
            report(results.Count, apps.Length, entry.Name, gamesWithData);
        }
        return results;
    }

    static void ScanUninstalledGames(GameToScan[] apps, GameSaveEnvironment environment,
        ILookup<uint, GameSaveBackupSummary> backups, int installedCount, Action<IReadOnlyList<GameSaveEntry>> publish,
        IProgress<GameSavePhasedScanProgress>? progress, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        progress?.Report(new(GameSaveScanPhase.Uninstalled, 0, apps.Length, null, installedCount));
        var resolver = new GameSavePathResolver(environment);
        var batch = new List<GameSaveEntry>(uninstalledBatchSize);
        for (var index = 0; index < apps.Length; index++)
        {
            ct.ThrowIfCancellationRequested();
            var app = apps[index];
            var entry = ScanGame(app, resolver, environment, backups[app.App.AppId].ToArray(), ct);
            batch.Add(entry);
            var processed = index + 1;
            progress?.Report(new(GameSaveScanPhase.Uninstalled, processed, apps.Length, entry.Name, installedCount + processed));
            if (batch.Count == uninstalledBatchSize || entry.HasLocalSaves || entry.HasScreenshots ||
                entry.Backups.Count > 0 || index == apps.Length - 1)
            {
                ct.ThrowIfCancellationRequested();
                publish(batch.ToArray());
                batch.Clear();
            }
        }
    }

    static GameSaveEntry ScanGame(GameToScan game, GameSavePathResolver resolver, GameSaveEnvironment environment,
        IReadOnlyList<GameSaveBackupSummary> backups, CancellationToken ct)
    {
        var app = game.App;
        var locations = resolver.ResolveLocations(app, game.InstalledDirectory);
        var scanned = new List<GameSaveLocationInfo>(locations.Count);
        var uniqueFiles = new HashSet<string>(PathComparer);
        long totalBytes = 0;
        DateTime? newest = null;
        var hasRemote = false;
        foreach (var location in locations)
        {
            ct.ThrowIfCancellationRequested();
            if (location.Status != GameSaveLocationStatus.Empty || location.ResolvedDirectory is not { } root)
            {
                scanned.Add(location);
                continue;
            }

            var warnings = new List<string>();
            var files = CollectFiles(root, location.Pattern, location.Recursive, warnings, ct);
            long locationBytes = 0;
            DateTime? locationNewest = null;
            foreach (var file in files)
            {
                locationBytes += file.SizeBytes;
                if (locationNewest == null || file.LastWriteUtc > locationNewest)
                    locationNewest = file.LastWriteUtc;
                if (uniqueFiles.Add(file.Path))
                    totalBytes += file.SizeBytes;
            }
            if (locationNewest != null && (newest == null || locationNewest > newest))
                newest = locationNewest;
            hasRemote |= files.Count > 0 && location.Kind == GameSaveLocationKind.SteamUserdataRemote;
            scanned.Add(location with
            {
                Status = files.Count > 0 ? GameSaveLocationStatus.Found :
                    warnings.Count > 0 ? GameSaveLocationStatus.AccessDenied : GameSaveLocationStatus.Empty,
                TotalBytes = locationBytes,
                FileCount = files.Count,
                LastModifiedUtc = locationNewest,
                ErrorMessage = warnings.Count == 0 ? null : string.Join(Environment.NewLine, warnings),
                Files = files.Select(ToFileItem).ToArray(),
            });
        }
        return new(app, app.AppId, app.DisplayName, game.IsInstalled, scanned, totalBytes, uniqueFiles.Count,
            newest, uniqueFiles.Count > 0, hasRemote, backups)
        {
            SteamId32 = environment.SteamId32,
            SteamId64 = environment.SteamId64,
            Screenshots = ScanScreenshots(resolver, app.AppId, ct),
        };
    }

    static GameScreenshotInfo? ScanScreenshots(GameSavePathResolver resolver, uint appId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var screenshots = resolver.ResolveScreenshots(appId);
        if (screenshots is not { Status: GameSaveLocationStatus.Empty, DirectoryPath: var root })
            return screenshots;

        var warnings = new List<string>();
        var images = CollectFiles(root, "*", recursive: false, warnings, ct, IsScreenshotImage);
        // Steam lists only original top-level images. All thumbnail files contribute to storage usage.
        var thumbnails = new List<GameSaveFile>();
        try
        {
            var directory = ResolveRelative(root, "thumbnails");
            if (CheckPath(directory, directory: true, allowMissing: true))
                thumbnails = CollectFiles(directory, "*", recursive: true, warnings, ct);
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            warnings.Add(ex.Message);
        }

        var files = images.Concat(thumbnails);
        return screenshots with
        {
            Status = images.Count > 0 || thumbnails.Count > 0 ? GameSaveLocationStatus.Found :
                warnings.Count > 0 ? GameSaveLocationStatus.AccessDenied : GameSaveLocationStatus.Empty,
            TotalBytes = files.Sum(file => file.SizeBytes),
            ScreenshotCount = images.Count,
            LastModifiedUtc = files.Max(file => (DateTime?)file.LastWriteUtc),
            Files = images.Select(ToFileItem).ToArray(),
            ErrorMessage = warnings.Count == 0 ? null : string.Join(Environment.NewLine, warnings),
        };
    }

    static GameSaveFileItem ToFileItem(GameSaveFile file) =>
        new(Path.GetFileName(file.Path), file.RelativePath, file.Path, file.SizeBytes, file.LastWriteUtc);

    static bool IsScreenshotImage(string name) =>
        Path.GetExtension(name).ToLowerInvariant() is ".jpg" or ".jpeg" or ".png" or ".bmp" or ".webp";

    GameSaveEnvironment GetEnvironment(GameSaveAccountInfo? account)
    {
        if (injectedEnvironment is { } environment)
        {
            return account == null ? environment : environment with
            {
                SteamId64 = account.SteamId64,
                SteamId32 = account.SteamId32,
            };
        }
        return GameSaveEnvironment.FromSystem(ISteamService.Instance.SteamDirPath, account?.SteamId64 ?? 0, account?.SteamId32 ?? 0);
    }

    static string? ReadLocalPersonaName(string? steamDirectory, long steamId64, uint steamId32)
    {
        if (string.IsNullOrWhiteSpace(steamDirectory))
            return null;

        var id = GameSavePathResolver.Number(steamId32);
        try
        {
            var path = ResolveRelative(CanonicalPath(steamDirectory), Path.Combine("userdata", id, "config", "localconfig.vdf"));
            if (!CheckPath(path, directory: false, allowMissing: true))
                return null;

            var root = VdfHelper.Read(path);
            if (!string.Equals(root.Name, "UserLocalConfigStore", StringComparison.OrdinalIgnoreCase))
                return null;

            var friends = Child(root, "friends");
            return Name(friends, "PersonaName") ?? Name(Child(friends, id), "name") ?? Name(Child(root, "System"), "PersonaName");
        }
        catch (Exception ex) when (IsFileError(ex) || ex is InvalidDataException or KeyValueException)
        {
            Log.Error(nameof(GameSaveScanner), ex, "Could not read a Steam userdata account name.");
            return null;
        }

        static KVObject? Child(KVObject? parent, string key) =>
            parent?.Children.FirstOrDefault(node => string.Equals(node.Name, key, StringComparison.OrdinalIgnoreCase));

        string? Name(KVObject? parent, string key) =>
            NormalizeName(Child(parent, key)?.Value.ToString(), steamId64, steamId32);
    }

    static string? NormalizeName(string? name, long steamId64, uint steamId32)
    {
        name = name?.Trim();
        return string.IsNullOrEmpty(name) ||
            name == steamId64.ToString(CultureInfo.InvariantCulture) ||
            name == GameSavePathResolver.Number(steamId32) ? null : name;
    }

    static string? GetNickname(SteamUser user)
    {
        // Steam also copies loginusers.vdf's persona into SteamID when loading remembered users.
        var id = unchecked((uint)user.SteamId64);
        var nickname = NormalizeName(user.SteamNickName, user.SteamId64, id);
        var persona = NormalizeName(user.PersonaName, user.SteamId64, id);
        return string.Equals(nickname, persona, StringComparison.Ordinal) ? null : nickname;
    }

    static GameSaveAccountInfo CreateAccount(string? steamDirectory, long steamId64, uint steamId32,
        string? name, bool isCurrent, string? remark = null)
    {
        var id = GameSavePathResolver.Number(steamId32);
        var directory = string.IsNullOrWhiteSpace(steamDirectory) ? "" : Path.Combine(steamDirectory, "userdata", id);
        name ??= steamId64.ToString(CultureInfo.InvariantCulture);
        if (remark != null && !string.Equals(name, remark, StringComparison.CurrentCultureIgnoreCase))
            name = $"{name} ({remark})";
        return new(steamId64, steamId32, name, directory, isCurrent);
    }

    static HashSet<uint> FindNumericDirectories(string? steamDirectory, string subdirectory, CancellationToken ct)
    {
        var results = new HashSet<uint>();
        if (string.IsNullOrWhiteSpace(steamDirectory))
            return results;
        try
        {
            var path = ResolveRelative(CanonicalPath(steamDirectory), subdirectory);
            if (!CheckPath(path, directory: true, allowMissing: true))
                return results;
            foreach (var directory in Directory.EnumerateDirectories(path))
            {
                ct.ThrowIfCancellationRequested();
                if (uint.TryParse(Path.GetFileName(directory), NumberStyles.None, CultureInfo.InvariantCulture, out var id) &&
                    id != 0 && !File.GetAttributes(directory).HasFlag(FileAttributes.ReparsePoint))
                    results.Add(id);
            }
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            Log.Error(nameof(GameSaveScanner), ex, "Could not enumerate Steam userdata.");
        }
        return results;
    }

    static bool IsGame(SteamApp app) =>
        app.Type is SteamAppType.Game or SteamAppType.Demo or SteamAppType.Mod or SteamAppType.Beta ||
        app.SaveFiles is { Count: > 0 };

    sealed record GameToScan(SteamApp App, bool IsInstalled, string? InstalledDirectory);

    sealed record GameDiscovery(Dictionary<uint, SteamApp> Installed, Dictionary<uint, SteamApp> Games);
}
