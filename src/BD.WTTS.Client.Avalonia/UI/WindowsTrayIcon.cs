#if WINDOWS
using System.Runtime.CompilerServices;
using DrawingIcon = System.Drawing.Icon;

namespace BD.WTTS.UI;

/// <summary>Owns the Windows notification icon and its independent message window on the UI thread.</summary>
internal sealed class WindowsTrayIcon : IDisposable
{
    internal const uint CallbackMessage = 0x8001; // WM_APP + 1

    const uint IconId = 1;
    const uint NimAdd = 0;
    const uint NimModify = 1;
    const uint NimDelete = 2;
    const uint NimSetFocus = 3;
    const uint NimSetVersion = 4;
    const uint NifMessage = 0x01;
    const uint NifIcon = 0x02;
    const uint NifTip = 0x04;
    const uint NifShowTip = 0x80;
    const uint WmClose = 0x0010;
    const uint WmNcDestroy = 0x0082;
    const uint WmContextMenu = 0x007B;
    const uint WmLButtonUp = 0x0202;
    const uint WmRButtonUp = 0x0205;
    const uint NinSelect = 0x0400;
    const uint NinKeySelect = 0x0401;
    const uint WsPopup = 0x80000000;
    const uint WsExToolWindow = 0x80;
    const uint MsgfltAllow = 1;
    const int SmCxSmIcon = 49;
    const int SmCySmIcon = 50;
    const int ToolTipCapacity = 128;

    static readonly SubclassProc windowProc = WindowProc;
    static readonly uint notifyIconDataSize = (uint)Unsafe.SizeOf<NotifyIconData>();

    readonly Dispatcher dispatcher;
    readonly string toolTip;
    readonly Action activate;
    readonly Action<PixelPoint> showMenu;
    DrawingIcon? icon;
    nint window;
    uint taskbarCreated;
    bool registered;
    bool version4;
    bool registering;
    bool insideCallback;
    bool disposed;

    /// <summary>The UI-thread-owned callback HWND, or zero after it is destroyed.</summary>
    internal nint MessageWindow => window;

    WindowsTrayIcon(Dispatcher dispatcher, string toolTip, Action activate, Action<PixelPoint> showMenu)
    {
        this.dispatcher = dispatcher;
        this.toolTip = toolTip;
        this.activate = activate;
        this.showMenu = showMenu;
    }

    /// <summary>Creates the tray on the UI thread, without taking ownership of the ICO stream.</summary>
    internal static WindowsTrayIcon? TryCreate(Stream iconStream, string toolTip, Action activate, Action<PixelPoint> showMenu)
    {
        var dispatcher = Dispatcher.UIThread;
        dispatcher.VerifyAccess();
        var tray = new WindowsTrayIcon(dispatcher, toolTip, activate, showMenu);
        var created = false;
        try
        {
            tray.Initialize(iconStream);
            created = tray.RegisterIcon();
            return created ? tray : null;
        }
        catch (Exception ex)
        {
            ReportError(ex, "Unable to create the Windows notification icon.");
            return null;
        }
        finally
        {
            if (!created)
                tray.Dispose();
        }
    }

    void Initialize(Stream iconStream)
    {
        icon = new DrawingIcon(iconStream, GetSystemMetrics(SmCxSmIcon), GetSystemMetrics(SmCySmIcon));
        taskbarCreated = RegisterWindowMessageW("TaskbarCreated");
        if (taskbarCreated == 0)
            throw new Win32Exception(Marshal.GetLastPInvokeError());

        // TaskbarCreated is broadcast to top-level windows, not message-only windows.
        window = CreateWindowExW(WsExToolWindow, "STATIC", 0, WsPopup, 0, 0, 0, 0, 0, 0, 0, 0);
        if (window == 0)
            throw new Win32Exception(Marshal.GetLastPInvokeError());

        // The subclass owns this root until WM_NCDESTROY, even if the caller drops its reference.
        var root = GCHandle.Alloc(this);
        var attached = false;
        try
        {
            attached = SetWindowSubclass(window, windowProc, 0, (nuint)GCHandle.ToIntPtr(root)) != 0;
            if (!attached)
                throw new InvalidOperationException("SetWindowSubclass failed for the notification icon window.");
        }
        finally
        {
            if (!attached)
                root.Free();
        }

        // Explorer normally runs at medium integrity even when this app is elevated.
        // Allow only the two pointer-free tray messages, and only on this HWND.
        _ = ChangeWindowMessageFilterEx(window, taskbarCreated, MsgfltAllow, 0);
        _ = ChangeWindowMessageFilterEx(window, CallbackMessage, MsgfltAllow, 0);
        dispatcher.ShutdownStarted += OnShutdownStarted;
    }

