#if WINDOWS
using PointDouble = System.Windows.PointD;
using PointInt32 = System.Drawing.Point;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class MouseHook
{
    static PInvoke.User32.SafeHookHandle? _mouseHookHandle;
    static PInvoke.User32.WindowsHookDelegate? _mouseDelegate;

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
        if (_mouseHookHandle != null)
        {
            _mouseHookHandle.Dispose();
            _mouseHookHandle = null;
            _mouseDelegate = null;
        }
    }

    static void Subscribe()
    {
        if (_mouseHookHandle == null)
        {
            var currentProcessMainModuleName = Process.GetCurrentProcess().MainModule?.ModuleName;
            if (string.IsNullOrWhiteSpace(currentProcessMainModuleName)) throw new ArgumentNullException(nameof(currentProcessMainModuleName));
            _mouseDelegate = MouseHookProc;
            PInvoke.User32.SafeHookHandle d;
            _mouseHookHandle = PInvoke.User32.SetWindowsHookEx(PInvoke.User32.WindowsHookType.WH_MOUSE_LL,
                _mouseDelegate,
                PInvoke.Kernel32.GetModuleHandle(currentProcessMainModuleName),
                0);
            if (_mouseHookHandle.DangerousGetHandle() == nint.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode);
            }
        }
    }

    static int MouseHookProc(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0)
        {
            var mouseHookStructObj = Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            if (mouseHookStructObj == null) throw new ArgumentNullException(nameof(mouseHookStructObj));
            var mouseHookStruct = (MSLLHOOKSTRUCT)mouseHookStructObj;
            if (wParam == WM_LBUTTONUP)
            {
                MouseUp?.Invoke(null, new PointDouble(mouseHookStruct.pt.X, mouseHookStruct.pt.Y));
            }
        }
        if (_mouseHookHandle != null)
        {
            return PInvoke.User32.CallNextHookEx(_mouseHookHandle.DangerousGetHandle(), nCode, wParam, lParam);
        }
        else
        {
            return 0;
        }
    }

    const int WM_LBUTTONUP = 0x0202;

    [StructLayout(LayoutKind.Sequential)]
    struct MSLLHOOKSTRUCT
    {
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
        public PointInt32 pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetSystemCursor(nint hcur, uint id);

    public const uint OCR_NORMAL = 32512;
    public const uint OCR_IBEAM = 32513;
    public const nint OCR_CROSS = 32515;
    public const uint OCR_HAND = 32649;
}

delegate void MouseUpEventHandler(object? sender, PointDouble p);
#endif