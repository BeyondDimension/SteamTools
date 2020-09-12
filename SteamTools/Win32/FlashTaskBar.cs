using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.Win32
{
    internal static class FlashTaskBar
    {
        [DllImport("User32.dll", CharSet = CharSet.Unicode, EntryPoint = "FlashWindow")]
        private static extern void FlashWindow(IntPtr hwnd, bool bInvert);
        [DllImport("User32.dll", CharSet = CharSet.Unicode, EntryPoint = "FlashWindowEx")]
        private static extern void FlashWindowEx(ref FLASHWINFO pwfi);

        internal struct FLASHWINFO
        {
#pragma warning disable 649
            public UInt32 cbSize;//该结构的字节大小
            public IntPtr hwnd;//要闪烁的窗口的句柄，该窗口可以是打开的或最小化的
            public UInt32 dwFlags;//闪烁的状态
            public UInt32 uCount;//闪烁窗口的次数
            public UInt32 dwTimeout;//窗口闪烁的频度，毫秒为单位；若该值为0，则为默认图标的闪烁频度
#pragma warning restore 649
        }

        public const UInt32 FLASHW_TRAY = 2;
        public const UInt32 FLASHW_TIMERNOFG = 12;

        public static void FlashWindow(IntPtr hwnd) 
        {
            //直接调用FlashWindow即可
            FlashWindow(hwnd, true);
        }
    }
}
