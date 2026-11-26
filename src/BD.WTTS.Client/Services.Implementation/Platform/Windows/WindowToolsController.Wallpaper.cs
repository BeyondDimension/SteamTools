#if WINDOWS
using static BD.WTTS.Services.Implementation.WindowToolsNative;

namespace BD.WTTS.Services.Implementation;

unsafe partial class WindowToolsController
{
    sealed record DesktopLayer(nint Progman, nint Parent, nint IconHost, nint IconView, nint Background, bool UsesLayeredChild)
    {
        internal bool IsDesktopForeground(nint window) =>
            window != 0 && (window == Progman || window == IconHost || window == Parent);
    }

    readonly record struct Click(nint Child, Point Point, uint Message, uint Data, uint Time);

    static readonly Guid AccessibleIid = new("618736E0-3C3D-11CF-810C-00AA00389B71");
    WindowState? keyboardWindow;
    WindowState? mouseWindow;
    nint mouseTarget;
    Click lastClick;

    static DesktopLayer? FindDesktopLayer()
    {
        var progman = FindWindow("Progman", null);
        if (progman == 0)
            return null;

        // Ask Explorer to create its wallpaper layer. Never wait indefinitely for Explorer.
        SendMessageTimeout(progman, 0x052C, 0xD, 1, 2, 150, out _);

        // On Windows 11 24H2 and later, Progman can host both the layered icon view and
        // the wallpaper WorkerW. Insert a layered child between them, just below DefView.
        var childView = FindWindowEx(progman, 0, "SHELLDLL_DefView", null);
        var childWorker = FindWindowEx(progman, 0, "WorkerW", null);
        if (childView != 0 && ((GetWindowLong(progman, ExStyle) & NoRedirectionBitmap) != 0 || childWorker != 0))
        {
            // A shell rebuild can leave the background in front of DefView. Keep
            // Explorer's own wallpaper behind the icon view before inserting ours.
            if (childWorker != 0 && FindWindowEx(progman, childView, "WorkerW", null) != childWorker &&
                SetWindowPos(childWorker, 1, 0, 0, 0, 0, NoMove | NoSize | NoActivate) == 0)
                return null;
            return new(progman, progman, progman, childView, childWorker, true);
        }

        // On the classic desktop, the icon view belongs to a top-level window.
        // The separate, empty WorkerW immediately behind that host is our parent.
        DesktopLayer? layer = null;
        EnumWindows((host, _) =>
        {
            var view = FindWindowEx(host, 0, "SHELLDLL_DefView", null);
            if (view == 0)
                return 1;

            var worker = FindWindowEx(0, host, "WorkerW", null);
            if (worker == 0 || FindWindowEx(worker, 0, "SHELLDLL_DefView", null) != 0 ||
                GetWindowThreadProcessId(worker, out var workerPid) == 0 ||
                GetWindowThreadProcessId(host, out var hostPid) == 0 || workerPid != hostPid)
                return 1;

            layer = new(progman, worker, host, view, worker, false);
            return 0;
        }, 0);
        return layer;
    }

    internal bool SetWallpaper(NativeWindowModel window, NativeWindowMonitor monitor, bool mouse, bool keyboard) =>
        Apply(window, state =>
        {
            var display = ResolveMonitor(monitor.DeviceName);
            var layer = state.Wallpaper ?? FindDesktopLayer();
            if (layer is null || display is null || IsWindow(layer.Parent) == 0 || IsWindow(layer.IconView) == 0)
                return false;

            var handle = state.Handle;
            var previous = CaptureState(handle, state.Pid);
            if (previous is null)
                return false;

            var style = (previous.OriginalStyle & ~(Popup | Frame | 0x21000000)) | Child;
            if (((IsIconic(handle) != 0 || IsZoomed(handle) != 0) && !RestoreSize(handle)) ||
                (previous.Options.Topmost && SetWindowPos(handle, -2, 0, 0, 0, 0, NoMove | NoSize | NoActivate) == 0) ||
                !TrySetStyle(handle, Style, style) ||
                (previous.OriginalParent != layer.Parent && !TryReparent(handle, layer.Parent)) ||
                !ApplyLayer(state, state.Options, layer.UsesLayeredChild) ||
                !Position(handle, layer.Parent, MonitorBounds(display), layer.UsesLayeredChild ? layer.IconView : 0,
                    FrameChanged | NoActivate | 0x40))
            {
                RestoreNative(previous);
                return false;
            }

            state.Wallpaper = layer;
            state.WallpaperPaused = false;
            state.ForwardMouse = mouse;
            state.ForwardKeyboard = keyboard;
            UpdateHooks();
            UpdateFocusBehaviors();
            return true;
        });

    internal bool ChangeWallpaperInput(NativeWindowModel window, bool mouse, bool keyboard)
    {
        lock (gate)
        {
            if (!TryIdentify(window, out var pid) || !windows.TryGetValue(window.Handle, out var state) || state.Pid != pid)
                return false;

            state.ForwardMouse = mouse;
            state.ForwardKeyboard = keyboard;
            if (!mouse && mouseWindow == state)
                ReleaseForwardedMouse(cancel: true);
            if (!keyboard && keyboardWindow == state)
                keyboardWindow = null;
            UpdateHooks();
            return true;
        }
    }

