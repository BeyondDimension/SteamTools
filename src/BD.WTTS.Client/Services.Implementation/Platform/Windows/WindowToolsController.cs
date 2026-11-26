#if WINDOWS
using static BD.WTTS.Services.Implementation.WindowToolsNative;

namespace BD.WTTS.Services.Implementation;

sealed unsafe partial class WindowToolsController
{
    readonly object gate = new();
    readonly Dictionary<nint, WindowState> windows = new();
    readonly HashSet<(nint Window, uint Pid)> appliedRules = new();
    Dictionary<string, NativeWindowRule> rules = new(StringComparer.OrdinalIgnoreCase);
    Timer? ruleTimer;
    WindowToolsHooks? hooks;
    bool stopped;

    sealed class WindowState(nint handle, uint pid, uint originalStyle, uint originalExStyle, nint originalParent,
        nint originalOwner, in Placement originalPlacement, Rect originalRect, bool visible, LayerAttributes? originalLayer)
    {
        internal nint Handle { get; } = handle;
        internal uint Pid { get; } = pid;
        internal uint OriginalStyle { get; } = originalStyle;
        internal uint OriginalExStyle { get; } = originalExStyle;
        internal nint OriginalParent { get; } = originalParent;
        internal nint OriginalOwner { get; } = originalOwner;
        internal Placement OriginalPlacement { get; } = originalPlacement;
        internal Rect OriginalRect { get; } = originalRect;
        internal bool OriginallyVisible { get; } = visible;
        internal LayerAttributes? OriginalLayer { get; } = originalLayer;
        internal NativeWindowToolOptions Options { get; set; } = ReadOptions(originalExStyle, originalLayer);
        internal DesktopLayer? Wallpaper { get; set; }
        internal bool WallpaperPaused { get; set; }
        internal bool ForwardMouse { get; set; }
        internal bool ForwardKeyboard { get; set; }
        internal nint KeyboardTarget { get; set; }
    }

    readonly record struct LayerAttributes(uint Color, byte Alpha, uint Flags);

    internal WindowToolsController() => AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

    static LayerAttributes? ReadLayer(nint window, uint style) =>
        (style & Layered) != 0 && GetLayeredWindowAttributes(window, out var color, out var alpha, out var flags) != 0
            ? new(color, alpha, flags) : null;

    static NativeWindowToolOptions ReadOptions(uint style, LayerAttributes? attributes) => new(
        Topmost: (style & Topmost) != 0,
        Opacity: attributes is { Flags: var flags, Alpha: var alpha } && (flags & 2) != 0
            ? Math.Clamp((int)Math.Round(alpha * (100.0 / 255)), 20, 100) : 100,
        ClickThrough: (style & (Layered | Transparent)) == (Layered | Transparent));

    static bool Matches(nint window, uint pid) => pid != 0 &&
        GetWindowThreadProcessId(window, out var actualPid) != 0 && pid == actualPid;

    static bool TryIdentify(NativeWindowModel window, out uint pid)
    {
        pid = 0;
        if (window.Process is not { } process || !Matches(window.Handle, (uint)process.Id) || process.Id == Environment.ProcessId)
            return false;

        pid = (uint)process.Id;
        return true;
    }

    WindowState? Track(nint handle, uint pid)
    {
        if (stopped || !Matches(handle, pid))
            return null;
        if (windows.TryGetValue(handle, out var current))
        {
            if (current.Pid == pid)
                return current;
            windows.Remove(handle);
        }

        var state = CaptureState(handle, pid);
        if (state is null)
            return null;

        windows.Add(handle, state);
        UpdateHooks();
        return state;
    }

    static WindowState? CaptureState(nint handle, uint pid)
    {
        var placement = new Placement { Size = (uint)sizeof(Placement) };
        if (GetWindowPlacement(handle, ref placement) == 0 || GetWindowRect(handle, out var rect) == 0)
            return null;

        var style = GetWindowLong(handle, Style);
        var extended = GetWindowLong(handle, ExStyle);
        var parent = (style & Child) != 0 ? GetAncestor(handle, 1) : 0;
        var owner = (style & Child) == 0 ? GetWindow(handle, 4) : 0;
        return new WindowState(handle, pid, style, extended, parent, owner, placement, rect,
            IsWindowVisible(handle) != 0, ReadLayer(handle, extended));
    }

