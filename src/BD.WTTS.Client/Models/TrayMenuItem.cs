namespace BD.WTTS.Models;

public partial class TrayMenuItem
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? NameResourceKey { get; set; }

    public ICommand? Command { get; set; }

    public object? CommandParameter { get; set; }

    /// <summary>Source of the confirmed state and availability of a toggle.</summary>
    public IObservable<TrayToggleState>? ToggleState { get; set; }

    /// <summary>Returns the successful acceleration session, if one is active.</summary>
    public Func<TrayAccelerationSession?>? GetAccelerationSession { get; set; }

    /// <summary>Saved accounts for an account group. The group's command opens account management.</summary>
    public IObservable<TrayAccountsSnapshot>? Accounts { get; set; }

    public ICommand? RefreshAccountsCommand { get; set; }

    public List<TrayMenuItem>? Items { get; set; }

    public object? IsVisible { get; set; }

    public object? IsEnabled { get; set; }

    public int Order { get; set; }
}

public enum TrayToggleState
{
    Unavailable,
    Loading,
    Off,
    Starting,
    On,
    Stopping,
    Preparing,
}

public sealed record TrayAccelerationSession(
    long StartedTimestamp,
    IReadOnlyList<string> Projects,
    Func<TrayTrafficSample?> ReadTraffic);

public readonly record struct TrayTrafficSample(long UploadedBytes, long DownloadedBytes, double UploadBytesPerSecond, double DownloadBytesPerSecond);

public enum TrayAccountsStatus
{
    Loading,
    Loaded,
    Failed,
}

public readonly record struct TrayAccountsSnapshot(TrayAccountsStatus Status, IReadOnlyList<TrayAccountItem> Rows);

public sealed record TrayAccountItem(
    string Name,
    string? Detail,
    string? AvatarUrl,
    bool IsCurrent,
    bool IsRecent,
    ICommand Command,
    object? CommandParameter)
{
    public bool HasDetail => !string.IsNullOrWhiteSpace(Detail);

    public bool ShowRecentBadge => IsRecent && !IsCurrent;

    public bool ShowSwitchHint => !IsRecent && !IsCurrent;
}
