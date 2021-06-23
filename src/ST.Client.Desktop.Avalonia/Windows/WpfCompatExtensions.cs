using Avalonia.Controls;
using System.Application.Services;
using System.Runtime.InteropServices;
using Window = Avalonia.Controls.Window;

namespace System.Windows
{
    /// <summary>
    /// 通过扩展兼容WPF部分API的内容
    /// </summary>
    public static partial class WpfCompatExtensions
    {
        /// <summary>
        /// 设置调整大小模式。
        /// </summary>
        /// <param name="window"></param>
        /// <param name="value"></param>
        public static void SetResizeMode(this Window window, ResizeModeCompat value)
        {
            var p = DI.Get<IDesktopPlatformService>();
            switch (value)
            {
                case ResizeModeCompat.NoResize:
                case ResizeModeCompat.CanMinimize:
                    window.CanResize = false;
                    break;
                case ResizeModeCompat.CanResize:
#pragma warning disable CS0618 // 类型或成员已过时
                case ResizeModeCompat.CanResizeWithGrip:
#pragma warning restore CS0618 // 类型或成员已过时
                    window.CanResize = true;
                    break;
            }
            p.SetResizeMode(window.PlatformImpl.Handle.Handle, value);
        }

        static readonly bool IsWin32NT = Environment.OSVersion.Platform == PlatformID.Win32NT;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void ActivateWorkaround(this Window window)
        {
            if (window is null) throw new ArgumentNullException(nameof(window));

            // Call default Activate() anyway.
            window.Activate();

            // Skip workaround for non-windows platforms.
            if (!IsWin32NT) return;

            var platformImpl = window.PlatformImpl;
            if (ReferenceEquals(platformImpl, null)) return;

            var platformHandle = platformImpl.Handle;
            if (ReferenceEquals(platformHandle, null)) return;

            var handle = platformHandle.Handle;
            if (IntPtr.Zero == handle) return;

            try
            {
                SetForegroundWindow(handle);
            }
            catch
            {
                // ignored
            }
        }

        public static void ShowActivate(this Window window)
        {
            if (window.WindowState != WindowState.Normal) window.WindowState = WindowState.Normal;
            window.Activate();
        }
    }
}