    internal static IReadOnlyList<NativeWindowMonitor> GetMonitors()
    {
        using var dpi = new DpiScope();
        var monitors = new List<NativeWindowMonitor>();
        EnumDisplayMonitors(0, null, (monitor, _, _, _) =>
        {
            var info = new MonitorInfo { Size = (uint)sizeof(MonitorInfo) };
            if (GetMonitorInfo(monitor, ref info) != 0)
            {
                var rect = info.Monitor;
                monitors.Add(new(new string(info.Device), (info.Flags & 1) != 0, rect.Left, rect.Top, rect.Width, rect.Height));
            }
            return 1;
        }, 0);
        return monitors.OrderByDescending(monitor => monitor.IsPrimary).ThenBy(monitor => monitor.DeviceName).ToArray();
    }

    internal NativeWindowToolInfo? GetInfo(NativeWindowModel window)
    {
        lock (gate)
        {
            using var dpi = new DpiScope();
            var handle = window.Handle;
            if (!TryIdentify(window, out var pid) || GetWindowRect(handle, out var rect) == 0)
                return null;

            windows.TryGetValue(handle, out var state);
            if (state?.Pid != pid)
                state = null;
            var exStyle = GetWindowLong(handle, ExStyle);
            return new(new(rect.Left, rect.Top, rect.Width, rect.Height),
                state?.Options ?? ReadOptions(exStyle, ReadLayer(handle, exStyle)),
                (GetWindowLong(handle, Style) & Frame) == 0, state?.Wallpaper is not null, state?.WallpaperPaused == true);
        }
    }

    bool Apply(NativeWindowModel window, Func<WindowState, bool> action)
    {
        lock (gate)
        {
            using var dpi = new DpiScope();
            return TryIdentify(window, out var pid) && Track(window.Handle, pid) is { } state && action(state);
        }
    }

    internal bool Borderless(NativeWindowModel window) => Apply(window, SetBorderless);

    internal bool Show(NativeWindowModel window, int command) => Apply(window, state => ShowWindowAsync(state.Handle, command) != 0);

    internal bool SetBounds(NativeWindowModel window, NativeWindowBounds bounds) =>
        Apply(window, state => Resize(state, bounds));

    internal bool SetOptions(NativeWindowModel window, NativeWindowToolOptions options) =>
        Apply(window, state => SetOptions(state, options));

    internal bool SetClickThrough(NativeWindowModel window, bool enabled) =>
        Apply(window, state => SetOptions(state, state.Options with { ClickThrough = enabled }));

    internal bool MoveToMonitor(NativeWindowModel window, NativeWindowMonitor monitor, NativeWindowMonitorAction action) =>
        Apply(window, state => MoveToMonitor(state, ResolveMonitor(monitor.DeviceName), action));

    static NativeWindowMonitor? ResolveMonitor(string? name)
    {
        var monitors = GetMonitors();
        return monitors.FirstOrDefault(monitor => string.Equals(monitor.DeviceName, name, StringComparison.OrdinalIgnoreCase))
            ?? monitors.FirstOrDefault();
    }

    static bool SetBorderless(WindowState state)
    {
        var window = state.Handle;
        return TrySetStyle(window, Style, GetWindowLong(window, Style) & ~Frame) &&
            SetWindowPos(window, 0, 0, 0, 0, 0, FrameChanged | NoMove | NoSize | NoActivate | NoZOrder) != 0;
    }

    static bool MoveToMonitor(WindowState state, NativeWindowMonitor? monitor, NativeWindowMonitorAction action)
    {
        if (monitor is null || GetWindowRect(state.Handle, out var rect) == 0)
            return false;
        if (action == NativeWindowMonitorAction.Fit)
            return SetBorderless(state) && Resize(state, MonitorBounds(monitor));

        // Center retains size. Move keeps the offset on the originating monitor when it fits.
        var source = GetMonitors().OrderByDescending(display => OverlapArea(display, rect)).FirstOrDefault();
        var x = action == NativeWindowMonitorAction.Move && source is not null
            ? Math.Clamp(rect.Left - source.X, 0, Math.Max(0, monitor.Width - rect.Width))
            : (monitor.Width - rect.Width) / 2;
        var y = action == NativeWindowMonitorAction.Move && source is not null
            ? Math.Clamp(rect.Top - source.Y, 0, Math.Max(0, monitor.Height - rect.Height))
            : (monitor.Height - rect.Height) / 2;
        return Resize(state, new(monitor.X + x, monitor.Y + y, rect.Width, rect.Height));
    }

