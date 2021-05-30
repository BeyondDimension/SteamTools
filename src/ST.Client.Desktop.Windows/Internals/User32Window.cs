using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System
{
    internal static class User32Window
    {
        public const string LibraryName = "user32.dll";

        [DllImport(LibraryName, CharSet = CharSet.Auto, EntryPoint = "FlashWindow")]
        static extern void FlashWindow(IntPtr hwnd, bool bInvert);

        [DllImport(LibraryName, CharSet = CharSet.Auto, EntryPoint = "FlashWindowEx")]
        static extern void FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport(LibraryName, CharSet = CharSet.Auto)]
        public static extern bool ShowWindow(IntPtr hWnd, short State);

        [DllImport(LibraryName, CharSet = CharSet.Auto)]
        internal static extern IntPtr GetOpenClipboardWindow();

        [DllImport(LibraryName, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        public enum Cmd_SHOWWINDOWS : short
        {
            /// <summary>
            ///     Minimizes a window, even if the thread that owns the window is not responding. This flag should only be used when
            ///     minimizing windows from a different thread.
            /// </summary>
            SW_FORCEMINIMIZE = 11,

            /// <summary>
            ///     Hides the window and activates another window.
            /// </summary>
            SW_HIDE = 0,

            /// <summary>
            ///     Maximizes the specified window.
            /// </summary>
            SW_MAXIMIZE = 3,

            /// <summary>
            ///     Minimizes the specified window and activates the next top-level window in the Z order.
            /// </summary>
            SW_MINIMIZE = 6,

            /// <summary>
            ///     Activates and displays the window. If the window is minimized or maximized, the system restores it to its original
            ///     size and position. An application should specify this flag when restoring a minimized window.
            /// </summary>
            SW_RESTORE = 9,

            /// <summary>
            ///     Activates the window and displays it in its current size and position.
            /// </summary>
            SW_SHOW = 5,

            /// <summary>
            ///     Sets the show state based on the SW_ value specified in the STARTUPINFO structure passed to the CreateProcess
            ///     function by the program that started the application.
            /// </summary>
            SW_SHOWDEFAULT = 10,

            /// <summary>
            ///     Activates the window and displays it as a maximized window.
            /// </summary>
            SW_SHOWMAXIMIZED = 3,

            /// <summary>
            ///     Activates the window and displays it as a minimized window.
            /// </summary>
            SW_SHOWMINIMIZED = 2,

            /// <summary>
            ///     Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is not
            ///     activated.
            /// </summary>
            SW_SHOWMINNOACTIVE = 7,

            /// <summary>
            ///     Displays the window in its current size and position. This value is similar to SW_SHOW, except that the window is
            ///     not activated.
            /// </summary>
            SW_SHOWNA = 8,

            /// <summary>
            ///     Displays a window in its most recent size and position. This value is similar to SW_SHOWNORMAL, except that the
            ///     window is not activated.
            /// </summary>
            SW_SHOWNOACTIVATE = 4,

            /// <summary>
            ///     Activates and displays a window. If the window is minimized or maximized, the system restores it to its original
            ///     size and position. An application should specify this flag when displaying the window for the first time.
            /// </summary>
            SW_SHOWNORMAL = 1
        };

        internal struct FLASHWINFO
        {
#pragma warning disable 649

            /// <summary>
            /// 该结构的字节大小
            /// </summary>
            public uint cbSize;

            /// <summary>
            /// 要闪烁的窗口的句柄，该窗口可以是打开的或最小化的
            /// </summary>
            public IntPtr hwnd;

            /// <summary>
            /// 闪烁的状态
            /// </summary>
            public uint dwFlags;

            /// <summary>
            /// 闪烁窗口的次数
            /// </summary>
            public uint uCount;

            /// <summary>
            /// 窗口闪烁的频度，毫秒为单位；若该值为0，则为默认图标的闪烁频度
            /// </summary>
            public uint dwTimeout;

#pragma warning restore 649
        }

        public const uint FLASHW_TRAY = 2;
        public const uint FLASHW_TIMERNOFG = 12;

        //获取窗口标题
        [DllImport(LibraryName, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(
            IntPtr hWnd,//窗口句柄
            StringBuilder lpString,//标题
            int nMaxCount //最大值
            );

        //获取类的名字
        [DllImport(LibraryName, CharSet = CharSet.Unicode)]
        public static extern int GetClassName(
            IntPtr hWnd,//句柄
            StringBuilder lpString, //类名
            int nMaxCount //最大值
            );

        //获取句柄所属进程pid
        [DllImport(LibraryName, CharSet = CharSet.Auto, EntryPoint = "GetWindowThreadProcessId")]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int pid);

        [DllImport(LibraryName, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport(LibraryName, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport(LibraryName, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport(LibraryName, ExactSpelling = true)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport(LibraryName, ExactSpelling = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport(LibraryName, ExactSpelling = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr GetWindowLongPtr(IntPtr hwnd, int nIndex)
        {
            return IntPtr.Size > 4
                ? GetWindowLongPtr_x64(hwnd, nIndex)
                : new IntPtr(GetWindowLong(hwnd, nIndex));
        }

        [DllImport(LibraryName, CharSet = CharSet.Auto)]
        public static extern int GetWindowLongA(IntPtr hWnd, int nIndex);

        [DllImport(LibraryName, CharSet = CharSet.Auto)]
        static extern int GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport(LibraryName, CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtr")]
        static extern IntPtr GetWindowLongPtr_x64(IntPtr hwnd, int nIndex);

        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr SetWindowLongPtr(IntPtr hwnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size > 4
                ? SetWindowLongPtr_x64(hwnd, nIndex, dwNewLong)
                : new IntPtr(SetWindowLong(hwnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport(LibraryName, CharSet = CharSet.Auto)]
        public static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);

        [DllImport(LibraryName, CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr_x64(IntPtr hwnd, int nIndex, IntPtr dwNewLong);

        [DllImport(LibraryName, CharSet = CharSet.Auto)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport(LibraryName, CharSet = CharSet.Auto, EntryPoint = "SetForegroundWindow")]
        //设置此窗体为活动窗体
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void FlashWindow(IntPtr hwnd)
        {
            //ShowWindow(hwnd, (short)Cmd_SHOWWINDOWS.SW_NORMAL);
            //直接调用FlashWindow即可
            FlashWindow(hwnd, true);
        }

        public static void FlashWindow(ref FLASHWINFO f)
        {
            //直接调用FlashWindow即可
            FlashWindowEx(ref f);
        }
    }
}