    bool RegisterIcon(bool recovering = false)
    {
        if (disposed || registering || window == 0 || icon is not { } ownedIcon)
            return false;

        registering = true;
        try
        {
            var data = Identify(window);
            data.Flags = NifMessage | NifIcon | NifTip | NifShowTip;
            data.CallbackMessage = CallbackMessage;
            data.Icon = ownedIcon.Handle;

            var length = Math.Min(toolTip.Length, ToolTipCapacity - 1);
            if (length < toolTip.Length && length > 0 && char.IsHighSurrogate(toolTip[length - 1]))
                length--;
            MemoryMarshal.Cast<char, ushort>(toolTip.AsSpan(0, length)).CopyTo(data.ToolTip[..length]);

            if (Shell_NotifyIconW(NimAdd, ref data) == 0)
            {
                // A DPI change can also broadcast TaskbarCreated without removing this icon.
                // Updating the same shell entry preserves its callback version.
                return recovering && !disposed && Shell_NotifyIconW(NimModify, ref data) != 0;
            }

            if (window == 0)
            {
                // The HWND was destroyed during the synchronous shell call.
                data = Identify(data.Window);
                _ = Shell_NotifyIconW(NimDelete, ref data);
                return false;
            }

            registered = true;
            if (disposed)
                return false;

            data = Identify(window);
            data.Version = 4;
            version4 = Shell_NotifyIconW(NimSetVersion, ref data) != 0;
            if (!version4 && !disposed)
            {
                // Version 3 retains legacy packing while enabling keyboard notifications when supported.
                data.Version = 3;
                _ = Shell_NotifyIconW(NimSetVersion, ref data);
            }

            return !disposed;
        }
        finally
        {
            registering = false;
            if (disposed)
                ReleaseResources();
        }
    }

    static nint WindowProc(nint hwnd, uint message, nuint wParam, nint lParam, nuint subclassId, nuint referenceData)
    {
        try
        {
            var root = GCHandle.FromIntPtr((nint)referenceData);
            if (root.Target is WindowsTrayIcon tray)
            {
                if (message == WmNcDestroy)
                {
                    _ = RemoveWindowSubclass(hwnd, windowProc, subclassId);
                    root.Free();
                    tray.window = 0;
                    try
                    {
                        tray.DeleteIcon(hwnd);
                    }
                    finally
                    {
                        tray.Dispose();
                    }
                }
                else if (message == WmClose)
                {
                    // The caller owns this hidden window's lifetime through Dispose.
                    return 0;
                }
                else if (message == CallbackMessage)
                {
                    tray.HandleNotification(wParam, lParam);
                    return 0;
                }
                else if (message == tray.taskbarCreated)
                {
                    tray.RegisterIcon(recovering: true);
                    return 0;
                }
            }

            return DefSubclassProc(hwnd, message, wParam, lParam);
        }
        catch (Exception ex)
        {
            ReportError(ex, "Windows notification icon callback failed.");
        }

        return 0;
    }

    void HandleNotification(nuint wParam, nint lParam)
    {
        if (disposed || !registered || registering || insideCallback)
            return;

        var code = unchecked((uint)lParam);
        if (version4)
        {
            if ((code >> 16) != IconId)
                return;
            code &= 0xffff;
        }
        else if (wParam != IconId)
        {
            return;
        }

        insideCallback = true;
        try
        {
            // v4 sends semantic select/context notifications. Ignore their raw mouse companions.
            if (code is NinSelect or NinKeySelect || (!version4 && code == WmLButtonUp))
            {
                activate();
            }
            else if ((code == WmContextMenu || (!version4 && code == WmRButtonUp))
                && TryGetMenuAnchor(out var point) && !disposed)
            {
                showMenu(point);
            }
        }
        finally
        {
            insideCallback = false;
        }
    }

    bool TryGetMenuAnchor(out PixelPoint point)
    {
        // The tray callback's wParam is undocumented for WM_CONTEXTMENU (and is an ID in
        // legacy mode). Anchor both mouse and keyboard requests to the icon when possible.
        var id = new NotifyIconIdentifier
        {
            Size = (uint)Unsafe.SizeOf<NotifyIconIdentifier>(),
            Window = window,
            Id = IconId,
        };
        if (Shell_NotifyIconGetRect(in id, out var rect) == 0 && rect.Right > rect.Left && rect.Bottom > rect.Top)
        {
            point = new PixelPoint(rect.Left + (rect.Right - rect.Left) / 2, rect.Top + (rect.Bottom - rect.Top) / 2);
            return true;
        }

        if (GetCursorPos(out var cursor) != 0)
        {
            point = new PixelPoint(cursor.X, cursor.Y);
            return true;
        }

        point = default;
        return false;
    }

