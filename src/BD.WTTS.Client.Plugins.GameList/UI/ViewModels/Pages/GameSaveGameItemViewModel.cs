using BD.SteamClient.Models;
using BD.WTTS.GameList.Saves;
using static BD.WTTS.UI.ViewModels.GameSavesPageViewModel;

namespace BD.WTTS.UI.ViewModels;

public sealed class GameSaveGameItemViewModel : ReactiveObject
{
    readonly Action selectionChanged;
    IReadOnlyList<GameSaveBackupSummary> snapshots;
    bool isSelected;
    bool isExpanded;
    bool detailsLoaded;

    internal GameSaveGameItemViewModel(GameSavesPageViewModel page, GameSaveEntry entry, Action selectionChanged)
    {
        Page = page;
        Entry = entry;
        snapshots = entry.Backups;
        this.selectionChanged = selectionChanged;
    }

    internal GameSaveEntry Entry { get; private set; }
    public GameSavesPageViewModel Page { get; }
    public SteamApp App => Entry.App;
    public string Name => Entry.Name;
    public bool IsInstalled => Entry.IsInstalled;
    public bool HasLocalSaves => Entry.HasLocalSaves;
    public bool HasScreenshots => Entry.HasScreenshots;
    public bool HasOpenableFolder => ScreenshotPath != null || Entry.Locations.Any(GameSaveLocationItemViewModel.CanOpenLocation);
    public string? ScreenshotPath => Entry.Screenshots is { Status: GameSaveLocationStatus.Found or GameSaveLocationStatus.Empty } screenshots
        ? screenshots.DirectoryPath : null;
    public bool HasScreenshotFolder => ScreenshotPath != null && HasScreenshots;
    public string InstallStatus => Entry.IsInstalled ? Strings.GameList_Saves_Installed : Strings.GameList_Saves_NotInstalled;
    public string SavesSizeDisplay => FormatSize(Entry.TotalBytes);
    public string ScreenshotsSizeDisplay => FormatSize(Entry.ScreenshotBytes);
    public string TotalOccupiedSizeDisplay => FormatSize(Entry.TotalOccupiedBytes);
    public int SaveFileCount => Entry.TotalFileCount;
    public int ScreenshotCount => Entry.ScreenshotCount;
    public string SavesStatistics => string.Format(Strings.GameList_Saves_SizeAndFiles, SavesSizeDisplay, SaveFileCount);
    public string Metadata => string.Format(Strings.GameList_Saves_CardMetadata, Entry.AppId,
        SavesSizeDisplay, ScreenshotsSizeDisplay, ScreenshotCount);
    internal DateTime? LastModifiedUtc
    {
        get
        {
            var saves = Entry.LastModifiedUtc;
            var screenshots = Entry.Screenshots?.LastModifiedUtc;
            return Nullable.Compare(saves, screenshots) >= 0 ? saves : screenshots;
        }
    }

    public IReadOnlyList<GameSaveLocationItemViewModel> Locations { get; private set; } = [];
    public bool HasLocations => Locations.Count > 0;
    public GameSaveScreenshotItemViewModel? ScreenshotTree { get; private set; }
    public IReadOnlyList<GameSaveBackupItemViewModel> Backups { get; private set; } = [];
    public int BackupCount => snapshots.Count;
    internal long BackupBytes => snapshots.Sum(backup => backup.TotalBytes);
    public bool HasBackups => BackupCount > 0;
    public string BackupBadge => string.Format(Strings.GameList_Saves_BackupBadge, BackupCount);
    public string BackupCountDisplay => string.Format(Strings.GameList_Saves_SnapshotCount, BackupCount);

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            var valid = value && HasLocalSaves;
            if (isSelected == valid)
                return;

