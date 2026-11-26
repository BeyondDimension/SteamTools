#if WINDOWS
namespace BD.WTTS.Services.Implementation;

static unsafe partial class WindowToolsNative
{
    internal const int Style = -16, ExStyle = -20, Owner = -8;
    internal const uint Child = 0x40000000, Popup = 0x80000000, Frame = 0x00CC0000;
    internal const uint Topmost = 0x8, Layered = 0x80000, Transparent = 0x20, NoRedirectionBitmap = 0x200000;
    internal const uint NoSize = 1, NoMove = 2, NoZOrder = 4, NoActivate = 0x10, FrameChanged = 0x20;
    internal const uint ShowWindowFlag = 0x40;
    internal const uint WmQuit = 0x12, WmConfigure = 0x8001;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Point
    {
        internal int X, Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        internal int Left, Top, Right, Bottom;
        internal readonly int Width => Right - Left;
        internal readonly int Height => Bottom - Top;
        internal readonly bool Contains(Point point) =>
            point.X >= Left && point.X < Right && point.Y >= Top && point.Y < Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Placement
    {
        internal uint Size, Flags, ShowCommand;
        internal Point MinPosition, MaxPosition;
        internal Rect NormalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MonitorInfo
    {
        internal uint Size;
        internal Rect Monitor, Work;
        internal uint Flags;
        internal fixed char Device[32];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Message
    {
        internal nint Window;
        internal uint Id;
        internal nuint WParam;
        internal nint LParam;
        internal uint Time;
        internal Point Point;
        internal uint Private;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MouseData
    {
        internal Point Point;
        internal uint Data, Flags, Time;
        internal nuint Extra;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KeyData
    {
        internal uint Key, ScanCode, Flags, Time;
        internal nuint Extra;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct GuiThreadInfo
    {
        internal uint Size, Flags;
        internal nint Active, Focus, Capture, MenuOwner, MoveSize, Caret;
        internal Rect CaretRect;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate int WindowCallback(nint window, nint param);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate int MonitorCallback(nint monitor, nint dc, Rect* rect, nint param);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate nint HookCallback(int code, nuint message, nint data);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    internal delegate void EventCallback(nint hook, uint @event, nint window, int objectId, int childId, uint thread, uint time);

    [LibraryImport("user32.dll")]
    internal static partial int EnumWindows(WindowCallback callback, nint param);

    [LibraryImport("user32.dll")]
    internal static partial int EnumDisplayMonitors(nint dc, Rect* clip, MonitorCallback callback, nint param);

    [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    internal static partial int GetMonitorInfo(nint monitor, ref MonitorInfo info);

    [LibraryImport("user32.dll")]
    internal static partial nint SetThreadDpiAwarenessContext(nint value);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongW")]
    internal static partial uint GetWindowLong(nint window, int index);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    internal static partial uint SetWindowLong(nint window, int index, uint value);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    internal static partial nint SetWindowLongPtr64(nint window, int index, nint value);

    [LibraryImport("user32.dll")]
    internal static partial nint GetWindow(nint window, uint command);

    [LibraryImport("user32.dll")]
    internal static partial nint GetAncestor(nint window, uint flags);

    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial nint SetParent(nint window, nint parent);

    [LibraryImport("user32.dll")]
    internal static partial int GetWindowRect(nint window, out Rect rect);

    [LibraryImport("user32.dll")]
    internal static partial int GetWindowPlacement(nint window, ref Placement placement);

    [LibraryImport("user32.dll")]
    internal static partial int SetWindowPlacement(nint window, in Placement placement);

    [LibraryImport("user32.dll")]
    internal static partial int SetWindowPos(nint window, nint insertAfter, int x, int y, int width, int height, uint flags);

    [LibraryImport("user32.dll")]
    internal static partial int ShowWindowAsync(nint window, int command);

    [LibraryImport("user32.dll")]
    internal static partial int IsWindow(nint window);

    [LibraryImport("user32.dll")]
    internal static partial nint CopyImage(nint image, uint type, int width, int height, uint flags);

    [LibraryImport("user32.dll")]
    internal static partial int IsWindowVisible(nint window);

    [LibraryImport("user32.dll")]
    internal static partial int IsIconic(nint window);

    [LibraryImport("user32.dll")]
    internal static partial int IsZoomed(nint window);

    [LibraryImport("user32.dll")]
    internal static partial uint GetWindowThreadProcessId(nint window, out uint pid);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowTextLengthW")]
    internal static partial int GetWindowTextLength(nint window);

    [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial nint FindWindow(string name, string? title);

    [LibraryImport("user32.dll", EntryPoint = "FindWindowExW", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial nint FindWindowEx(nint parent, nint after, string name, string? title);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageTimeoutW")]
    internal static partial nint SendMessageTimeout(nint window, uint message, nuint wParam, nint lParam, uint flags, uint timeout, out nuint result);

    [LibraryImport("user32.dll")]
    internal static partial int MapWindowPoints(nint from, nint to, ref Point point, uint count);

    [LibraryImport("user32.dll")]
    internal static partial nint WindowFromPoint(Point point);

    [LibraryImport("user32.dll")]
    internal static partial nint ChildWindowFromPointEx(nint parent, Point point, uint flags);

    [LibraryImport("user32.dll")]
    internal static partial int IsChild(nint parent, nint window);

    [LibraryImport("user32.dll")]
    internal static partial nint GetForegroundWindow();

    [LibraryImport("user32.dll")]
    internal static partial int SetForegroundWindow(nint window);

    [LibraryImport("user32.dll")]
    internal static partial int GetGUIThreadInfo(uint thread, ref GuiThreadInfo info);

    [LibraryImport("user32.dll")]
    internal static partial int ClipCursor(Rect* rect);

    [LibraryImport("user32.dll")]
    internal static partial int GetClipCursor(out Rect rect);

    [LibraryImport("user32.dll")]
    internal static partial int SetLayeredWindowAttributes(nint window, uint color, byte alpha, uint flags);

    [LibraryImport("user32.dll")]
    internal static partial int GetLayeredWindowAttributes(nint window, out uint color, out byte alpha, out uint flags);

    [LibraryImport("user32.dll", EntryPoint = "PostMessageW")]
    internal static partial int PostMessage(nint window, uint message, nuint wParam, nint lParam);

    [LibraryImport("user32.dll", EntryPoint = "PostThreadMessageW")]
    internal static partial int PostThreadMessage(uint thread, uint message, nuint wParam, nint lParam);

    [LibraryImport("user32.dll", EntryPoint = "PeekMessageW")]
    internal static partial int PeekMessage(out Message message, nint window, uint min, uint max, uint flags);

    [LibraryImport("user32.dll", EntryPoint = "GetMessageW")]
    internal static partial int GetMessage(out Message message, nint window, uint min, uint max);

    [LibraryImport("user32.dll")]
    internal static partial short GetAsyncKeyState(int key);

    [LibraryImport("user32.dll")]
    internal static partial uint GetDoubleClickTime();

    [LibraryImport("user32.dll")]
    internal static partial int GetSystemMetrics(int index);

    [LibraryImport("user32.dll", EntryPoint = "GetClassLongW")]
    internal static partial uint GetClassLong(nint window, int index);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowsHookExW")]
    internal static partial nint SetWindowsHookEx(int kind, HookCallback callback, nint module, uint thread);

    [LibraryImport("user32.dll")]
    internal static partial nint CallNextHookEx(nint hook, int code, nuint message, nint data);

    [LibraryImport("user32.dll")]
    internal static partial int UnhookWindowsHookEx(nint hook);

    [LibraryImport("user32.dll")]
    internal static partial nint SetWinEventHook(uint min, uint max, nint module, EventCallback callback, uint process, uint thread, uint flags);

    [LibraryImport("user32.dll")]
    internal static partial int UnhookWinEvent(nint hook);

    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16)]
    internal static partial nint GetModuleHandle(string? module);

    [LibraryImport("kernel32.dll")]
    internal static partial uint GetCurrentThreadId();

    [LibraryImport("oleacc.dll")]
    internal static partial int AccessibleObjectFromWindow(nint window, uint objectId, in Guid iid, out nint accessible);

    [LibraryImport("oleaut32.dll")]
    internal static partial int VariantClear(ref Variant value);

    [LibraryImport("ole32.dll")]
    internal static partial int CoInitializeEx(nint reserved, uint mode);

    [LibraryImport("ole32.dll")]
    internal static partial void CoUninitialize();

    // The trailing BRECORD pair in the VARIANT union determines its native size.
    [StructLayout(LayoutKind.Sequential)]
    internal struct Variant
    {
        internal ushort Type, Reserved1, Reserved2, Reserved3;
        internal nint Value, RecordInfo;
    }

    internal readonly ref struct DpiScope
    {
        readonly nint previous;

        public DpiScope() => previous = SetThreadDpiAwarenessContext(-4); // Per-monitor v2, physical virtual-screen coordinates.

        public void Dispose()
        {
            if (previous != 0)
                SetThreadDpiAwarenessContext(previous);
        }
    }

    internal readonly ref struct ComScope
    {
        readonly int result;

        public ComScope() => result = CoInitializeEx(0, 0);

        internal bool IsReady => result >= 0 || result == unchecked((int)0x80010106); // An existing STA is also valid.

        public void Dispose()
        {
            if (result >= 0)
                CoUninitialize();
        }
    }
}
#endif
