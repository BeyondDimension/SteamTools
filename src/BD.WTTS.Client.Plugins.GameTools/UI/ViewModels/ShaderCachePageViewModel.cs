using BD.WTTS.Services;
using ReactiveUI.Reactive;
using System.Windows.Input;

namespace BD.WTTS.UI.ViewModels;

public sealed class ShaderCachePageViewModel : ViewModelBase
{
    const string Tag = nameof(ShaderCachePageViewModel);
    const long BytesPerMegabyte = 1024 * 1024;
    static readonly SemaphoreSlim cleanupGate = new(1, 1);

    readonly ShaderCacheFileStore files = new((path, ex) => Log.Error(Tag, ex, "Could not access game cache path {0}.", path));
    CancellationTokenSource? scanCancellation;
    string[] steamLibraries = [];
    ShaderCacheRowViewModel[] rows = [];
    string statusText = string.Empty;
    int selectedCount;
    bool isBusy;
    bool isScanning;

    public ShaderCachePageViewModel()
    {
        var openFolder = ReactiveCommand.Create<ShaderCacheRowViewModel>(OpenCacheFolder);
        CompositeDisposable.Add(openFolder);
        OpenCacheFolderCommand = openFolder;
    }

    public IReadOnlyList<ShaderCacheRowViewModel> Rows => rows;

    public ICommand OpenCacheFolderCommand { get; }

    public string StatusText
    {
        get => statusText;
        private set => this.RaiseAndSetIfChanged(ref statusText, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            this.RaiseAndSetIfChanged(ref isBusy, value);
            this.RaisePropertyChanged(nameof(CanClean));
        }
    }

    public bool IsScanning
    {
        get => isScanning;
        private set => this.RaiseAndSetIfChanged(ref isScanning, value);
    }

    public bool CanClean => !IsBusy && selectedCount != 0;

    public decimal? MinimumAgeDays { get; set; } = 30;

    public decimal? MinimumSizeMb { get; set; } = 100;

    public void SelectAll() => Select(static row => row.CanSelect);

    public void SelectOldSteamCaches()
    {
        var cutoff = DateTime.UtcNow.AddDays(-(double)(MinimumAgeDays ?? 0));
        Select(row => row.Location.Kind == ShaderCacheKind.SteamGame && row.Statistics.LastWriteUtc < cutoff);
    }

    public void SelectLargeCaches()
    {
        var cutoff = (MinimumSizeMb ?? 0) * BytesPerMegabyte;
        Select(row => row.CanSelect && row.Statistics.Bytes > cutoff);
    }

    void Select(Func<ShaderCacheRowViewModel, bool> predicate)
    {
        foreach (var row in rows)
            row.IsSelected = predicate(row);
    }

    public void CancelScan() => scanCancellation?.Cancel();

    void OpenCacheFolder(ShaderCacheRowViewModel row)
    {
        if (IsBusy)
            return;

        try
        {
            var path = ShaderCacheFileStore.GetCacheDirectoryPath(row.Location);
            IPlatformService.Instance.OpenFolderByDirectoryPath(new DirectoryInfo(path));
        }
        catch (Exception ex)
        {
            ex.LogAndShowT(Tag);
        }
    }

