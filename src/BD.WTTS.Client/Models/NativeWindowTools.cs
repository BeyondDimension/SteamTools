namespace BD.WTTS.Models;

public sealed record NativeWindowMonitor(string DeviceName, bool IsPrimary, int X, int Y, int Width, int Height)
{
    public override string ToString() => $"{DeviceName} ({Width} × {Height})";
}

public readonly record struct NativeWindowBounds(int X, int Y, int Width, int Height);

public enum NativeWindowMonitorAction { Center, Move, Fit }

public enum NativeWindowBackgroundPriority { Unchanged, BelowNormal, Idle }

public sealed record NativeWindowToolOptions(
    bool Topmost = false,
    int Opacity = 100,
    bool ClickThrough = false,
    bool ConfineCursor = false,
    bool MuteInBackground = false,
    NativeWindowBackgroundPriority BackgroundPriority = NativeWindowBackgroundPriority.Unchanged);

public sealed record NativeWindowToolInfo(
    NativeWindowBounds Bounds,
    NativeWindowToolOptions Options,
    bool Borderless,
    bool WallpaperAttached,
    bool WallpaperPaused);

public sealed record NativeWindowRule(
    string ProcessName,
    bool AutoApply,
    bool Borderless,
    bool Topmost,
    int Opacity,
    string? FitMonitor,
    bool MuteInBackground,
    NativeWindowBackgroundPriority BackgroundPriority);
