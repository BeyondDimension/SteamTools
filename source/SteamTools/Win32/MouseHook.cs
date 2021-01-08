using MetroRadiance.Interop.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SteamTools.Win32
{
    internal static class MouseHook
    {
        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        private static int _mouseHookHandle;
        private static HookProc _mouseDelegate;

        private static event MouseUpEventHandler MouseUp;
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

        private static void Unsubscribe()
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

        private static void Subscribe()
        {
            if (_mouseHookHandle == 0)
            {
                _mouseDelegate = MouseHookProc;
                _mouseHookHandle = SetWindowsHookEx(WH_MOUSE_LL,
                    _mouseDelegate,
                    GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),
                    0);
                if (_mouseHookHandle == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }

        private static int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT mouseHookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                if (wParam == WM_LBUTTONUP)
                {
                    if (MouseUp != null)
                    {
                        MouseUp.Invoke(null, new Point(mouseHookStruct.pt.X, mouseHookStruct.pt.Y));
                    }
                }
            }
            return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONUP = 0x0202;

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string name);

        [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto,
           CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int UnhookWindowsHookEx(int idHook);

        [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto,
             CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto)]
        public static extern IntPtr LoadCursor(IntPtr hInstance, uint lpCursorName);

        [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto)]
        public static extern bool SetSystemCursor(IntPtr hcur, uint id);
        public const uint OCR_NORMAL = 32512;
        public const uint OCR_IBEAM = 32513;
        public const uint OCR_CROSS = 32515;
        public const uint OCR_HAND = 32649;

        [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto)]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam,
        IntPtr pvParam, uint fWinIni);
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
        public static extern bool GetCursorPos(out POINT pt);

        [DllImport(User32Window.LibraryName, CharSet = CharSet.Auto, EntryPoint = "WindowFromPoint")]
        public static extern IntPtr WindowFromPoint(POINT pt);
    }

    public delegate void MouseUpEventHandler(object sender, Point p);
}
