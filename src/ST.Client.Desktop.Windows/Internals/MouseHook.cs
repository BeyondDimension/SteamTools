using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PointDouble = System.Windows.Point;
using PointInt32 = System.Drawing.Point;

static class MouseHook
{
    delegate int HookProc(int nCode, int wParam, IntPtr lParam);

    static int _mouseHookHandle;
    static HookProc? _mouseDelegate;

    static event MouseUpEventHandler? MouseUp;

    public static event MouseUpEventHandler OnMouseUp
    {
        add
        {
            Subscribe();
            MouseUp += value;
        }
        remove
        {
            MouseUp -= value;
            Unsubscribe();
        }
    }

    static void Unsubscribe()
    {
        if (_mouseHookHandle != 0)
        {
            int result = UnhookWindowsHookEx(_mouseHookHandle);
            _mouseHookHandle = 0;
            _mouseDelegate = null;
            if (result == 0)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode);
            }
        }
    }

    static void Subscribe()
    {
        if (_mouseHookHandle == 0)
        {
            var currentProcessMainModuleName = Process.GetCurrentProcess().MainModule?.ModuleName;
            if (string.IsNullOrWhiteSpace(currentProcessMainModuleName)) throw new ArgumentNullException(nameof(currentProcessMainModuleName));
            _mouseDelegate = MouseHookProc;
            _mouseHookHandle = SetWindowsHookEx(WH_MOUSE_LL,
                _mouseDelegate,
                GetModuleHandle(currentProcessMainModuleName),
                0);
            if (_mouseHookHandle == 0)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode);
            }
        }
    }

    static int MouseHookProc(int nCode, int wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var mouseHookStructObj = Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            if (mouseHookStructObj == null) throw new ArgumentNullException(nameof(mouseHookStructObj));
            var mouseHookStruct = (MSLLHOOKSTRUCT)mouseHookStructObj;
            if (wParam == WM_LBUTTONUP)
            {
                if (MouseUp != null)
                {
                    MouseUp.Invoke(null, new PointDouble(mouseHookStruct.pt.X, mouseHookStruct.pt.Y));
                }
            }
        }
        return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
    }

    const int WH_MOUSE_LL = 14;
    const int WM_LBUTTONUP = 0x0202;

    [StructLayout(LayoutKind.Sequential)]
    struct MSLLHOOKSTRUCT
    {
        public PointInt32 pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr GetModuleHandle(string name);

    [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

    [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto,
       CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    static extern int UnhookWindowsHookEx(int idHook);

    [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto,
         CallingConvention = CallingConvention.StdCall)]
    static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

    [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto)]
    public static extern IntPtr LoadCursor(IntPtr hInstance, uint lpCursorName);

    [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto)]
    public static extern bool SetSystemCursor(IntPtr hcur, uint id);

    public const uint OCR_NORMAL = 32512;
    public const uint OCR_IBEAM = 32513;
    public const uint OCR_CROSS = 32515;
    public const uint OCR_HAND = 32649;

    [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto)]
    public static extern bool SystemParametersInfo(uint uAction, uint uParam,
    IntPtr lpvParam, uint init);

    public enum SystemParametersDesktopInfo : uint
    {
        SPIF_SENDWININICHANGE = 2,
        SPI_SETDESKWALLPAPER = 20,
        SPI_SETDESKPATTERN = 21,
        SPI_SETWORKAREA = 47,
        SPI_GETWORKAREA = 48,
        SPI_GETFONTSMOOTHING = 74,
        SPI_SETFONTSMOOTHING = 75,
        SPI_SETCURSORS = 87,
        SPI_GETDESKWALLPAPER = 115,
        SPI_GETFLATMENU = 4130,
        SPI_SETFLATMENU = 4131,
        SPI_GETDROPSHADOW = 4132,
        SPI_SETDROPSHADOW = 4133,
        SPI_GETCLEARTYPE = 4168,
        SPI_SETCLEARTYPE = 4169,
        SPI_GETFONTSMOOTHINGTYPE = 8202,
        SPI_SETFONTSMOOTHINGTYPE = 8203,
        SPI_GETFONTSMOOTHINGCONTRAST = 8204,
        SPI_SETFONTSMOOTHINGCONTRAST = 8205,
        SPI_GETFONTSMOOTHINGORIENTATION = 8210,
        SPI_SETFONTSMOOTHINGORIENTATION = 8211
    }
    [Flags]
    public enum SystemParamtersInfoFlags : uint
    {
        None = 0,
        SPIF_UPDATEINIFILE = 1,
        SPIF_SENDCHANGE = 2,
        SPIF_SENDWININICHANGE = 2
    }

    [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto, EntryPoint = "GetCursorPos")]
    public static extern bool GetCursorPos(out PointInt32 pt);

    [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto, EntryPoint = "WindowFromPoint")]
    public static extern IntPtr WindowFromPoint(PointInt32 pt);
}

delegate void MouseUpEventHandler(object? sender, PointDouble p);