    /// <summary>Returns focus to the notification area after the caller dismisses its UI.</summary>
    internal void RestoreShellFocus()
    {
        dispatcher.VerifyAccess();
        if (disposed || !registered || registering)
            return;

        var data = Identify(window);
        _ = Shell_NotifyIconW(NimSetFocus, ref data);
    }

    void OnShutdownStarted(object? sender, EventArgs e) => Dispose();

    public void Dispose()
    {
        dispatcher.VerifyAccess();
        if (disposed)
            return;

        disposed = true;
        dispatcher.ShutdownStarted -= OnShutdownStarted;
        // A synchronous call to Explorer can dispatch sent messages. Let an in-flight add
        // finish first so its registration can still be deleted with the original HWND.
        if (!registering)
            ReleaseResources();
    }

    void ReleaseResources()
    {
        var ownedIcon = icon;
        icon = null;
        try
        {
            DeleteIcon(window);
        }
        finally
        {
            try
            {
                if (window != 0)
                {
                    if (DestroyWindow(window) != 0)
                        window = 0; // Also covers a window whose subclass never attached.
                    else
                        ReportError(new Win32Exception(Marshal.GetLastPInvokeError()), "Unable to destroy the notification icon window.");
                }
            }
            finally
            {
                ownedIcon?.Dispose();
            }
        }
    }

    void DeleteIcon(nint hwnd)
    {
        if (!registered)
            return;

        registered = false;
        var data = Identify(hwnd);
        _ = Shell_NotifyIconW(NimDelete, ref data);
    }

    static NotifyIconData Identify(nint hwnd) => new() { Size = notifyIconDataSize, Window = hwnd, Id = IconId };

    static void ReportError(Exception ex, string message)
    {
        try
        {
            Log.Error(nameof(WindowsTrayIcon), ex, message);
        }
        catch (Exception)
        {
            // A failing log sink must not unwind through the native window procedure.
        }
    }

    // Native WCHAR arrays are inline UTF-16 code units. Using ushort keeps the entire
    // structure blittable under both JIT and Native AOT without /unsafe or a custom pack.
    [InlineArray(ToolTipCapacity)]
    struct ToolTipBuffer { ushort first; }

    [InlineArray(256)]
    struct InfoBuffer { ushort first; }

    [InlineArray(64)]
    struct InfoTitleBuffer { ushort first; }

    [StructLayout(LayoutKind.Sequential)]
    struct NotifyIconData
    {
        public uint Size;
        public nint Window;
        public uint Id;
        public uint Flags;
        public uint CallbackMessage;
        public nint Icon;
        public ToolTipBuffer ToolTip;
        public uint State;
        public uint StateMask;
        public InfoBuffer Info;
        public uint Version; // Union with uTimeout.
        public InfoTitleBuffer InfoTitle;
        public uint InfoFlags;
        public Guid GuidItem;
        public nint BalloonIcon;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct NotifyIconIdentifier
    {
        public uint Size;
        public nint Window;
        public uint Id;
        public Guid GuidItem;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct NativePoint { public int X, Y; }

    [StructLayout(LayoutKind.Sequential)]
    struct NativeRect { public int Left, Top, Right, Bottom; }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    delegate nint SubclassProc(nint hwnd, uint message, nuint wParam, nint lParam, nuint subclassId, nuint referenceData);

    [DllImport("comctl32.dll", ExactSpelling = true)]
    static extern int SetWindowSubclass(nint hwnd, SubclassProc proc, nuint subclassId, nuint referenceData);

    [DllImport("comctl32.dll", ExactSpelling = true)]
    static extern int RemoveWindowSubclass(nint hwnd, SubclassProc proc, nuint subclassId);

    [DllImport("comctl32.dll", ExactSpelling = true)]
    static extern nint DefSubclassProc(nint hwnd, uint message, nuint wParam, nint lParam);

    [DllImport("shell32.dll", ExactSpelling = true)]
    static extern int Shell_NotifyIconW(uint message, ref NotifyIconData data);

    [DllImport("shell32.dll", ExactSpelling = true)]
    static extern int Shell_NotifyIconGetRect(in NotifyIconIdentifier identifier, out NativeRect rect);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    static extern nint CreateWindowExW(uint exStyle, string className, nint name, uint style, int x, int y,
        int width, int height, nint parent, nint menu, nint instance, nint parameter);

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    static extern int DestroyWindow(nint hwnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    static extern uint RegisterWindowMessageW(string message);

    [DllImport("user32.dll", ExactSpelling = true)]
    static extern int ChangeWindowMessageFilterEx(nint hwnd, uint message, uint action, nint changeInfo);

    [DllImport("user32.dll", ExactSpelling = true)]
    static extern int GetCursorPos(out NativePoint point);

    [DllImport("user32.dll", ExactSpelling = true)]
    static extern int GetSystemMetrics(int index);
}
#endif
