using static BD.WTTS.GameList.Saves.GameSaveFileSystem;

namespace BD.WTTS.GameList.Saves;

public sealed class GameSaveBackupService
{
    const string manifestName = "manifest.json";
    const string stagingPrefix = ".game-save-";
    const string autoBackupDirectory = "_AutoPreRestore";
    readonly SemaphoreSlim operations = new(1, 1);

    public Task<GameSaveOperationResult> BackupGameAsync(GameSaveEntry game, string destinationRoot,
        string? note = null, bool isAutoPreRestore = false, CancellationToken ct = default) =>
        RunOperationAsync(async () =>
        {
            var source = new SnapshotSource(game.AppId, game.Name, game.SteamId32, game.SteamId64, game.Locations);
            return await CreateSnapshotAsync(source, destinationRoot, note, isAutoPreRestore,
                forRestore: false, ct).ConfigureAwait(false);
        }, ct);

    public async Task<IReadOnlyList<GameSaveOperationResult>> BackupGamesAsync(IEnumerable<GameSaveEntry> games,
        string destinationRoot, string? note = null, IProgress<GameSaveScanProgress>? progress = null,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var entries = games.ToArray();
        var results = new List<GameSaveOperationResult>(entries.Length);
        progress?.Report(new(0, entries.Length, null));
        foreach (var game in entries)
        {
            results.Add(await BackupGameAsync(game, destinationRoot, note, ct: ct).ConfigureAwait(false));
            progress?.Report(new(results.Count, entries.Length, game.Name));
        }
        return results;
    }