    internal bool PauseWallpaper(NativeWindowModel window) => Apply(window, state =>
    {
        if (state.Wallpaper is null || CaptureState(state.Handle, state.Pid) is not { } previous)
            return false;

        var handle = state.Handle;
        var style = (GetWindowLong(handle, Style) & ~Child) | Popup;
        var rect = previous.OriginalRect;
        if (!TryReparent(handle, 0) || !TrySetStyle(handle, Style, style) ||
            !ApplyLayer(state, state.Options, false) ||
            !Position(handle, 0, new(rect.Left, rect.Top, rect.Width, rect.Height), state.Options.Topmost ? -1 : -2, FrameChanged | NoActivate))
        {
            RestoreNative(previous);
            return false;
        }

        state.Wallpaper = null;
        state.WallpaperPaused = true;
        if (mouseWindow == state)
            ReleaseForwardedMouse(cancel: true);
        if (keyboardWindow == state)
            keyboardWindow = null;
        UpdateHooks();
        UpdateFocusBehaviors();
        return true;
    });

    internal void ResetWallpaper()
    {
        lock (gate)
        {
            using var dpi = new DpiScope();
            foreach (var state in windows.Values.Where(state => state.Wallpaper is not null || state.WallpaperPaused).ToArray())
            {
                state.ForwardMouse = false;
                state.ForwardKeyboard = false;
                RestoreState(state);
            }

            keyboardWindow = null;
            UpdateHooks();
            UpdateFocusBehaviors();
        }
    }

    void ForwardInput(WindowToolsHooks.Input input)
    {
        using var dpi = new DpiScope();
        if (!input.IsMouse)
        {
            lock (gate)
                ForwardKey(input);
            return;
        }

        DesktopLayer? layer;
        lock (gate)
            layer = stopped ? null : windows.Values.FirstOrDefault(state => state.Wallpaper is not null)?.Wallpaper;
        if (layer is null)
            return;

        var hit = WindowFromPoint(input.Point);
        // Accessible hit-testing can call into Explorer. Keep it off the hook thread
        // and outside the controller lock so a stalled shell cannot hold up cleanup.
        var bareDesktop = IsBareDesktop(layer, hit, input.Point);
        lock (gate)
        {
            if (stopped)
                return;

            var hitRoot = GetAncestor(hit, 2);
            var state = WindowAtPoint(layer, input.Point);
            var hitTarget = state is not null && (hit == state.Handle || IsChild(state.Handle, hit) != 0);
            var down = input.Message is 0x201 or 0x204 or 0x207 or 0x20B;
            if (down)
            {
                keyboardWindow = null;
                if (state is not null && !state.Options.ClickThrough && state.ForwardKeyboard && (bareDesktop || hitTarget))
                {
                    keyboardWindow = state;
                    state.KeyboardTarget = FindChildAtPoint(state.Handle, input.Point);
                }
            }

            // Shell chrome and app windows occlude the wallpaper. If Windows already
            // delivers to the wallpaper directly (for example with icons hidden), avoid duplication.
            if (!bareDesktop || hitTarget || state is null || !state.ForwardMouse || state.Options.ClickThrough ||
                WindowFromPoint(input.Point) != hit || !layer.IsDesktopForeground(hitRoot))
            {
                lastClick = default;
                ReleaseForwardedMouse(cancel: true);
                return;
            }

            if (mouseWindow != state || (mouseTarget != state.Handle && IsChild(state.Handle, mouseTarget) == 0))
                ReleaseForwardedMouse(cancel: true);
            var child = mouseTarget != 0 ? mouseTarget : FindChildAtPoint(state.Handle, input.Point);
            var message = DoubleClickMessage(input, child, down);
            var point = input.Point;
            if (message is not (0x20A or 0x20E)) // WM_MOUSEWHEEL and WM_MOUSEHWHEEL take screen coordinates.
                MapWindowPoints(0, child, ref point, 1);
            var data = message is >= 0x20A and <= 0x20E ? input.Data & 0xFFFF0000 : 0;
            if (PostMessage(child, message, data | input.MouseKeys, PackPoint(point)) != 0)
            {
                if (down)
                {
                    mouseWindow = state;
                    mouseTarget = child;
                }
                else if ((input.MouseKeys & 0x73) == 0) // MK_LBUTTON, MK_RBUTTON, MK_MBUTTON, MK_XBUTTON1/2.
                {
                    ReleaseForwardedMouse(cancel: false);
                }
            }
        }
    }

    void ReleaseForwardedMouse(bool cancel)
    {
        if (cancel && mouseWindow is { } state && Matches(state.Handle, state.Pid) &&
            (mouseTarget == state.Handle || IsChild(state.Handle, mouseTarget) != 0))
            PostMessage(mouseTarget, 0x001F, 0, 0); // WM_CANCELMODE releases a drag when desktop hit-testing stops it.
        mouseWindow = null;
        mouseTarget = 0;
    }

