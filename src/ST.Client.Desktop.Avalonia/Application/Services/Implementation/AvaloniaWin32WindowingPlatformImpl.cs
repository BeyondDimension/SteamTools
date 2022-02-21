#pragma warning disable CA1416 // 验证平台兼容性
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Avalonia.Rendering;
using Avalonia.Styling;
using Avalonia.Win32;
using FluentAvalonia.Interop;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace System.Application.Services.Implementation
{
    [SupportedOSPlatform("Windows7.0")]
    public sealed class AvaloniaWin32WindowingPlatformImpl : IWindowingPlatform
    {
        public IWindowImpl CreateWindow()
        {
            if (!Design.IsDesignMode && OperatingSystem2.IsWindows10AtLeast)
            {
                return CreateWin10Window();
            }

            //return PlatformManager.CreateWindow();
            return ((IWindowingPlatform)Win32Platform).CreateWindow();
        }

        public IWindowImpl CreateEmbeddableWindow()
        {
            return ((IWindowingPlatform)Win32Platform).CreateEmbeddableWindow();
        }

        public ITrayIconImpl? CreateTrayIcon()
        {
            return ((IWindowingPlatform)Win32Platform).CreateTrayIcon();
        }

        static Win32Platform GetWin32Platform()
        {
            var type = typeof(Win32Platform);
            const string name = "Instance";
            var property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Static) ?? type.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            if (property == null)
                throw new NullReferenceException("Avalonia.Win32.Win32Platform.Instance not found.");
            if (property.PropertyType != type)
                throw new TypeAccessException("Avalonia.Win32.Win32Platform.Instance type error.");
            var value = (Win32Platform?)property.GetValue(null);
            if (value == null)
                throw new NullReferenceException("Avalonia.Win32.Win32Platform.Instance is null.");
            return value;
        }

        static readonly Lazy<Win32Platform> _Win32Platform = new(GetWin32Platform);
        public static Win32Platform Win32Platform => _Win32Platform.Value;

        [SupportedOSPlatform("Windows10.0.10240.0")]
        public static WindowImpl CreateWin10Window() => new Window10Impl();

        [SupportedOSPlatform("Windows10.0.10240.0")]
        internal class Window10Impl : WindowImpl
        {
            public Window10Impl()
            {
                //Win32Interop.OSVERSIONINFOEX version = new Win32Interop.OSVERSIONINFOEX
                //{
                //    OSVersionInfoSize = Marshal.SizeOf<Win32Interop.OSVERSIONINFOEX>()
                //};

                //Win32Interop.RtlGetVersion(ref version);

                //if (version.MajorVersion < 10)
                if (!OperatingSystem2.IsWindows10AtLeast)
                {
                    throw new NotSupportedException("Windows versions earlier than 10 are not supported");
                }

                //_isWindows11 = version.BuildNumber >= 22000;
                //_version = version;
            }

            protected override IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
            {
                switch ((WM)msg)
                {
                    case WM.NCCALCSIZE:
                        // Follows logic from how to extend window frame + WindowsTerminal + Firefox
                        // Windows Terminal only handles WPARAM = TRUE & only adjusts the top of the
                        // rgrc[0] RECT & gets the correct result
                        // Firefox, on the other hand, handles BOTH times WM_NCCALCSIZE is called,
                        // and modifies the RECT.
                        // This particularly differs from the "built-in" method in Avalonia in that
                        // I retain the SystemBorder & ability resize the window in the transparent
                        // area over the drop shadows, meaning resize handles don't overlap the window

                        if (wParam != IntPtr.Zero &&
                            _owner?.Window.CanResize == true)
                        {
                            var ncParams = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(lParam);

                            var originalTop = ncParams.rgrc[0].top;

                            var ret = Win32Interop.DefWindowProc(hWnd, (uint)WM.NCCALCSIZE, wParam, lParam);
                            if (ret != IntPtr.Zero)
                                return ret;

                            var newSize = ncParams.rgrc[0];

                            if (newSize.Width == ncParams.rgrc[1].Width &&
                                newSize.Height == ncParams.rgrc[1].Height)
                            {
                                Marshal.StructureToPtr(ncParams, lParam, true);
                                return IntPtr.Zero;
                            }

                            newSize.top = originalTop;

                            if (WindowState == WindowState.Maximized ||
                                WindowState == WindowState.FullScreen)
                            {
                                //newSize.top += GetResizeHandleHeight();
                            }
                            else
                            {
                                if (_owner != null)
                                {
                                    newSize.left += 8;
                                    newSize.right -= 8;
                                    newSize.bottom -= 8;
                                }
                            }

                            ncParams.rgrc[0] = newSize;

                            Marshal.StructureToPtr(ncParams, lParam, true);
                            return IntPtr.Zero;
                        }
                        break;

                    //case WM.NCHITTEST:
                    //    return HandleNCHitTest(lParam);

                    case WM.SIZE:
                        EnsureExtended();

                        if (_fakingMaximizeButton)
                        {
                            // Sometimes the effect can get stuck, so if we resize, clear it
                            _owner.FakeMaximizePressed(false);
                            _wasFakeMaximizeDown = false;
                        }
                        break;

                    //case WM.ACTIVATE:
                    //    EnsureExtended();
                    //    break;

                    case WM.NCMOUSEMOVE:
                        if (_fakingMaximizeButton)
                        {
                            var point = PointToClient(PointFromLParam(lParam));
                            _owner.FakeMaximizeHover(_owner.HitTestMaximizeButton(point));
                            return IntPtr.Zero;
                        }
                        break;

                    case WM.NCLBUTTONDOWN:
                        if (_fakingMaximizeButton)
                        {
                            var point = PointToClient(PointFromLParam(lParam));
                            _owner.FakeMaximizePressed(_owner.HitTestMaximizeButton(point));
                            _wasFakeMaximizeDown = true;

                            // This is important. If we don't tell the System we've handled this, we'll get that
                            // classic Win32 button showing when we mouse press, and that's not good
                            return IntPtr.Zero;
                        }
                        break;

                    case WM.NCLBUTTONUP:
                        if (_fakingMaximizeButton && _wasFakeMaximizeDown)
                        {
                            var point = PointToClient(PointFromLParam(lParam));
                            _owner.FakeMaximizePressed(false);
                            _wasFakeMaximizeDown = false;
                            _owner.FakeMaximizeClick();
                            return IntPtr.Zero;
                        }
                        break;
                }

                return base.WndProc(hWnd, msg, wParam, lParam);
            }

            internal void SetOwner(ICoreWindow wnd)
            {
                _owner = wnd;

                _owner.Window.Opened += (s, e) =>
                {
                    //_owner.Resized(new Size(_owner.Window.Width += 32, _owner.Window.Height += 16), PlatformResizeReason.Layout);

                    _owner.Window.GetObservable(Window.WindowStateProperty)
                        .Subscribe(x =>
                        {
                            if (x == WindowState.Normal)
                            {
                                _owner.Resized(new Size(_owner.Window.Width += 32, _owner.Window.Height += 16), PlatformResizeReason.Layout);
                            }
                        });
                };

                if (OperatingSystem2.IsWindows11AtLeast)
                {
                    ((IPseudoClasses)_owner.Classes).Set(":windows11", true);
                }
                else
                {
                    ((IPseudoClasses)_owner.Classes).Set(":windows10", true);
                }
            }

            private int GetResizeHandleHeight()
            {
                //if (_version.BuildNumber >= 14393)
                if (OperatingSystem2.IsWindowsVersionAtLeast(10, 0, 14393))
                {
                    return Win32Interop.GetSystemMetricsForDpi(92 /*SM_CXPADDEDBORDER*/, (uint)(RenderScaling * 96)) +
                        Win32Interop.GetSystemMetricsForDpi(33 /* SM_CYSIZEFRAME */, (uint)(RenderScaling * 96));
                }

                return Win32Interop.GetSystemMetrics(92 /* SM_CXPADDEDBORDER */) +
                    Win32Interop.GetSystemMetrics(33/* SM_CYSIZEFRAME */);
            }

            private void EnsureExtended()
            {
                // We completely ignore anything for extending client area in Avalonia Window impl b/c
                // we're doing super specialized stuff to ensure the best experience interacting with
                // the window and mimic-ing a "modern app"
                var marg = new Win32Interop.MARGINS();

                // WS_OVERLAPPEDWINDOW
                // 0x00C00000L 0x00080000L 不需要标题栏
                // 0x00040000L 创建一个具有粗框的窗口可以用来调整窗口的大小
                var style = 0x00000000L | 0X00800000L | 0x00020000L | 0x00010000L;

                // This is causing the window to appear solid but is completely transparent. Weird...
                //Win32Interop.GetWindowLongPtr(Hwnd, -16).ToInt32();

                RECT frame = new RECT();
                Win32Interop.AdjustWindowRectExForDpi(ref frame,
                    (int)style, false, 0, (int)(RenderScaling * 96));

                marg.topHeight = -frame.top + (_isWindows11 ? 0 : -1);
                Win32Interop.DwmExtendFrameIntoClientArea(Handle.Handle, ref marg);
            }

            protected IntPtr HandleNCHitTest(IntPtr lParam)
            {
                // Because we still have the System Border (which technically extends beyond the actual window
                // into where the Drop shadows are), we can use DefWindowProc here to handle resizing, except
                // on the top. We'll handle that below
                var originalRet = Win32Interop.DefWindowProc(Hwnd, (uint)WM.NCHITTEST, IntPtr.Zero, lParam);
                if (originalRet != new IntPtr(1))
                {
                    return originalRet;
                }

                // At this point, we know that the cursor is inside the client area so it
                // has to be either the little border at the top of our custom title bar,
                // the drag bar or something else in the XAML island. But the XAML Island
                // handles WM_NCHITTEST on its own so actually it cannot be the XAML
                // Island. Then it must be the drag bar or the little border at the top
                // which the user can use to move or resize the window.

                var point = PointToClient(PointFromLParam(lParam));

                RECT rcWindow;
                Win32Interop.GetWindowRect(Hwnd, out rcWindow);

                // On the Top border, the resize handle overlaps with the Titlebar area, which matches
                // a typical Win32 window or modern app window
                var resizeBorderHeight = GetResizeHandleHeight();
                bool isOnResizeBorder = point.Y < resizeBorderHeight;

                // Make sure the caption buttons still get precedence
                // This is where things get tricky too. On Win11, we still want to suppor the snap
                // layout feature when hovering over the Maximize button. Unfortunately no API exists
                // yet to call that manually if using a custom titlebar. But, if we return HT_MAXBUTTON
                // here, the pointer events no longer enter the window
                // See https://github.com/dotnet/wpf/issues/4825 for more on this...
                // To hack our way into making this work, we'll return HT_MAXBUTTON here, but manually
                // manage the state and handle stuff through the WM_NCLBUTTON... events
                // This only applies on Windows 11, Windows 10 will work normally b/c no snap layout thing

                if (_owner!.HitTestCaptionButtons(point))
                {
                    if (_isWindows11)
                    {
                        var result = _owner.HitTestMaximizeButton(point);

                        if (result)
                        {
                            _fakingMaximizeButton = true;
                            return new IntPtr(9);
                        }
                    }
                }
                else
                {
                    if (_fakingMaximizeButton)
                    {
                        _fakingMaximizeButton = false;
                        _owner.FakeMaximizeHover(false);
                        _owner.FakeMaximizePressed(false);
                    }

                    if (WindowState != WindowState.Maximized && isOnResizeBorder)
                        return new IntPtr(12); // HT_TOP

                    if (_owner.HitTestTitleBarRegion(point))
                        return new IntPtr(2); //HT_CAPTION
                }

                if (_fakingMaximizeButton)
                {
                    _fakingMaximizeButton = false;
                    _owner.FakeMaximizeHover(false);
                    _owner.FakeMaximizePressed(false);
                }
                _fakingMaximizeButton = false;
                // return HT_CLIENT, we're in the normal window
                return new IntPtr(1);
            }

            private PixelPoint PointFromLParam(IntPtr lParam)
            {
                return new PixelPoint((short)(ToInt32(lParam) & 0xffff), (short)(ToInt32(lParam) >> 16));
            }

            private static int ToInt32(IntPtr ptr)
            {
                if (IntPtr.Size == 4)
                    return ptr.ToInt32();

                return (int)(ptr.ToInt64() & 0xffffffff);
            }

            private bool _wasFakeMaximizeDown;
            private bool _fakingMaximizeButton;
            //private bool _isWindows11 = false;
            private bool _isWindows11 => OperatingSystem2.IsWindows11AtLeast;
            private ICoreWindow? _owner;
            //private Win32Interop.OSVERSIONINFOEX _version;
        }
    }
}
#pragma warning restore CA1416 // 验证平台兼容性