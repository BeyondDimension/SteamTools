using BD.SteamClient.Models;

namespace BD.WTTS.GameList.Saves;

public enum GameSaveLocationKind
{
    UfsRule,
    SteamUserdataRemote,
}

public enum GameSaveLocationStatus
{
    Found,
    Empty,
    NotFound,
    Unresolved,
    AccessDenied,
}

public sealed record GameSaveLocationInfo(
    string Id,
    uint AppId,
    GameSaveLocationKind Kind,
    string? RootName,
    string? RawPath,
    string Pattern,
    bool Recursive,
    string? ResolvedDirectory,
    GameSaveLocationStatus Status = GameSaveLocationStatus.Unresolved,
    long TotalBytes = 0,
    int FileCount = 0,
    DateTime? LastModifiedUtc = null,
    string? ErrorMessage = null)
{
    public IReadOnlyList<GameSaveFileItem> Files { get; init; } = [];
}

public sealed record GameSaveFileItem(string Name, string RelativePath, string FullPath, long SizeBytes, DateTime LastWriteUtc);

public sealed record GameScreenshotInfo(
    string DirectoryPath,
    GameSaveLocationStatus Status,
    long TotalBytes,
    int ScreenshotCount,
    DateTime? LastModifiedUtc,
    IReadOnlyList<GameSaveFileItem> Files,
    string? ErrorMessage = null);

public sealed record GameSaveEntry(
    SteamApp App,
    uint AppId,
    string Name,
    bool IsInstalled,
    IReadOnlyList<GameSaveLocationInfo> Locations,
    long TotalBytes,
    int TotalFileCount,
    DateTime? LastModifiedUtc,
    bool HasLocalSaves,
    bool HasUserdataRemote,
    IReadOnlyList<GameSaveBackupSummary> Backups)
{
    public uint SteamId32 { get; init; }
    public long SteamId64 { get; init; }
    public GameScreenshotInfo? Screenshots { get; init; }

    /// <summary>
    /// Bytes in top-level .jpg, .jpeg, .png, .bmp, and .webp images plus all files under thumbnails.
    /// Only the top-level images are included in <see cref="ScreenshotCount"/>.
    /// </summary>
    public long ScreenshotBytes => Screenshots?.TotalBytes ?? 0;

    public int ScreenshotCount => Screenshots?.ScreenshotCount ?? 0;
    public bool HasScreenshots => ScreenshotCount > 0 || ScreenshotBytes > 0;
    public long TotalOccupiedBytes => TotalBytes + ScreenshotBytes;
}

public sealed class GameSaveBackupManifest
{
    [System.Text.Json.Serialization.JsonRequired]
    public int Version { get; init; } = 1;
    public string BackupId { get; init; } = "";
    public uint AppId { get; init; }
    public string GameName { get; init; } = "";
    public uint SteamId32 { get; init; }
    public long SteamId64 { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public string? Note { get; init; }
    public bool IsAutoPreRestore { get; init; }
    public long TotalBytes { get; init; }
    public int FileCount { get; init; }
    public IReadOnlyList<GameSaveBackupLocationEntry> Locations { get; init; } = [];
}

public sealed record GameSaveBackupLocationEntry(
    int Index,
    GameSaveLocationKind Kind,
    string? RootName,
    string OriginalDirectory,
    string Pattern,
    bool Recursive,
    string RelativeArchiveDir,
    IReadOnlyList<GameSaveBackupFileEntry> Files);

public sealed record GameSaveBackupFileEntry(string RelativePath, long SizeBytes, DateTime LastWriteUtc);

public sealed record GameSaveBackupSummary(
    string BackupId,
    string SnapshotDirectory,
    uint AppId,
    string GameName,
    DateTime CreatedAtUtc,
    string? Note,
    bool IsAutoPreRestore,
    long TotalBytes,
    int FileCount,
    int LocationCount);

public sealed record GameSaveRestoreOptions(bool CreatePreRestoreBackup = true, string? PreRestoreBackupRoot = null);

public sealed record GameSaveOperationResult(
    bool Success,
    int FilesProcessed = 0,
    long BytesProcessed = 0,
    string? TargetPath = null,
    string? AutoBackupPath = null,
    string? ErrorMessage = null)
{
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public sealed record GameSaveAccountInfo(long SteamId64, uint SteamId32, string DisplayName, string UserdataPath, bool IsCurrent);

public sealed record GameSaveScanProgress(int ProcessedGames, int TotalGames, string? CurrentGameName);

public enum GameSaveScanPhase
{
    Installed,
    Uninstalled,
    Completed,
}

public sealed record GameSavePhasedScanProgress(
    GameSaveScanPhase Phase, int ProcessedInPhase, int TotalInPhase, string? CurrentGameName, int TotalDiscoveredGamesCount)
{
    public int InstalledGamesWithDataCount { get; init; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(GameSaveBackupManifest))]
[JsonSerializable(typeof(GameSaveSettings))]
internal sealed partial class GameSaveJsonContext : JsonSerializerContext;