    static long OverlapArea(NativeWindowMonitor monitor, Rect rect) =>
        (long)Math.Max(0, Math.Min(rect.Right, monitor.X + monitor.Width) - Math.Max(rect.Left, monitor.X)) *
        Math.Max(0, Math.Min(rect.Bottom, monitor.Y + monitor.Height) - Math.Max(rect.Top, monitor.Y));

    static NativeWindowBounds MonitorBounds(NativeWindowMonitor monitor) => new(monitor.X, monitor.Y, monitor.Width, monitor.Height);

    static bool Resize(WindowState state, NativeWindowBounds bounds)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return false;

        if ((IsIconic(state.Handle) != 0 || IsZoomed(state.Handle) != 0) && !RestoreSize(state.Handle))
            return false;

        var parent = GetActualParent(state.Handle);
        return Position(state.Handle, parent, bounds, 0, NoZOrder | NoActivate | FrameChanged);
    }

    static bool RestoreSize(nint window)
    {
        var placement = new Placement { Size = (uint)sizeof(Placement) };
        if (GetWindowPlacement(window, ref placement) == 0)
            return false;

        placement.Flags = 0;
        placement.ShowCommand = 4; // SW_SHOWNOACTIVATE restores the normal show state.
        return SetWindowPlacement(window, in placement) != 0;
    }

    static nint GetActualParent(nint window) => (GetWindowLong(window, Style) & Child) != 0 ? GetAncestor(window, 1) : 0;

    static bool Position(nint window, nint parent, NativeWindowBounds bounds, nint after, uint flags)
    {
        var location = new Point { X = bounds.X, Y = bounds.Y };
        if (parent != 0)
            MapWindowPoints(0, parent, ref location, 1);
        return SetWindowPos(window, after, location.X, location.Y, bounds.Width, bounds.Height, flags) != 0;
    }

    static bool TrySetStyle(nint window, int index, uint value) =>
        GetWindowLong(window, index) == value || SetWindowLong(window, index, value) != 0 || Marshal.GetLastPInvokeError() == 0;

    static bool TryReparent(nint window, nint parent) =>
        SetParent(window, parent) != 0 || Marshal.GetLastPInvokeError() == 0;

    static bool SetOwner(nint window, nint owner) => IntPtr.Size == 8
        ? SetWindowLongPtr64(window, Owner, owner) != 0 || Marshal.GetLastPInvokeError() == 0
        : SetWindowLong(window, Owner, unchecked((uint)owner)) != 0 || Marshal.GetLastPInvokeError() == 0;

    bool SetOptions(WindowState state, NativeWindowToolOptions options)
    {
        options = options with { Opacity = Math.Clamp(options.Opacity, 20, 100) };
        if (!Enum.IsDefined(options.BackgroundPriority))
            options = options with { BackgroundPriority = NativeWindowBackgroundPriority.Unchanged };

        var old = state.Options;
        var window = state.Handle;
        var interacting = wallpaperInteraction?.State == state;
        if ((old.Opacity != options.Opacity || old.ClickThrough != options.ClickThrough) &&
            !ApplyLayer(state, options, !interacting && state.Wallpaper?.UsesLayeredChild == true))
            return false;

        if (old.Topmost != options.Topmost && state.Wallpaper is null &&
            SetWindowPos(window, options.Topmost ? -1 : -2, 0, 0, 0, 0, NoMove | NoSize | NoActivate) == 0)
        {
            ApplyLayer(state, old, false);
            return false;
        }

        state.Options = options;
        if (interacting && options.ClickThrough)
            EndWallpaperInteraction();
        if (old.ConfineCursor != options.ConfineCursor || old.MuteInBackground != options.MuteInBackground ||
            old.BackgroundPriority != options.BackgroundPriority)
        {
            UpdateHooks();
            UpdateFocusBehaviors();
        }
        return true;
    }

    static bool ApplyLayer(WindowState state, NativeWindowToolOptions options, bool layeredDesktop)
    {
        var window = state.Handle;
        var original = GetWindowLong(window, ExStyle);
        var newStyle = original & ~(Layered | Transparent);
        var opacityChanged = options.Opacity != ReadOptions(state.OriginalExStyle, state.OriginalLayer).Opacity;
        var needsAlpha = layeredDesktop || opacityChanged || options.ClickThrough;
        // On the raised desktop, Explorer keeps native input. Wallpaper interaction
        // is routed explicitly by the desktop hit test and the user's forwarding settings.
        if (options.ClickThrough || layeredDesktop || (state.OriginalExStyle & (Layered | Transparent)) == Transparent)
            newStyle |= Transparent;
        if (needsAlpha || (state.OriginalExStyle & Layered) != 0)
            newStyle |= Layered;
        // An existing per-pixel layered window cannot safely switch to constant-alpha rendering.
        if (needsAlpha && (state.OriginalExStyle & Layered) != 0 && state.OriginalLayer is null)
            return false;
        if (!TrySetStyle(window, ExStyle, newStyle))
            return false;

        var attributes = state.OriginalLayer;
        if (needsAlpha)
        {
            var alpha = opacityChanged ? (byte)Math.Round(options.Opacity * (255.0 / 100)) : attributes?.Alpha ?? 255;
            attributes = new(attributes?.Color ?? 0, alpha, (attributes?.Flags ?? 0) | 2);
        }
        if (attributes is null || SetLayeredWindowAttributes(window, attributes.Value.Color, attributes.Value.Alpha, attributes.Value.Flags) != 0)
            return true;

        TrySetStyle(window, ExStyle, original);
        return false;
    }

    internal bool Restore(NativeWindowModel window)
    {
        lock (gate)
        {
            using var dpi = new DpiScope();
            if (!TryIdentify(window, out var pid))
                return false;
            if (!windows.TryGetValue(window.Handle, out var state) || state.Pid != pid)
                return ShowWindowAsync(window.Handle, 9) != 0;

            var result = RestoreState(state);
            UpdateHooks();
            UpdateFocusBehaviors();
            return result;
        }
    }

    bool RestoreState(WindowState state)
    {
        if (mouseWindow == state)
            ReleaseForwardedMouse(cancel: true);
        if (keyboardWindow == state)
            keyboardWindow = null;
        if (!RestoreNative(state))
            return false;

        ClearWallpaperInteraction(state);
        windows.Remove(state.Handle);
        return true;
    }

    static bool RestoreNative(WindowState state)
    {
        var handle = state.Handle;
        if (Matches(handle, state.Pid))
        {
            var parent = IsWindow(state.OriginalParent) != 0 ? state.OriginalParent : 0;
            var style = parent == 0 && (state.OriginalStyle & Child) != 0
                ? (state.OriginalStyle & ~Child) | Popup : state.OriginalStyle;
            if ((parent != 0 && !TrySetStyle(handle, Style, style)) ||
                (GetActualParent(handle) != parent && !TryReparent(handle, parent)) ||
                !TrySetStyle(handle, Style, style) || !TrySetStyle(handle, ExStyle, state.OriginalExStyle))
                return false;

            if (parent == 0 && GetWindow(handle, 4) != state.OriginalOwner)
                SetOwner(handle, IsWindow(state.OriginalOwner) != 0 ? state.OriginalOwner : 0);

            if (state.OriginalLayer is { } attributes)
                SetLayeredWindowAttributes(handle, attributes.Color, attributes.Alpha, attributes.Flags);

            var rect = state.OriginalRect;
            var positionOk = Position(handle, parent, new(rect.Left, rect.Top, rect.Width, rect.Height),
                (state.OriginalExStyle & Topmost) != 0 ? -1 : -2,
                FrameChanged | NoActivate | (parent != 0 ? NoZOrder : 0));
            var placement = state.OriginalPlacement;
            if (!state.OriginallyVisible)
                placement.ShowCommand = 0;
            if (SetWindowPlacement(handle, in placement) == 0 || !positionOk)
                return false;
        }

        return true;
    }

    internal void SetRules(IReadOnlyList<NativeWindowRule> newRules)
    {
        lock (gate)
        {
            if (stopped)
                return;
            rules = new(StringComparer.OrdinalIgnoreCase);
            foreach (var rule in newRules.Where(rule => rule.AutoApply && !string.IsNullOrWhiteSpace(rule.ProcessName)))
                rules[rule.ProcessName] = rule;

            if (rules.Count != 0)
                ruleTimer ??= new Timer(WatchRules, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
            else
            {
                ruleTimer?.Dispose();
                ruleTimer = null;
                appliedRules.Clear();
            }
            UpdateHooks();
        }
    }

    void WatchRules(object? state)
    {
        // A slow external window should not stack timer callbacks.
        if (!Monitor.TryEnter(gate))
            return;
        try
        {
            if (stopped || rules.Count == 0)
                return;

            using var dpi = new DpiScope();
            appliedRules.RemoveWhere(key => !Matches(key.Window, key.Pid));
            var processRules = new Dictionary<uint, NativeWindowRule?>();
            EnumWindows((handle, _) =>
            {
                if (IsWindowVisible(handle) == 0 || GetWindow(handle, 4) != 0 || GetWindowTextLength(handle) == 0 ||
                    (GetWindowLong(handle, ExStyle) & 0x80) != 0 ||
                    GetWindowThreadProcessId(handle, out var pid) == 0 || pid == Environment.ProcessId ||
                    appliedRules.Contains((handle, pid)))
                    return 1;

                if (!processRules.TryGetValue(pid, out var rule))
                {
                    try
                    {
                        using var process = Process.GetProcessById((int)pid);
                        rules.TryGetValue(process.ProcessName, out rule);
                    }
                    catch (Exception e) when (e is ArgumentException or InvalidOperationException or Win32Exception)
                    {
                        // The process can exit between EnumWindows and opening its handle.
                    }
                    processRules.Add(pid, rule);
                }

                if (rule is not null && Track(handle, pid) is { } window)
                {
                    appliedRules.Add((handle, pid)); // Apply once per window, including after a manual restore.
                    SetOptions(window, window.Options with
                    {
                        Topmost = rule.Topmost,
                        Opacity = rule.Opacity,
                        MuteInBackground = rule.MuteInBackground,
                        BackgroundPriority = rule.BackgroundPriority,
                    });
                    if (rule.FitMonitor is { } device)
                        MoveToMonitor(window, ResolveMonitor(device), NativeWindowMonitorAction.Fit);
                    else if (rule.Borderless)
                        SetBorderless(window);
                }
                return 1;
            }, 0);
        }
        finally
        {
            Monitor.Exit(gate);
        }
    }

    void UpdateHooks()
    {
        if (windows.Count != 0)
            hooks ??= new(OnWindowEvent, ForwardInput);
        if (hooks is null)
            return;

        var desktopHosts = new HashSet<nint>();
        var watchedWindows = new HashSet<nint>();
        var mouse = false;
        var foreground = false;
        foreach (var state in windows.Values)
        {
            watchedWindows.Add(state.Handle);
            foreground |= state.Options.ConfineCursor || state.Options.MuteInBackground ||
                state.Options.BackgroundPriority != NativeWindowBackgroundPriority.Unchanged;
            if (state.Wallpaper is not { } layer)
                continue;
            watchedWindows.Add(layer.Parent);
            watchedWindows.Add(layer.IconView);
            mouse |= state.ForwardMouse || state.ForwardKeyboard;
            if (state.ForwardKeyboard)
            {
                foreground = true;
                desktopHosts.Add(layer.Progman);
                desktopHosts.Add(layer.IconHost);
                desktopHosts.Add(layer.Parent);
            }
        }
        hooks.Configure(mouse, desktopHosts.ToArray(), watchedWindows.ToArray(), foreground);
    }

    void OnWindowEvent(uint @event, nint handle)
    {
        lock (gate)
        {
            if (stopped)
                return;

            using var dpi = new DpiScope();
            if (@event == WindowToolsHooks.WindowDestroyed)
            {
                appliedRules.RemoveWhere(item => item.Window == handle);
                if (windows.Remove(handle, out var removed))
                {
                    ClearWallpaperInteraction(removed);
                    if (mouseWindow == removed)
                        ReleaseForwardedMouse(cancel: false);
                    if (keyboardWindow == removed)
                        keyboardWindow = null;
                }
                foreach (var state in windows.Values.ToArray())
                {
                    if (state.Wallpaper is { } layer && (handle == layer.Parent || handle == layer.IconView))
                        RestoreState(state);
                }
                UpdateHooks();
            }
            UpdateFocusBehaviors();
        }
    }

    void OnProcessExit(object? sender, EventArgs e) => ShutDown();

    internal void ShutDown()
    {
        lock (gate)
        {
            if (stopped)
                return;

            stopped = true;
            AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
            ruleTimer?.Dispose();
            hooks?.Dispose();
            using var dpi = new DpiScope();
            ReleaseCursor();
            RestoreProcessEffects();
            foreach (var state in windows.Values.ToArray())
                RestoreState(state);
        }
    }
}
#endif
