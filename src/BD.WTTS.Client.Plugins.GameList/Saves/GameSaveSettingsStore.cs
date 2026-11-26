using static BD.WTTS.GameList.Saves.GameSaveFileSystem;

namespace BD.WTTS.GameList.Saves;

public sealed record GameSaveSettings(string BackupRootPath, bool AutoBackupBeforeRestore = true);

public sealed class GameSaveSettingsStore
{
    readonly string appDataDirectory;

    public GameSaveSettingsStore() : this(IOPath.AppDataDirectory)
    {
    }

    public GameSaveSettingsStore(string appDataDirectory)
    {
        this.appDataDirectory = CanonicalPath(appDataDirectory);
    }

    public string DefaultBackupRootPath => Path.Combine(appDataDirectory, "GameSaveBackups");

    string SettingsPath => Path.Combine(appDataDirectory, "GameSaves", "settings.json");

    public async Task<GameSaveSettings> LoadAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (!CheckPath(SettingsPath, directory: false, allowMissing: true))
            return new(DefaultBackupRootPath);

        await using var stream = File.OpenRead(SettingsPath);
        var settings = await SystemTextJsonSerializer.DeserializeAsync(stream, GameSaveJsonContext.Default.GameSaveSettings, ct)
            .ConfigureAwait(false) ?? new(DefaultBackupRootPath);
        return string.IsNullOrWhiteSpace(settings.BackupRootPath)
            ? settings with { BackupRootPath = DefaultBackupRootPath }
            : settings;
    }

    public Task SaveAsync(GameSaveSettings settings, CancellationToken ct = default)
    {
        var root = CanonicalPath(settings.BackupRootPath);
        CheckPath(root, directory: true, allowMissing: true);
        return WriteJsonAsync(SettingsPath, settings with { BackupRootPath = root },
            GameSaveJsonContext.Default.GameSaveSettings, overwrite: true, ct);
    }

    internal static async Task WriteJsonAsync<T>(string path, T value, JsonTypeInfo<T> typeInfo, bool overwrite, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var directory = Path.GetDirectoryName(path)!;
        CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await SystemTextJsonSerializer.SerializeAsync(stream, value, typeInfo, ct).ConfigureAwait(false);
                await stream.FlushAsync(ct).ConfigureAwait(false);
            }

            ct.ThrowIfCancellationRequested();
            CheckPath(temporaryPath, directory: false);
            CheckPath(path, directory: false, allowMissing: true);
            File.Move(temporaryPath, path, overwrite);
        }
        finally
        {
            TryDeleteTemporaryFile(temporaryPath);
        }
    }

    internal static void TryDeleteTemporaryFile(string path)
    {
        try
        {
            if (CheckPath(path, directory: false, allowMissing: true))
                File.Delete(path);
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            Log.Error(nameof(GameSaveSettingsStore), ex, $"Could not clean up '{path}'.");
        }
    }
}
