using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Application;
using System.Application.Services;
using System.Application.UI;
using System.Runtime.InteropServices;
using Window = Avalonia.Controls.Window;

// ReSharper disable once CheckNamespace
namespace System
{
    public static partial class WindowExtensions
    {
        public static bool IsActiveWindow(this Window? window)
        {
            if (window == null)
                return false;
            if (IAvaloniaApplication.Instance.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.Windows.Any_Nullable(x => x == window && x.IsActive && x.IsVisible))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 设置调整大小模式。
        /// </summary>
        /// <param name="window"></param>
        /// <param name="value"></param>
        public static void SetResizeMode(this Window window, ResizeMode value)
        {
            var p = DI.Get<IPlatformService>();
            switch (value)
            {
                case ResizeMode.NoResize:
                case ResizeMode.CanMinimize:
                    window.CanResize = false;
                    break;
                case ResizeMode.CanResize:
#pragma warning disable CS0618 // 类型或成员已过时
                case ResizeMode.CanResizeWithGrip:
#pragma warning restore CS0618 // 类型或成员已过时
                    window.CanResize = true;
                    break;
            }
            p.SetResizeMode(window.PlatformImpl.Handle.Handle, value);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void ActivateWorkaround(this Window window)
        {
            ArgumentNullException.ThrowIfNull(window);

            // Call default Activate() anyway.
            window.Activate();

            // Skip workaround for non-windows platforms.
            if (!OperatingSystem2.IsWindows()) return;

            var platformImpl = window.PlatformImpl;
            if (ReferenceEquals(platformImpl, null)) return;

            var platformHandle = platformImpl.Handle;
            if (ReferenceEquals(platformHandle, null)) return;

            var handle = platformHandle.Handle;
            if (handle == IntPtr.Zero) return;

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