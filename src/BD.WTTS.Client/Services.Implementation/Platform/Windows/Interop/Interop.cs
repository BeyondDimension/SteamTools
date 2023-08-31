#if WINDOWS
using Kernel32_ = PInvoke.Kernel32;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class Interop
{
    public static partial class Kernel32
    {
        // use PInvoke.Kernel32

        const uint QueryLimitedInformation = 0x00001000;

        public static string? QueryFullProcessImageName(Process process)
        {
            try
            {
                int capacity = 1024;
                StringBuilder sb = new(capacity);
                var handle = Kernel32_.OpenProcess(new(QueryLimitedInformation), false, process.Id);
                Kernel32_.QueryFullProcessImageName(handle, 0, sb, ref capacity);
                string fullPath = sb.ToString(0, capacity);
                return fullPath;
            }
            catch
            {
                return null;
            }
        }
    }

    public static partial class User32
    {
        // use PInvoke.User32

        [LibraryImport("user32.dll")]
        public static partial IntPtr SetActiveWindow(IntPtr hWnd);

        [LibraryImport("user32.dll")]
        public static partial int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, int bAlpha, int dwFlags);

        [LibraryImport("user32.dll")]
        public static partial int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);
    }

    public static partial class DWMApi
    {
#if DEBUG
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();
#endif

        public const int DWM_TNP_VISIBLE = 0x8;
        public const int DWM_TNP_OPACITY = 0x4;
        public const int DWM_TNP_RECTDESTINATION = 0x1;

        [LibraryImport("dwmapi.dll")]
        public static partial int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [LibraryImport("dwmapi.dll")]
        public static partial int DwmUnregisterThumbnail(IntPtr thumb);

        [LibraryImport("dwmapi.dll")]
        public static partial int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

        [DllImport("dwmapi.dll", CharSet = CharSet.Auto)]
        public static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DWM_THUMBNAIL_PROPERTIES props);

        [StructLayout(LayoutKind.Sequential)]
        public struct DWM_THUMBNAIL_PROPERTIES
        {
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
            public int dwFlags;
            public RECT rcDestination;
            public RECT rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PSIZE
        {
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
            public int x;
            public int y;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;

            public int Height => Bottom - Top;
        }
    }
}
#endif