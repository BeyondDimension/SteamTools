#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
namespace BD.WTTS.Services;

public sealed partial class SteamConnectService
{
    readonly object idleProcessGate = new();
    readonly Func<SteamApp, Process?> startIdleProcess;
    int connectionMonitorStarted;

    public SteamApp StartIdleApp(SteamApp app)
    {
        lock (idleProcessGate)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return StartIdleAppCore(app);
        }
    }

    SteamApp StartIdleAppCore(SteamApp app)
    {
        if (app.AppId == 0) throw new ArgumentOutOfRangeException(nameof(app));
        var current = RuningSteamApps.GetOrAdd(app.AppId, app);
        if (IsIdleProcessAlive(current.Process)) return current;
        try
        {
            current.Process = startIdleProcess(current);
            if (current.Process is null) RuningSteamApps.TryRemove(app.AppId, out _);
            return current;
        }
        catch
        {
            RuningSteamApps.TryRemove(app.AppId, out _);
            throw;
        }
    }

    public void StopIdleApp(uint appId)
    {
        lock (idleProcessGate)
        {
            if (!RuningSteamApps.TryGetValue(appId, out var current)) return;
            current.StopSteamAppProcess();
            RuningSteamApps.TryRemove(appId, out _);
        }
    }

    public SteamApp ToggleIdleApp(SteamApp app)
    {
        lock (idleProcessGate)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            if (RuningSteamApps.TryGetValue(app.AppId, out var current) && IsIdleProcessAlive(current.Process))
            {
                current.StopSteamAppProcess();
                RuningSteamApps.TryRemove(app.AppId, out _);
                return current;
            }
            return StartIdleAppCore(app);
        }
    }

    public bool TryGetRunningIdleApp(uint appId, out SteamApp? app)
    {
        lock (idleProcessGate)
        {
            if (RuningSteamApps.TryGetValue(appId, out var current))
            {
                if (IsIdleProcessAlive(current.Process))
                {
                    app = current;
                    return true;
                }
                current.StopSteamAppProcess();
                RuningSteamApps.TryRemove(appId, out _);
            }
            app = null;
            return false;
        }
    }

    internal int StartConfiguredIdleApps(IEnumerable<KeyValuePair<uint, string?>> configured)
    {
        lock (idleProcessGate)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            var started = 0;
            foreach (var entry in configured)
            {
                if (entry.Key == 0) continue;
                if (RuningSteamApps.TryGetValue(entry.Key, out var existing) && IsIdleProcessAlive(existing.Process)) continue;
                var app = existing ?? new SteamApp(entry.Key) { Name = entry.Value };
                try
                {
                    if (StartIdleAppCore(app).Process is not null) started++;
                }
                catch (Exception ex)
                {
                    ex.LogAndShowT(nameof(SteamConnectService));
                }
            }
            return started;
        }
    }

    static bool IsIdleProcessAlive(Process? process)
    {
        if (process is null) return false;
        try { return !process.HasExited; }
        catch (InvalidOperationException) { return false; }
    }
}
#endif