    public async Task RescanAsync()
    {
        if (IsBusy)
            return;
        IsBusy = true;
        try
        {
            await ScanAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    async Task ScanAsync()
    {
        using var cancellation = new CancellationTokenSource();
        scanCancellation = cancellation;
        IsScanning = true;
        StatusText = Strings.GameTools_Shader_Scanning;
        try
        {
            var discovery = new ShaderCacheDiscovery(files);
            var result = await Task.Run(() => discovery.ScanAsync(cancellation.Token), cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            steamLibraries = result.SteamLibraries;
            selectedCount = 0;
            rows = result.Caches.Select(entry => new ShaderCacheRowViewModel(entry, OnSelectionChanged)).ToArray();
            this.RaisePropertyChanged(nameof(Rows));
            ShowScanSummary();
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            StatusText = Strings.GameTools_Shader_ScanCancelled;
        }
        catch (Exception ex)
        {
            Log.Error(Tag, ex, "Game cache scan failed.");
            StatusText = Strings.GameTools_Shader_ScanFailed;
        }
        finally
        {
            scanCancellation = null;
            IsScanning = false;
        }
    }

    public async Task CleanAsync(Func<string, Task<bool>> confirmAsync)
    {
        if (!CanClean)
            return;
        var selected = rows.Where(static row => row.IsSelected).ToArray();
        var locations = selected.Select(static row => row.Location).ToArray();
        IsBusy = true;
        ShowScanSummary();
        try
        {
            if (!await cleanupGate.WaitAsync(0))
            {
                StatusText = Strings.GameTools_Shader_CleanupInProgress;
                return;
            }
            try
            {
                await ConfirmAndCleanAsync(selected, locations, confirmAsync);
            }
            finally
            {
                cleanupGate.Release();
            }
        }
        catch (Exception ex)
        {
            Log.Error(Tag, ex, "Game cache cleanup failed.");
            StatusText = Strings.GameTools_Shader_CleanFailed;
        }
        finally
        {
            IsBusy = false;
        }
    }

    async Task ConfirmAndCleanAsync(ShaderCacheRowViewModel[] selected, ShaderCacheLocation[] locations,
        Func<string, Task<bool>> confirmAsync)
    {
        var running = await Task.Run(() => ShaderCacheDiscovery.FindRunningProcesses(locations, steamLibraries));
        if (await CheckClientsStoppedAsync(running.Steam, running.GameClients))
            return;

        var message = string.Format(Strings.GameTools_Shader_Confirm, FormatSize(selected.Sum(row => row.Statistics.Bytes)));
        if (running.Driver.Length != 0)
            message += Environment.NewLine + Environment.NewLine +
                string.Format(Strings.GameTools_Shader_DriverRunning, string.Join(Environment.NewLine, running.Driver));
        if (!await confirmAsync(message))
            return;

        StatusText = Strings.GameTools_Shader_Cleaning;
        var current = await Task.Run(() => ShaderCacheDiscovery.FindRunningProcesses(locations, steamLibraries));
        if (await CheckClientsStoppedAsync(current.Steam, current.GameClients))
            return;
        if (current.Driver.Except(running.Driver, StringComparer.OrdinalIgnoreCase).Any())
        {
            await ShowBlockedAsync(Strings.GameTools_Shader_ProcessChanged, current.Driver);
            return;
        }

        var result = await Task.Run(() => CleanCaches(locations));
        var summary = string.Format(Strings.GameTools_Shader_CleanSummary, FormatSize(result.Bytes),
            result.SkippedFiles, result.SkippedDirectories);
        Log.Info(Tag, "Game cache cleanup freed {0} bytes and skipped {1} files and {2} folders.",
            result.Bytes, result.SkippedFiles, result.SkippedDirectories);
        await MessageBox.ShowAsync(summary, Strings.GameTools_Shader_Title);
        await ScanAsync();
    }

    async Task<bool> CheckClientsStoppedAsync(string[] steam, string[] gameClients)
    {
        var running = steam.Union(gameClients, StringComparer.OrdinalIgnoreCase).ToArray();
        if (running.Length == 0)
            return false;

        await ShowBlockedAsync(Strings.GameTools_Shader_ClientRunning, running);
        return true;
    }

    async Task ShowBlockedAsync(string template, string[] names)
    {
        var text = string.Format(template, string.Join(Environment.NewLine, names));
        await MessageBox.ShowAsync(text, Strings.GameTools_Shader_Title, icon: MessageBox.Image.Warning);
        ShowScanSummary();
    }

    ShaderCacheStatistics CleanCaches(IEnumerable<ShaderCacheLocation> locations)
    {
        var total = new ShaderCacheStatistics();
        foreach (var location in locations)
        {
            var result = files.Clean(location, CancellationToken.None);
            total = new(total.Bytes + result.Bytes, total.Files + result.Files, null,
                total.SkippedFiles + result.SkippedFiles, total.SkippedDirectories + result.SkippedDirectories);
        }
        return total;
    }

    void OnSelectionChanged(bool selected)
    {
        selectedCount += selected ? 1 : -1;
        this.RaisePropertyChanged(nameof(CanClean));
    }

    void ShowScanSummary()
    {
        StatusText = rows.Length == 0
            ? Strings.GameTools_Shader_Empty
            : string.Format(Strings.GameTools_Shader_Total, FormatSize(rows.Sum(static row => row.Statistics.Bytes)),
                rows.Sum(static row => row.Statistics.SkippedFiles + row.Statistics.SkippedDirectories));
    }

    internal static string FormatSize(long bytes) => bytes switch
    {
        >= 1024L * BytesPerMegabyte => $"{bytes / (1024d * BytesPerMegabyte):N2} GB",
        >= BytesPerMegabyte => $"{bytes / (double)BytesPerMegabyte:N2} MB",
        >= 1024 => $"{bytes / 1024d:N2} KB",
        _ => $"{bytes:N0} B",
    };
}

public sealed class ShaderCacheRowViewModel : ReactiveObject
{
    readonly Action<bool> onSelectionChanged;
    bool isSelected;

    internal ShaderCacheRowViewModel(ShaderCacheScanEntry entry, Action<bool> onSelectionChanged)
    {
        Location = entry.Location;
        Statistics = entry.Statistics;
        this.onSelectionChanged = onSelectionChanged;
    }

    internal ShaderCacheLocation Location { get; }

    internal ShaderCacheStatistics Statistics { get; }

    public string Name => Location.Kind switch
    {
        ShaderCacheKind.SteamGame => string.Format(Strings.GameTools_Shader_SteamGame, Location.Name),
        ShaderCacheKind.SteamTemporary => Strings.GameTools_Shader_SteamTemporary,
        ShaderCacheKind.GameClient or ShaderCacheKind.Game => string.Format(Strings.GameTools_Shader_ProductCache, Location.Name),
        _ => Location.Name,
    };

    public string Path => Location.Path;

    public string Size => CanSelect ? ShaderCachePageViewModel.FormatSize(Statistics.Bytes) : Strings.GameTools_Shader_Unavailable;

    public string FileCount => CanSelect ? Statistics.Files.ToString("N0") : "—";

    public string LastModified => Statistics.LastWriteUtc?.ToLocalTime().ToString("g") ?? "—";

    public bool CanSelect => Statistics.LastWriteUtc.HasValue;

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected == value)
                return;
            this.RaiseAndSetIfChanged(ref isSelected, value);
            onSelectionChanged(value);
        }
    }
}