            this.RaiseAndSetIfChanged(ref isSelected, valid);
            selectionChanged();
        }
    }

    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (isExpanded == value)
                return;

            if (value && !detailsLoaded)
                LoadDetails();

            this.RaiseAndSetIfChanged(ref isExpanded, value);
            this.RaisePropertyChanged(nameof(ExpansionLabel));
            this.RaisePropertyChanged(nameof(ExpandedDetails));
        }
    }

    public string ExpansionLabel => IsExpanded ? Strings.GameList_Saves_Collapse : Strings.GameList_Saves_Expand;
    public GameSaveGameItemViewModel? ExpandedDetails => IsExpanded ? this : null;

    void LoadDetails()
    {
        detailsLoaded = true;
        Locations = Entry.Locations.Select(location => new GameSaveLocationItemViewModel(location)).ToArray();
        ScreenshotTree = new GameSaveScreenshotItemViewModel(Entry.Screenshots);
        Backups = snapshots.Select(backup => new GameSaveBackupItemViewModel(this, backup)).ToArray();
        this.RaisePropertyChanged(nameof(Locations));
        this.RaisePropertyChanged(nameof(HasLocations));
        this.RaisePropertyChanged(nameof(ScreenshotTree));
        this.RaisePropertyChanged(nameof(Backups));
    }

    internal void UpdateEntry(GameSaveEntry entry)
    {
        Entry = entry;
        snapshots = entry.Backups;
        if (detailsLoaded)
            LoadDetails();

        this.RaisePropertyChanged(nameof(App));
        this.RaisePropertyChanged(nameof(Name));
        this.RaisePropertyChanged(nameof(IsInstalled));
        this.RaisePropertyChanged(nameof(HasLocalSaves));
        this.RaisePropertyChanged(nameof(HasScreenshots));
        this.RaisePropertyChanged(nameof(HasOpenableFolder));
        this.RaisePropertyChanged(nameof(HasScreenshotFolder));
        this.RaisePropertyChanged(nameof(InstallStatus));
        this.RaisePropertyChanged(nameof(Metadata));
        this.RaisePropertyChanged(nameof(SavesStatistics));
        this.RaisePropertyChanged(nameof(TotalOccupiedSizeDisplay));
        NotifyBackupMetrics();
        if (!HasLocalSaves)
            IsSelected = false;
    }

    internal void SetBackups(IReadOnlyList<GameSaveBackupSummary> backups)
    {
        snapshots = backups;
        if (detailsLoaded)
        {
            Backups = backups.Select(backup => new GameSaveBackupItemViewModel(this, backup)).ToArray();
            this.RaisePropertyChanged(nameof(Backups));
        }
        NotifyBackupMetrics();
    }

    void NotifyBackupMetrics()
    {
        this.RaisePropertyChanged(nameof(HasBackups));
        this.RaisePropertyChanged(nameof(BackupBadge));
        this.RaisePropertyChanged(nameof(BackupCountDisplay));
    }
}

public sealed class GameSaveLocationItemViewModel(GameSaveLocationInfo location)
{
    internal GameSaveLocationInfo Location { get; } = location;
    public string? Path => Location.ResolvedDirectory;
    public bool HasPath => !string.IsNullOrEmpty(Path);
    public bool CanOpen => CanOpenLocation(Location);
    internal bool CanRestore => CanOpen || (HasPath && Location.Status == GameSaveLocationStatus.NotFound);
    public string PathDisplay => Path ?? Strings.GameList_Saves_PathUnresolved;
    public string Source => Location.Kind == GameSaveLocationKind.UfsRule
        ? Strings.GameList_Saves_SourceRule : Strings.GameList_Saves_SourceCloud;
    public string Statistics => Location.Status switch
    {
        GameSaveLocationStatus.Found => string.Format(Strings.GameList_Saves_SizeAndFiles, FormatSize(Location.TotalBytes), Location.FileCount),
        GameSaveLocationStatus.Empty => Strings.GameList_Saves_LocationEmpty,
        GameSaveLocationStatus.NotFound => Strings.GameList_Saves_LocationMissing,
        GameSaveLocationStatus.AccessDenied => Strings.GameList_Saves_LocationDenied,
        _ => Strings.GameList_Saves_LocationUnresolved,
    };
    public GameSaveFileTreeViewModel Tree { get; } = new(location.Files.Count > 0, false, () => GameSaveTreeBuilder.Build(location));

    internal static bool CanOpenLocation(GameSaveLocationInfo location) =>
        !string.IsNullOrEmpty(location.ResolvedDirectory) && location.Status is GameSaveLocationStatus.Found or GameSaveLocationStatus.Empty;
}

public sealed class GameSaveScreenshotItemViewModel(GameScreenshotInfo? screenshots)
{
    public string? Path => screenshots?.DirectoryPath;
    public bool CanOpen => screenshots?.Status is GameSaveLocationStatus.Found or GameSaveLocationStatus.Empty;
    public string Statistics => string.Format(Strings.GameList_Saves_ScreenshotSizeAndCount,
        FormatSize(screenshots?.TotalBytes ?? 0), screenshots?.ScreenshotCount ?? 0);
    public GameSaveFileTreeViewModel Tree { get; } = new(screenshots is { Files.Count: > 0 }, true, () => (screenshots?.Files ?? [])
        .OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase)
        .Select(file => new GameSaveTreeNode(file.Name, file.FullPath, file.RelativePath,
            false, file.SizeBytes, 1, file.LastWriteUtc, [])).ToArray());
}

public sealed class GameSaveBackupItemViewModel(GameSaveGameItemViewModel game, GameSaveBackupSummary summary)
{
    public GameSaveGameItemViewModel Game { get; } = game;
    internal GameSaveBackupSummary Summary { get; } = summary;
    public string Timestamp => FormatDate(Summary.CreatedAtUtc);
    public string Statistics => string.Format(Strings.GameList_Saves_SizeAndFiles, FormatSize(Summary.TotalBytes), Summary.FileCount);
    public string? Note => Summary.Note;
    public bool HasNote => !string.IsNullOrWhiteSpace(Note);
    public bool IsAutoPreRestore => Summary.IsAutoPreRestore;
}
