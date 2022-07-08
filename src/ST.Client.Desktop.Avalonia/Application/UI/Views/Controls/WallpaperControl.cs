using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using System.Application.Services;
using System.Application.Settings;

namespace System.Application.UI.Views.Controls
{
    public class WallpaperControl : Control
    {
        readonly INativeWindowApiService? windowApiService = INativeWindowApiService.Instance;

        Window? window;
        Window? ParentWindow;
        IntPtr _Handle;
        IntPtr _DwmHandle;

        public WallpaperControl()
        {
            //this.InitializeComponent();

            if (OperatingSystem2.IsWindows())
            {
                this.GetObservable(IsVisibleProperty)
                    .Subscribe(x =>
                    {
                        if (x)
                        {
                            if (window == null)
                            {
                                window = new Window
                                {
                                    Width = Bounds.Width,
                                    Height = Bounds.Height,
                                    Background = null,
                                    WindowStartupLocation = WindowStartupLocation.Manual,
                                    WindowState = WindowState.Normal,
                                    ExtendClientAreaToDecorationsHint = true,
                                    ExtendClientAreaTitleBarHeightHint = -1,
                                    ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome,
                                    SystemDecorations = SystemDecorations.Full,
                                    CanResize = false,
                                    TransparencyLevelHint = WindowTransparencyLevel.Transparent,
                                    ShowInTaskbar = false,
                                    Topmost = false,
                                    Focusable = false,
                                    IsEnabled = false,
                                    ShowActivated = false,
                                    IsTabStop = false,
                                    IsHitTestVisible = false,
                                };
                                AttachedToVisualTree += EmptyControl_AttachedToVisualTree;
                                DetachedFromVisualTree += EmptyControl_DetachedFromVisualTree;
                                LayoutUpdated += EmptyControl_LayoutUpdated;
                                window.GotFocus += Window_GotFocus;
                                if (Parent != null && VisualRoot != null)
                                    EmptyControl_AttachedToVisualTree(null, new VisualTreeAttachmentEventArgs(Parent, VisualRoot));
                            }
                        }
                        else
                        {
                            if (window != null)
                            {
                                if (Parent != null && VisualRoot != null)
                                    EmptyControl_DetachedFromVisualTree(null, new VisualTreeAttachmentEventArgs(Parent, VisualRoot));
                                LayoutUpdated -= EmptyControl_LayoutUpdated;
                                AttachedToVisualTree -= EmptyControl_AttachedToVisualTree;
                                DetachedFromVisualTree -= EmptyControl_DetachedFromVisualTree;
                                window = null;
                            }
                        }
                    });
            }
        }

        private void Window_GotFocus(object? sender, Avalonia.Input.GotFocusEventArgs e)
        {
            ParentWindow?.Focus();
        }

        private void EmptyControl_DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (windowApiService != null)
            {
                windowApiService.ReleaseBackground(_DwmHandle);
            }
            if (ParentWindow != null)
            {
                ParentWindow.PositionChanged -= Parent_PositionChanged;
                ParentWindow.Closing -= ParentWindow_Closing;
                ParentWindow.GotFocus -= ParentWindow_GotFocus;
            }
            Close();
            window = null;
        }

        private void EmptyControl_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (window == null) return;
            ParentWindow = (Window)e.Root;
            Show();
            IAvaloniaApplication.Instance.SetTopmostOneTime();
            ParentWindow.PositionChanged += Parent_PositionChanged;
            ParentWindow.Closing += ParentWindow_Closing;
            ParentWindow.GotFocus += ParentWindow_GotFocus;
            ParentWindow.Opened += ParentWindow_Opened;

            ParentWindow.GetObservable(Window.WindowStateProperty)
                .Subscribe(x =>
                {
                    if (x != WindowState.Minimized)
                    {
                        ParentWindow_GotFocus(null, null);
                    }
                });

            _Handle = window.PlatformImpl.Handle.Handle;
            if (windowApiService != null)
            {
                windowApiService.SetWindowPenetrate(_Handle);
                //windowApiService.SetParentWindow(_Handle, ParentWindow.PlatformImpl.Handle.Handle);
                _DwmHandle = windowApiService.SetDesktopBackgroundToWindow(
                    _Handle, (int)window.Width, (int)window.Height);
            }
        }

        private void ParentWindow_Opened(object? sender, EventArgs e)
        {
            Show();
            ParentWindow_GotFocus(null, null);
        }

        private void ParentWindow_GotFocus(object? sender, Avalonia.Input.GotFocusEventArgs e)
        {
            if (window != null)
            {
                window.Topmost = true;
                window.Topmost = false;
            }
            IAvaloniaApplication.Instance.SetTopmostOneTime();
        }

        private void ParentWindow_Closing(object? sender, ComponentModel.CancelEventArgs e)
        {
            window?.Hide();
        }

        private void Parent_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            if (window == null) return;
            window.Position = this.PointToScreen(Bounds.Position);
        }

        private void EmptyControl_LayoutUpdated(object? sender, EventArgs e)
        {
            if (window == null || window.Bounds.Width == 0 || window.Bounds.Height == 0) return;
            window.Position = this.PointToScreen(Bounds.Position);
            window.Width = Bounds.Width;
            window.Height = Bounds.Height;
            var dpi = window.Screens.ScreenFromVisual(window)?.PixelDensity;
            if (windowApiService != null)
            {
                windowApiService.BackgroundUpdate(_DwmHandle, (int)(window.Width * (dpi ?? 0)), (int)(window.Height * (dpi ?? 0)));
                //NativeMethods.SetWindowPos(HWND, NativeMethods.HWND_TOPMOST, window.Position.X, window.Position.Y, (int)window.Width, (int)window.Height, 0);
                //window.IsVisible = true;
            }
        }

        public void Show()
        {
            window?.Show();
        }

        public IntPtr Handle
        {
            get { return _Handle; }
        }

        public void Close()
        {
            window?.Close();
        }
    }
}