    WindowState? WindowAtPoint(DesktopLayer layer, Point point)
    {
        // Read the desktop parent's actual sibling Z-order when multiple wallpaper windows overlap.
        var child = GetWindow(layer.Parent, 5);
        for (var i = 0; i < 512 && child != 0; i++, child = GetWindow(child, 2))
        {
            if (windows.TryGetValue(child, out var state) && state.Wallpaper == layer && Matches(child, state.Pid) &&
                IsWindowVisible(child) != 0 && GetWindowRect(child, out var rect) != 0 && rect.Contains(point))
                return state;
        }

        return null;
    }

    static bool IsBareDesktop(DesktopLayer layer, nint hit, Point point)
    {
        if (hit == 0 || (hit != layer.IconView && hit != layer.Parent && hit != layer.IconHost && hit != layer.Progman && hit != layer.Background &&
            IsChild(layer.IconView, hit) == 0))
            return false;

        var list = FindWindowEx(layer.IconView, 0, "SysListView32", null);
        if (list == 0 || IsWindowVisible(list) == 0)
            return hit == layer.IconView || hit == layer.Background || layer.IsDesktopForeground(hit);
        if (hit != list && IsChild(layer.IconView, hit) != 0)
            return false;
        if (GetWindowRect(list, out var rect) == 0)
            return false;
        return !rect.Contains(point) || IsBareListView(list, point);
    }

    static bool IsBareListView(nint list, Point point)
    {
        // LVM_HITTEST's pointer is not marshalled across processes. MSAA performs the
        // SysListView32 hit test via the shell's existing accessibility proxy.
        using var apartment = new ComScope();
        if (!apartment.IsReady ||
            AccessibleObjectFromWindow(list, unchecked((uint)-4), in AccessibleIid, out var accessible) < 0 || accessible == 0)
            return false;
        var hit = new Variant();
        try
        {
            var table = *(nint**)accessible;
            var accHitTest = (delegate* unmanaged[Stdcall]<nint, int, int, Variant*, int>)table[24];
            return accHitTest(accessible, point.X, point.Y, &hit) == 0 && hit.Type == 3 && (int)hit.Value == 0;
        }
        finally
        {
            VariantClear(ref hit);
            Marshal.Release(accessible);
        }
    }

    static nint FindChildAtPoint(nint window, Point point)
    {
        // ChildWindowFromPointEx searches one level. Walk down the client hierarchy.
        for (var depth = 0; depth < 32; depth++)
        {
            var local = point;
            MapWindowPoints(0, window, ref local, 1);
            var child = ChildWindowFromPointEx(window, local, 7);
            if (child == 0 || child == window)
                break;
            window = child;
        }
        return window;
    }

    uint DoubleClickMessage(WindowToolsHooks.Input input, nint child, bool down)
    {
        if (!down)
            return input.Message;

        var last = lastClick;
        if (last.Child == child && last.Message == input.Message && last.Data == input.Data &&
            unchecked(input.Time - last.Time) <= GetDoubleClickTime() &&
            Math.Abs(input.Point.X - last.Point.X) <= GetSystemMetrics(36) / 2 &&
            Math.Abs(input.Point.Y - last.Point.Y) <= GetSystemMetrics(37) / 2 &&
            (GetClassLong(child, -26) & 8) != 0)
        {
            lastClick = default;
            return input.Message + 2;
        }
        lastClick = new(child, input.Point, input.Message, input.Data, input.Time);
        return input.Message;
    }

    static nint PackPoint(Point point) => unchecked((int)((uint)(ushort)point.X | ((uint)(ushort)point.Y << 16)));

    void ForwardKey(WindowToolsHooks.Input input)
    {
        if (stopped || keyboardWindow is not { Wallpaper: { } layer, ForwardKeyboard: true } state ||
            state.Options.ClickThrough || !layer.IsDesktopForeground(input.Foreground) ||
            GetForegroundWindow() != input.Foreground || IsWindowVisible(state.Handle) == 0 ||
            !Matches(state.Handle, state.Pid))
            return;

        var thread = GetWindowThreadProcessId(state.Handle, out _);
        var info = new GuiThreadInfo { Size = (uint)sizeof(GuiThreadInfo) };
        var target = GetGUIThreadInfo(thread, ref info) != 0 &&
            (info.Focus == state.Handle || IsChild(state.Handle, info.Focus) != 0) ? info.Focus : state.KeyboardTarget;
        if (target != state.Handle && IsChild(state.Handle, target) == 0)
            target = state.Handle;

        var keyBits = 1u | ((input.ScanCode & 0xFF) << 16) | ((input.Flags & 1) << 24);
        if (input.Message == 0x101)
            keyBits |= 0xC0000000;
        var virtualKey = input.Data switch
        {
            0xA0 or 0xA1 => 0x10u, // VK_LSHIFT/VK_RSHIFT -> VK_SHIFT
            0xA2 or 0xA3 => 0x11u, // VK_LCONTROL/VK_RCONTROL -> VK_CONTROL
            _ => input.Data,
        };
        PostMessage(target, input.Message, virtualKey, unchecked((int)keyBits));
    }
}
#endif
