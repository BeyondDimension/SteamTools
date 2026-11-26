#if WINDOWS
using static BD.WTTS.Services.Implementation.WindowToolsNative;

namespace BD.WTTS.Services.Implementation;

unsafe partial class WindowToolsController
{
    sealed class ProcessEffects(Process process)
    {
        internal Process Process { get; } = process;
        internal ProcessPriorityClass? OriginalPriority { get; set; }
        internal Dictionary<string, bool> OriginalMuteStates { get; } = new(StringComparer.Ordinal);
    }

    readonly Dictionary<uint, ProcessEffects> processEffects = new();
    nint clippedWindow;
    Rect clippedRect;

    void UpdateFocusBehaviors()
    {
        var foreground = GetForegroundWindow();
        GetWindowThreadProcessId(foreground, out var foregroundPid);
        if (keyboardWindow?.Wallpaper is { } layer && !layer.IsDesktopForeground(foreground))
            keyboardWindow = null;
        UpdateCursor(foreground);

        var wanted = windows.Values
            .Where(window => (window.Options.MuteInBackground || window.Options.BackgroundPriority != NativeWindowBackgroundPriority.Unchanged)
                && Matches(window.Handle, window.Pid))
            .GroupBy(window => window.Pid)
            .ToDictionary(group => group.Key, group => (
                Mute: group.Any(window => window.Options.MuteInBackground),
                Priority: group.Max(window => window.Options.BackgroundPriority)));

        foreach (var (pid, effects) in processEffects.ToArray())
        {
            if (!wanted.ContainsKey(pid))
            {
                ApplyProcessEffects(effects, false, NativeWindowBackgroundPriority.Unchanged);
                effects.Process.Dispose();
                processEffects.Remove(pid);
            }
        }

        foreach (var (pid, behavior) in wanted)
        {
            if (!processEffects.TryGetValue(pid, out var effects))
            {
                try
                {
                    effects = new(Process.GetProcessById((int)pid));
                    processEffects.Add(pid, effects);
                }
                catch (ArgumentException)
                {
                    continue; // The process closed since the foreground event was queued.
                }
            }

            var background = pid != foregroundPid;
            ApplyProcessEffects(effects, background && behavior.Mute,
                background ? behavior.Priority : NativeWindowBackgroundPriority.Unchanged);
        }
    }

    static void ApplyProcessEffects(ProcessEffects effects, bool mute, NativeWindowBackgroundPriority priority)
    {
        try
        {
            var process = effects.Process;
            if (process.HasExited)
                return;

            if (priority == NativeWindowBackgroundPriority.Unchanged)
            {
                if (effects.OriginalPriority is { } original)
                {
                    process.PriorityClass = original;
                    effects.OriginalPriority = null;
                }
            }
            else
            {
                var requested = priority == NativeWindowBackgroundPriority.Idle ? ProcessPriorityClass.Idle : ProcessPriorityClass.BelowNormal;
                var previous = effects.OriginalPriority ?? process.PriorityClass;
                // An already-low priority process should never be promoted by a background rule.
                if (previous == ProcessPriorityClass.Idle)
                    requested = ProcessPriorityClass.Idle;
                process.PriorityClass = requested;
                effects.OriginalPriority = previous;
            }
        }
        catch (Exception e) when (e is InvalidOperationException or Win32Exception)
        {
            // A protected process or one that just exited can deny priority queries/changes.
        }

        if (mute || effects.OriginalMuteStates.Count != 0)
            WindowToolsAudio.SetProcessMute((uint)effects.Process.Id, mute, effects.OriginalMuteStates);
    }

    void RestoreProcessEffects()
    {
        foreach (var effects in processEffects.Values)
        {
            ApplyProcessEffects(effects, false, NativeWindowBackgroundPriority.Unchanged);
            effects.Process.Dispose();
        }
        processEffects.Clear();
    }

    void UpdateCursor(nint foreground)
    {
        if (!windows.TryGetValue(foreground, out var state) || !state.Options.ConfineCursor ||
            state.Wallpaper is not null || !Matches(foreground, state.Pid) || IsIconic(foreground) != 0 ||
            GetWindowRect(foreground, out var rect) == 0)
        {
            ReleaseCursor();
            return;
        }

        if (clippedWindow == foreground && clippedRect.Equals(rect))
            return;

        ReleaseCursor();
        if (ClipCursor(&rect) != 0)
        {
            clippedWindow = foreground;
            clippedRect = rect;
        }
    }

    void ReleaseCursor()
    {
        if (clippedWindow == 0)
            return;

        // A foreground game can replace our clip as it takes focus; preserve its new clip.
        if (GetClipCursor(out var current) != 0 && current.Equals(clippedRect))
            ClipCursor(null);
        clippedWindow = 0;
    }
}
#endif