    public Task<IReadOnlyList<GameSaveBackupSummary>> ListBackupsAsync(string backupRoot, uint? filterAppId = null,
        CancellationToken ct = default) => Task.Run(async () =>
    {
        var root = CanonicalPath(backupRoot);
        var results = new List<GameSaveBackupSummary>();
        if (!CheckPath(root, directory: true, allowMissing: true))
            return (IReadOnlyList<GameSaveBackupSummary>)results;
        if (!await TryReadSnapshotAsync(root).ConfigureAwait(false))
        {
            foreach (var child in ReadChildDirectories(root, ct))
            {
                if (await TryReadSnapshotAsync(child).ConfigureAwait(false))
                    continue;
                foreach (var grandchild in ReadChildDirectories(child, ct))
                    await TryReadSnapshotAsync(grandchild).ConfigureAwait(false);
            }
        }
        return results.OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.BackupId, StringComparer.Ordinal).ToArray();

        async Task<bool> TryReadSnapshotAsync(string directory)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                if (!CheckPath(Path.Combine(directory, manifestName), directory: false, allowMissing: true))
                    return false;
                var manifest = await ReadManifestAsync(directory, ct).ConfigureAwait(false);
                if (filterAppId == null || manifest.AppId == filterAppId)
                    results.Add(ToSummary(manifest, directory));
            }
            catch (Exception ex) when (IsSnapshotError(ex))
            {
                Log.Error(nameof(GameSaveBackupService), ex, $"Could not read snapshot '{directory}'.");
            }
            return true;
        }
    }, ct);

    internal static Task<GameSaveBackupManifest?> ReadSnapshotManifestAsync(string snapshotDirectory) => Task.Run(async () =>
    {
        var directory = CanonicalPath(snapshotDirectory);
        return CheckPath(Path.Combine(directory, manifestName), directory: false, allowMissing: true)
            ? await ReadManifestAsync(directory, default).ConfigureAwait(false)
            : null;
    });

    public Task<GameSaveOperationResult> RestoreFromSnapshotAsync(string snapshotDirectory, GameSaveRestoreOptions options,
        CancellationToken ct = default) => RestoreFromSnapshotAsync(snapshotDirectory, options, approvedManifest: null, ct);

    internal Task<GameSaveOperationResult> RestoreFromSnapshotAsync(string snapshotDirectory, GameSaveRestoreOptions options,
        GameSaveBackupManifest? approvedManifest, CancellationToken ct = default) => RunOperationAsync(async () =>
    {
        var directory = CanonicalPath(snapshotDirectory);
        // The confirmation shows the destinations from this validated manifest. Use the same plan after approval.
        var manifest = approvedManifest ?? await ReadManifestAsync(directory, ct).ConfigureAwait(false);
        var plan = new Dictionary<string, RestoreFile>(PathComparer);
        var locations = new List<GameSaveLocationInfo>(manifest.Locations.Count);
        foreach (var location in manifest.Locations)
        {
            var original = CanonicalPath(location.OriginalDirectory);
            RejectOverlappingDirectories(directory, original);
            var archive = ResolveRelative(directory, location.RelativeArchiveDir);
            locations.Add(new(GameSavePathResolver.Number((uint)location.Index), manifest.AppId, location.Kind,
                location.RootName, null, location.Pattern, location.Recursive, original));
            foreach (var file in location.Files)
            {
                ct.ThrowIfCancellationRequested();
                var source = ResolveRelative(archive, file.RelativePath);
                var target = ResolveRelative(original, file.RelativePath);
                CheckPath(source, directory: false);
                if (new FileInfo(source).Length != file.SizeBytes)
                    throw new IOException($"The snapshot file size does not match its manifest at '{source}'.");
                CheckPath(target, directory: false, allowMissing: true);
                if (plan.TryGetValue(target, out var previous) &&
                    (previous.SizeBytes != file.SizeBytes || previous.LastWriteUtc != file.LastWriteUtc))
                    throw new IOException($"The snapshot contains inconsistent versions of '{target}'.");
                plan.TryAdd(target, new(source, target, file.SizeBytes, file.LastWriteUtc));
            }
        }
        ValidateRestoreTargets(plan.Keys);
        var saveSource = new SnapshotSource(manifest.AppId, manifest.GameName, manifest.SteamId32, manifest.SteamId64, locations);
        return await RestoreAsync(plan.Values, saveSource, directory, options, ct).ConfigureAwait(false);
    }, ct);

    public Task<GameSaveOperationResult> RestoreRawFolderToLocationAsync(string sourceFolder, GameSaveLocationInfo targetLocation,
        GameSaveRestoreOptions options, CancellationToken ct = default) =>
        RestoreRawFolderToLocationAsync(sourceFolder, targetLocation, options, game: null, ct);

    internal Task<GameSaveOperationResult> RestoreRawFolderToLocationAsync(string sourceFolder, GameSaveLocationInfo targetLocation,
        GameSaveRestoreOptions options, GameSaveEntry? game, CancellationToken ct = default) => RunOperationAsync(async () =>
    {
        if (targetLocation.AppId == 0 || (game != null && game.AppId != targetLocation.AppId) || targetLocation.ResolvedDirectory == null ||
            targetLocation.Status is GameSaveLocationStatus.Unresolved or GameSaveLocationStatus.AccessDenied)
            throw new IOException("Choose a resolved, accessible save directory.");

        var source = CanonicalPath(sourceFolder);
        var target = CanonicalPath(targetLocation.ResolvedDirectory);
        RejectOverlappingDirectories(source, target);
        CheckPath(source, directory: true);
        var warnings = new List<string>();
        var files = CollectFiles(source, "*", true, warnings, ct);
        if (warnings.Count > 0)
            return new(false, ErrorMessage: "The restore source could not be read safely.") { Warnings = warnings };
        if (files.Count == 0)
            return new(false, ErrorMessage: "The source folder contains no restorable files.");

        var plan = new List<RestoreFile>(files.Count);
        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();
            var targetFile = ResolveRelative(target, file.RelativePath);
            CheckPath(targetFile, directory: false, allowMissing: true);
            plan.Add(new(file.Path, targetFile, file.SizeBytes, file.LastWriteUtc));
        }

        var id = targetLocation.AppId;
        var live = targetLocation with { ResolvedDirectory = target, Pattern = "*", Recursive = true };
        var saveSource = new SnapshotSource(id, game?.Name ?? GameSavePathResolver.Number(id),
            game?.SteamId32 ?? 0, game?.SteamId64 ?? 0, [live]);
        return await RestoreAsync(plan, saveSource, source, options, ct).ConfigureAwait(false);
    }, ct);

    public Task<GameSaveOperationResult> DeleteBackupAsync(GameSaveBackupSummary backup, string backupRoot,
        CancellationToken ct = default) => RunOperationAsync(async () =>
    {
        var root = CanonicalPath(backupRoot);
        var snapshot = CanonicalPath(backup.SnapshotDirectory);
        if (!IsWithin(snapshot, root) || Path.GetRelativePath(root, snapshot).Split(Path.DirectorySeparatorChar).Length > 2)
            throw new IOException("The snapshot is outside the supported layout of this backup root.");
        if (backup.BackupId != Path.GetFileName(snapshot))
            throw new IOException("The snapshot directory does not match the selected backup ID.");

        var manifest = await ReadManifestAsync(snapshot, ct).ConfigureAwait(false);
        if (manifest.AppId != backup.AppId || manifest.BackupId != backup.BackupId ||
            manifest.CreatedAtUtc != backup.CreatedAtUtc)
            throw new IOException("The snapshot manifest does not match the selected backup.");

        // Inspect the complete tree before changing anything. Delete checked children individually.
        var tree = ReadSnapshotTree(snapshot, ct);
        var manifestPath = Path.Combine(snapshot, manifestName);
        tree.RemoveAll(entry => PathComparer.Equals(entry.Path, manifestPath));
        tree.Insert(tree.Count - 1, (manifestPath, false));
        ct.ThrowIfCancellationRequested();
        DeleteTree(tree);
        return new(true, TargetPath: snapshot);
    }, ct);

    async Task<GameSaveOperationResult> RunOperationAsync(Func<Task<GameSaveOperationResult>> operation, CancellationToken ct)
    {
        await operations.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            return await Task.Run(operation, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (IsSnapshotError(ex))
        {
            return new(false, ErrorMessage: ex.Message);
        }
        finally
        {
            operations.Release();
        }
    }

    static async Task<GameSaveOperationResult> CreateSnapshotAsync(SnapshotSource source, string destinationRoot,
        string? note, bool isAutoPreRestore, bool forRestore, CancellationToken ct)
    {
        if (source.AppId == 0 || string.IsNullOrWhiteSpace(source.GameName))
            throw new IOException("The save snapshot requires a valid Steam game.");
        var root = CanonicalPath(destinationRoot);
        var appDirectory = ResolveRelative(root, $"{source.AppId}_{SanitizeName(source.GameName)}");
        var warnings = new List<string>();
        var captured = new List<(GameSaveLocationInfo Location, string Directory, List<GameSaveFile> Files)>();
        var uniqueFiles = new HashSet<string>(PathComparer);
        foreach (var location in source.Locations)
        {
            ct.ThrowIfCancellationRequested();
            if ((!forRestore && location.Status != GameSaveLocationStatus.Found) ||
                location.ResolvedDirectory is not { } path)
                continue;
            var directory = CanonicalPath(path);
            try
            {
                if (CheckPath(directory, directory: true, allowMissing: true))
                {
                    var files = CollectFiles(directory, location.Pattern, location.Recursive, warnings, ct);
                    files.RemoveAll(file => !uniqueFiles.Add(file.Path));
                    if (files.Count > 0)
                        captured.Add((location, directory, files));
                }
                else if (location.Status == GameSaveLocationStatus.Found)
                {
                    warnings.Add($"The save directory '{directory}' no longer exists.");
                }
            }
            catch (Exception ex) when (IsFileError(ex))
            {
                warnings.Add($"{directory}: {ex.Message}");
            }
        }
        if (isAutoPreRestore && warnings.Count > 0)
            return new(false, ErrorMessage: "Could not capture all live saves for the safety backup.") { Warnings = warnings };
        if (captured.Count == 0)
            return forRestore ? new(true) :
                new(false, ErrorMessage: "No local save files were available to back up.") { Warnings = warnings };

        var created = DateTime.UtcNow;
        var backupId = $"{created.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture)}_{Guid.NewGuid():N}"[..20];
        var finalDirectory = ResolveRelative(appDirectory, backupId);
        foreach (var location in source.Locations)
        {
            if (location.ResolvedDirectory is not { } path)
                continue;
            var saveDirectory = CanonicalPath(path);
            if (IsAtOrWithin(root, saveDirectory) || IsAtOrWithin(appDirectory, saveDirectory))
                throw new IOException("The backup destination is inside a live save directory.");
            if (IsAtOrWithin(saveDirectory, finalDirectory))
                throw new IOException("A live save directory overlaps the snapshot destination.");
        }

        var staging = ResolveRelative(appDirectory, stagingPrefix + Guid.NewGuid().ToString("N"));
        CreateDirectory(staging);
        try
        {
            long bytes = 0;
            int count = 0;
            var locations = new List<GameSaveBackupLocationEntry>(captured.Count);
            foreach (var (location, directory, files) in captured)
            {
                var index = locations.Count;
                var archivePath = $"files/{index.ToString(CultureInfo.InvariantCulture)}";
                var archive = ResolveRelative(staging, archivePath);
                var entries = new List<GameSaveBackupFileEntry>(files.Count);
                foreach (var file in files)
                {
                    var target = ResolveRelative(archive, file.RelativePath);
                    await CopyFileAsync(new(file.Path, target, file.SizeBytes, file.LastWriteUtc), overwrite: false, ct)
                        .ConfigureAwait(false);
                    entries.Add(new(file.RelativePath.Replace(Path.DirectorySeparatorChar, '/'), file.SizeBytes, file.LastWriteUtc));
                    bytes += file.SizeBytes;
                    count++;
                }
                locations.Add(new(index, location.Kind, location.RootName, directory, location.Pattern, location.Recursive, archivePath, entries));
            }

            var manifest = new GameSaveBackupManifest
            {
                BackupId = backupId,
                AppId = source.AppId,
                GameName = source.GameName,
                SteamId32 = source.SteamId32,
                SteamId64 = source.SteamId64,
                CreatedAtUtc = created,
                Note = note,
                IsAutoPreRestore = isAutoPreRestore,
                TotalBytes = bytes,
                FileCount = count,
                Locations = locations,
            };
            await GameSaveSettingsStore.WriteJsonAsync(Path.Combine(staging, manifestName), manifest,
                GameSaveJsonContext.Default.GameSaveBackupManifest, overwrite: false, ct).ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();
            CheckPath(staging, directory: true);
            CheckPath(finalDirectory, directory: true, allowMissing: true);
            Directory.Move(staging, finalDirectory);
            return new(true, count, bytes, finalDirectory) { Warnings = warnings };
        }
        finally
        {
            TryCleanStagingDirectory(staging);
        }
    }

    static async Task<GameSaveOperationResult> RestoreAsync(IEnumerable<RestoreFile> files, SnapshotSource live,
        string sourceDirectory, GameSaveRestoreOptions options, CancellationToken ct)
    {
        string? autoBackup = null;
        if (options.CreatePreRestoreBackup)
        {
            var root = CanonicalPath(options.PreRestoreBackupRoot ??
                Path.Combine(Path.GetDirectoryName(sourceDirectory)!, autoBackupDirectory));
            if (IsAtOrWithin(root, sourceDirectory))
                throw new IOException("The safety backup cannot be written inside the restore source.");

            var result = await CreateSnapshotAsync(live, root, note: null,
                isAutoPreRestore: true, forRestore: true, ct).ConfigureAwait(false);
            if (!result.Success)
                return result with { ErrorMessage = $"The safety backup failed. {result.ErrorMessage}" };
            autoBackup = result.TargetPath;
        }

        int count = 0;
        long bytes = 0;
        var targetPath = live.Locations.Count == 1 ? live.Locations[0].ResolvedDirectory : null;
        try
        {
            foreach (var file in files)
            {
                await CopyFileAsync(file, overwrite: true, ct).ConfigureAwait(false);
                count++;
                bytes += file.SizeBytes;
            }
            return new(true, count, bytes, targetPath, autoBackup);
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            return new(false, count, bytes, targetPath, autoBackup, ex.Message);
        }
    }

    static async Task CopyFileAsync(RestoreFile file, bool overwrite, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        CheckPath(file.Source, directory: false);
        var directory = Path.GetDirectoryName(file.Target)!;
        CreateDirectory(directory);
        CheckPath(file.Target, directory: false, allowMissing: true);
        var temporaryPath = Path.Combine(directory, $".{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var source = new FileStream(file.Source, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 81920, useAsync: true))
            {
                if (source.Length != file.SizeBytes)
                    throw new IOException($"The size of '{file.Source}' changed while copying.");
                await using var destination = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                    bufferSize: 81920, useAsync: true);
                await source.CopyToAsync(destination, ct).ConfigureAwait(false);
                if (destination.Length != file.SizeBytes)
                    throw new IOException($"The copy of '{file.Source}' does not match its expected size.");
            }
            ct.ThrowIfCancellationRequested();
            CheckPath(temporaryPath, directory: false);
            File.SetLastWriteTimeUtc(temporaryPath, file.LastWriteUtc);
            CheckPath(file.Target, directory: false, allowMissing: true);
            File.Move(temporaryPath, file.Target, overwrite);
        }
        finally
        {
            GameSaveSettingsStore.TryDeleteTemporaryFile(temporaryPath);
        }
    }

    static async Task<GameSaveBackupManifest> ReadManifestAsync(string snapshot, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var path = Path.Combine(snapshot, manifestName);
        CheckPath(path, directory: false);
        await using var stream = File.OpenRead(path);
        var manifest = await SystemTextJsonSerializer.DeserializeAsync(stream, GameSaveJsonContext.Default.GameSaveBackupManifest, ct)
            .ConfigureAwait(false);
        if (manifest is not { Version: 1, AppId: > 0, FileCount: > 0, TotalBytes: >= 0, Locations.Count: > 0 } ||
            string.IsNullOrWhiteSpace(manifest.BackupId) || string.IsNullOrWhiteSpace(manifest.GameName) ||
            manifest.CreatedAtUtc == default || manifest.CreatedAtUtc.Kind != DateTimeKind.Utc)
            throw new InvalidDataException("The backup manifest is invalid or uses an unsupported version.");

        long bytes = 0;
        int count = 0;
        var indices = new HashSet<int>();
        foreach (var location in manifest.Locations)
        {
            ct.ThrowIfCancellationRequested();
            if (location is not { Index: >= 0, Files.Count: > 0 } || !indices.Add(location.Index) ||
                !Enum.IsDefined(location.Kind) || !IsValidPattern(location.Pattern) ||
                string.IsNullOrWhiteSpace(location.RelativeArchiveDir) ||
                string.IsNullOrWhiteSpace(location.OriginalDirectory))
                throw new InvalidDataException("The backup manifest contains an invalid location.");

            var original = CanonicalPath(location.OriginalDirectory);
            var archive = ResolveRelative(snapshot, location.RelativeArchiveDir);
            var expected = Path.Combine(snapshot, "files", location.Index.ToString(CultureInfo.InvariantCulture));
            if (!PathComparer.Equals(archive, expected))
                throw new InvalidDataException("A backup location points to an unexpected archive directory.");

            var paths = new HashSet<string>(PathComparer);
            foreach (var file in location.Files)
            {
                ct.ThrowIfCancellationRequested();
                if (file == null || string.IsNullOrWhiteSpace(file.RelativePath) || file.SizeBytes < 0 ||
                    file.LastWriteUtc == default || file.LastWriteUtc.Kind != DateTimeKind.Utc)
                    throw new InvalidDataException("The backup manifest contains an invalid file entry.");
                var target = ResolveRelative(original, file.RelativePath);
                if (!paths.Add(target) || !MatchesPattern(location.Pattern, Path.GetFileName(target)) ||
                    (!location.Recursive && !PathComparer.Equals(Path.GetDirectoryName(target), original)))
                    throw new InvalidDataException("A backup file is duplicated or outside its declared save rule.");
                if (file.SizeBytes > manifest.TotalBytes - bytes || count >= manifest.FileCount)
                    throw new InvalidDataException("The backup manifest file totals are invalid.");
                bytes += file.SizeBytes;
                count++;
            }
        }
        if (bytes != manifest.TotalBytes || count != manifest.FileCount)
            throw new InvalidDataException("The backup manifest file totals are invalid.");
        return manifest;
    }

    static List<string> ReadChildDirectories(string parent, CancellationToken ct)
    {
        var paths = new List<string>();
        try
        {
            CheckPath(parent, directory: true);
            foreach (var path in Directory.EnumerateDirectories(parent))
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    if (!Path.GetFileName(path).StartsWith(stagingPrefix, StringComparison.Ordinal) &&
                        !File.GetAttributes(CanonicalPath(path)).HasFlag(FileAttributes.ReparsePoint))
                        paths.Add(path);
                }
                catch (Exception ex) when (IsFileError(ex))
                {
                    Log.Error(nameof(GameSaveBackupService), ex, $"Could not read backup directory '{path}'.");
                }
            }
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            Log.Error(nameof(GameSaveBackupService), ex, $"Could not list backup directory '{parent}'.");
        }
        return paths;
    }

    static List<(string Path, bool Directory)> ReadSnapshotTree(string root, CancellationToken ct)
    {
        var entries = new List<(string, bool)>();
        var pending = new Stack<(string Path, bool Directory)>();
        pending.Push((root, true));
        while (pending.TryPop(out var entry))
        {
            ct.ThrowIfCancellationRequested();
            CheckPath(entry.Path, entry.Directory);
            entries.Add(entry);
            if (entry.Directory)
            {
                foreach (var path in Directory.EnumerateFileSystemEntries(entry.Path))
                {
                    ct.ThrowIfCancellationRequested();
                    var attributes = File.GetAttributes(CanonicalPath(path));
                    CheckAttributes(attributes);
                    pending.Push((path, attributes.HasFlag(FileAttributes.Directory)));
                }
            }
        }
        entries.Reverse();
        return entries;
    }

    static void DeleteTree(List<(string Path, bool Directory)> entries)
    {
        foreach (var entry in entries)
        {
            CheckPath(entry.Path, entry.Directory);
            if (entry.Directory)
                Directory.Delete(entry.Path);
            else
                File.Delete(entry.Path);
        }
    }

    static void TryCleanStagingDirectory(string staging)
    {
        try
        {
            if (CheckPath(staging, directory: true, allowMissing: true))
                DeleteTree(ReadSnapshotTree(staging, default));
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            Log.Error(nameof(GameSaveBackupService), ex, $"Could not remove incomplete snapshot '{staging}'.");
        }
    }

    static void ValidateRestoreTargets(IEnumerable<string> paths)
    {
        var targets = paths.ToHashSet(PathComparer);
        foreach (var path in targets)
        {
            for (var parent = Path.GetDirectoryName(path); parent != null; parent = Path.GetDirectoryName(parent))
            {
                if (targets.Contains(parent))
                    throw new IOException("The snapshot treats a restore path as both a file and a directory.");
            }
        }
    }

    static void RejectOverlappingDirectories(string first, string second)
    {
        if (IsAtOrWithin(first, second) || IsAtOrWithin(second, first))
            throw new IOException("The restore source and the live save directory overlap.");
    }

    static string SanitizeName(string gameName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = new string(gameName.Select(x => char.IsControl(x) || "<>:\"/\\|?*".Contains(x) || invalid.Contains(x)
            ? '_' : x).Take(80).ToArray()).Trim(' ', '.');
        return string.IsNullOrWhiteSpace(safe) ? "Game" : safe;
    }

    static bool IsSnapshotError(Exception ex) => IsFileError(ex) || ex is System.Text.Json.JsonException or InvalidDataException;

    static GameSaveBackupSummary ToSummary(GameSaveBackupManifest manifest, string path) =>
        new(manifest.BackupId, path, manifest.AppId, manifest.GameName, manifest.CreatedAtUtc, manifest.Note,
            manifest.IsAutoPreRestore, manifest.TotalBytes, manifest.FileCount, manifest.Locations.Count);

    sealed record SnapshotSource(uint AppId, string GameName, uint SteamId32, long SteamId64, IReadOnlyList<GameSaveLocationInfo> Locations);
    sealed record RestoreFile(string Source, string Target, long SizeBytes, DateTime LastWriteUtc);
}
