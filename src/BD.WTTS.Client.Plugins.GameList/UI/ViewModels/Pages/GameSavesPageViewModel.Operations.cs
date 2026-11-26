using BD.SteamClient.Models;
using BD.WTTS.GameList.Saves;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class GameSavesPageViewModel
{
    public Task BackupGameAsync(GameSaveGameItemViewModel game, string? destination = null) =>
        game.HasLocalSaves && IsCurrent(game) ? BackupAsync([game.Entry], destination) : Task.CompletedTask;

    public Task BackupSelectedAsync(string? destination = null) =>
        CanBatch ? BackupAsync(games.Values.Where(game => game.IsSelected).Select(game => game.Entry).ToArray(), destination)
            : Task.CompletedTask;

    Task BackupAsync(GameSaveEntry[] entries, string? destination)
    {
        var copy = destination != null;
        var failure = copy ? Strings.GameList_Saves_CopyFailed : Strings.GameList_Saves_BackupFailed;
        return RunMutationAsync(failure, async () =>
        {
            var results = await backups.BackupGamesAsync(entries, destination ?? BackupRootPath);
            var succeeded = results.Count(result => result.Success);
            var warnings = false;
            foreach (var result in results)
                warnings |= LogDiagnostics(result);

            if (succeeded == 0)
                ShowError(failure);
            else
            {
                var failed = results.Count - succeeded;
                var message = failed > 0
                    ? string.Format(copy ? Strings.GameList_Saves_CopyPartial : Strings.GameList_Saves_BackupPartial, succeeded, failed)
                    : string.Format(copy ? Strings.GameList_Saves_CopyComplete : Strings.GameList_Saves_BackupComplete, succeeded);
                NotifyCompletion(message, warnings || failed > 0);
            }

            if (destination == null || GameSaveFileSystem.PathComparer.Equals(
                    GameSaveFileSystem.CanonicalPath(destination), GameSaveFileSystem.CanonicalPath(BackupRootPath)))
                await RefreshBackupsAfterChangeAsync();
        });
    }

    public Task ChangeBackupRootAsync(string path) =>
        RunMutationAsync(Strings.GameList_Saves_BackupFolderFailed, async () =>
        {
            var root = GameSaveFileSystem.CanonicalPath(path);
            var updatedSettings = settings with { BackupRootPath = root };
            await Task.Run(() => settingsStore.SaveAsync(updatedSettings));
            SetSettings(updatedSettings);
            SetBackups([]);
            await RefreshBackupsAfterChangeAsync();
            Toast.Show(ToastIcon.Success, Strings.GameList_Saves_BackupFolderUpdated);
        });

    async Task SetAutoBackupAsync(bool value)
    {
        await RunMutationAsync(Strings.GameList_Saves_SettingsSaveFailed, async () =>
        {
            var updatedSettings = settings with { AutoBackupBeforeRestore = value };
            await Task.Run(() => settingsStore.SaveAsync(updatedSettings));
            SetSettings(updatedSettings);
        });
        this.RaisePropertyChanged(nameof(AutoBackupBeforeRestore));
    }

    internal bool IsCurrentSnapshot(GameSaveBackupItemViewModel snapshot) =>
        IsCurrent(snapshot.Game) && snapshot.Game.Backups.Any(item => GameSaveFileSystem.PathComparer.Equals(
            item.Summary.SnapshotDirectory, snapshot.Summary.SnapshotDirectory));

    public Task DeleteBackupAsync(GameSaveBackupItemViewModel backup) =>
        RunMutationAsync(Strings.GameList_Saves_DeleteFailed, async () =>
        {
            if (IsCurrentSnapshot(backup) && Complete(await backups.DeleteBackupAsync(backup.Summary, BackupRootPath),
                _ => Strings.GameList_Saves_DeleteComplete, Strings.GameList_Saves_DeleteFailed))
                await RefreshBackupsAfterChangeAsync();
        });

    internal Task RestoreAsync(GameSaveGameItemViewModel game, string folder, bool fromSnapshot,
        Func<GameSaveRestorePreview, Task<GameSaveRestoreSelection?>> confirm) =>
        RunMutationAsync(Strings.GameList_Saves_RestoreFailed, async () =>
        {
            if (!IsCurrent(game))
                return;

            GameSaveBackupManifest? manifest;
            try
            {
                manifest = await GameSaveBackupService.ReadSnapshotManifestAsync(folder);
            }
            catch (Exception ex) when (ex is InvalidDataException or System.Text.Json.JsonException)
            {
                ShowError(Strings.GameList_Saves_SnapshotInvalid, ex);
                return;
            }
            if (manifest != null && manifest.AppId != game.Entry.AppId)
            {
                ShowError(Strings.GameList_Saves_SnapshotWrongGame);
                return;
            }
            if (fromSnapshot && manifest == null)
            {
                ShowError(Strings.GameList_Saves_SnapshotInvalid);
                return;
            }

            var locations = manifest == null ? game.Entry.Locations.Select(location => new GameSaveLocationItemViewModel(location))
                .Where(location => location.CanRestore).DistinctBy(location => location.Path!, GameSaveFileSystem.PathComparer).ToArray()
                : [];
            if (manifest == null && locations.Length == 0)
            {
                ShowError(Strings.GameList_Saves_RestoreNoTarget);
                return;
            }

            var targets = manifest?.Locations.Select(location => location.OriginalDirectory)
                .Distinct(GameSaveFileSystem.PathComparer).ToArray() ?? [];
            var selection = await confirm(new(folder, manifest, targets, locations, settings.AutoBackupBeforeRestore));
            if (selection == null)
                return;

            var options = new GameSaveRestoreOptions(selection.AutoBackup, BackupRootPath);
            var result = manifest != null
                ? await backups.RestoreFromSnapshotAsync(folder, options, manifest)
                : await backups.RestoreRawFolderToLocationAsync(folder, selection.Location!.Location, options, game.Entry);
            var failure = result.FilesProcessed > 0
                ? string.Format(Strings.GameList_Saves_RestorePartial, result.FilesProcessed)
                : Strings.GameList_Saves_RestoreFailed;
            Complete(result, count => string.Format(Strings.GameList_Saves_RestoreComplete, count), failure);

            // A partial restore can still change files and create a pre-restore backup.
            try
            {
                var entry = await scanner.RescanGameAsync(game.App, scannedAccount);
                filteredGames.Remove(game);
                game.UpdateEntry(CurrentEntry(entry));
                if (MatchesFilter(game))
                    InsertSorted(game);
                scan?.RestoredApps.Add(entry.AppId);
                UpdateSelection();
                UpdateMetrics();
                NotifyEmptyState();
            }
            catch (Exception ex)
            {
                ShowError(Strings.GameList_Saves_ScanFailed, ex);
            }
            await RefreshBackupsAfterChangeAsync();
        });

    bool IsCurrent(GameSaveGameItemViewModel game) =>
        games.TryGetValue(game.Entry.AppId, out var current) && current == game;

    async Task RefreshBackupsAfterChangeAsync()
    {
        try
        {
            SetBackups(await backups.ListBackupsAsync(BackupRootPath));
        }
        catch (Exception ex)
        {
            ShowError(Strings.GameList_Saves_RefreshFailed, ex);
        }
    }

    void SetBackups(IReadOnlyList<GameSaveBackupSummary> snapshots)
    {
        var byApp = snapshots.ToLookup(snapshot => snapshot.AppId);
        refreshedBackups = byApp;
        foreach (var game in games.Values)
            game.SetBackups(byApp[game.Entry.AppId].ToArray());

        // Let the scanner publish the actual game before exposing its restore actions.
        if (scan == null)
            AddBackupOnlyRows(byApp);
        UpdateMetrics();
        if (filterIndex == 4)
            ApplyFilter();
        else
            NotifyEmptyState();
    }

    void AddBackupOnlyRows(ILookup<uint, GameSaveBackupSummary> byApp)
    {
        // Backups may also exist for apps absent from the local library cache.
        foreach (var group in byApp)
        {
            if (games.ContainsKey(group.Key))
                continue;

            var app = new SteamApp(group.Key) { Name = group.First().GameName };
            var row = CreateRow(new(app, app.AppId, app.DisplayName, false, [], 0, 0, null, false, false, group.ToArray()));
            games.Add(group.Key, row);
            AccumulateMetrics(row, 1);
            if (MatchesFilter(row))
                InsertSorted(row);
        }
        SetDiscoveryCount(Math.Max(TotalDiscoveredGamesCount, games.Count));
    }

    async Task RunMutationAsync(string failure, Func<Task> operation)
    {
        if (!CanMutate)
            return;

        SetMutating(true);
        try
        {
            if (!settingsLoaded)
            {
                SetSettings(await Task.Run(() => settingsStore.LoadAsync()));
                settingsLoaded = true;
            }
            await operation();
        }
        catch (Exception ex)
        {
            ShowError(failure, ex);
        }
        finally
        {
            SetMutating(false);
            if (rescanAfterMutation)
            {
                rescanAfterMutation = false;
                _ = EnsureScannedAsync();
            }
        }
    }

    void SetMutating(bool value)
    {
        isMutating = value;
        this.RaisePropertyChanged(nameof(IsMutating));
        this.RaisePropertyChanged(nameof(CanMutate));
        this.RaisePropertyChanged(nameof(CanRefreshAccounts));
        NotifySelection();
    }

    static bool Complete(GameSaveOperationResult result, Func<int, string> summary, string failure)
    {
        var warnings = LogDiagnostics(result);
        if (!result.Success)
        {
            ShowError(failure);
            return false;
        }
        NotifyCompletion(summary(result.FilesProcessed), warnings);
        return true;
    }

    static bool LogDiagnostics(GameSaveOperationResult result)
    {
        if (result.ErrorMessage is { } error)
            Log.Warn(nameof(GameSavesPageViewModel), "{0}", error);
        foreach (var warning in result.Warnings)
            Log.Warn(nameof(GameSavesPageViewModel), "{0}", warning);
        return result.Warnings.Count > 0;
    }

    static void NotifyCompletion(string message, bool warnings)
    {
        Toast.Show(ToastIcon.Success, message);
        if (warnings)
            Toast.Show(ToastIcon.Warning, Strings.GameList_Saves_OperationWarnings);
    }

    internal static void ShowError(string message, Exception? error = null)
    {
        if (error != null)
            Log.Error(nameof(GameSavesPageViewModel), error, "{0}", message);
        Toast.Show(ToastIcon.Error, message);
    }

    internal static string FormatSize(long bytes) => bytes switch
    {
        >= 1L << 40 => string.Format(Strings.GameList_Saves_TerabytesFormat, bytes / (double)(1L << 40)),
        >= 1L << 30 => string.Format(Strings.GameList_Saves_GigabytesFormat, bytes / (double)(1L << 30)),
        >= 1L << 20 => string.Format(Strings.GameList_Saves_MegabytesFormat, bytes / (double)(1L << 20)),
        >= 1L << 10 => string.Format(Strings.GameList_Saves_KilobytesFormat, bytes / (double)(1L << 10)),
        _ => string.Format(Strings.GameList_Saves_BytesFormat, bytes),
    };

    internal static string FormatDate(DateTime? utc) =>
        utc?.ToLocalTime().ToString(Strings.GameList_Saves_DateTimeFormat, CultureInfo.CurrentCulture) ??
        Strings.GameList_Saves_Unavailable;
}

internal sealed record GameSaveRestorePreview(string SourceFolder, GameSaveBackupManifest? Manifest,
    IReadOnlyList<string> TargetDirectories, IReadOnlyList<GameSaveLocationItemViewModel> RawTargets, bool AutoBackup);

internal sealed record GameSaveRestoreSelection(bool AutoBackup, GameSaveLocationItemViewModel? Location);
