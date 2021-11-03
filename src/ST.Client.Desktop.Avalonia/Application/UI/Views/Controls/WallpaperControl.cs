using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using System;
using System.Application.Settings;
using System.Application.Services;

namespace System.Application.UI.Views.Controls
{
    public class WallpaperControl : TemplatedControl
    {
        readonly INativeWindowApiService windowApiService = INativeWindowApiService.Instance;

        Window window;
        Window ParentWindow;
        IntPtr _Handle;
        IntPtr _DwmHandle;
        public WallpaperControl()
        {
            //this.InitializeComponent();

            if (OperatingSystem2.IsWindows)
            {
                this.GetObservable(IsVisibleProperty)
                    .Subscribe(x =>
                    {
                        if (x)
                        {
                            if (window == null)
                            {
                                this.LayoutUpdated += EmptyControl_LayoutUpdated;
                                this.AttachedToVisualTree += EmptyControl_AttachedToVisualTree;
                                this.DetachedFromVisualTree += EmptyControl_DetachedFromVisualTree;
                                window = new Window
                                {
                                    Width = this.Bounds.Width,
                                    Height = this.Bounds.Height,
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
                                this.LayoutUpdated -= EmptyControl_LayoutUpdated;
                                this.AttachedToVisualTree -= EmptyControl_AttachedToVisualTree;
                                this.DetachedFromVisualTree -= EmptyControl_DetachedFromVisualTree;
                                window = null;
                            }
                        }
                    });
            }
        }

        private void Window_GotFocus(object? sender, Avalonia.Input.GotFocusEventArgs e)
        {
            ParentWindow.Focus();
        }

        private void EmptyControl_DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            windowApiService.ReleaseBackground(_DwmHandle);
            ParentWindow.PositionChanged -= Parent_PositionChanged;
            ParentWindow.Closing -= ParentWindow_Closing;
            ParentWindow.GotFocus -= ParentWindow_GotFocus;
            Close();
        }

        private void EmptyControl_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            ParentWindow = (Window)e.Root;
            ParentWindow.Topmost = true;
            Show();
            ParentWindow.Topmost = false;
            ParentWindow.PositionChanged += Parent_PositionChanged;
            ParentWindow.Closing += ParentWindow_Closing;
            ParentWindow.GotFocus += ParentWindow_GotFocus;
            ParentWindow.Opened += ParentWindow_Opened;
            _Handle = window.PlatformImpl.Handle.Handle;
            windowApiService.SetWindowPenetrate(_Handle);
            //windowApiService.SetParentWindow(_Handle, ParentWindow.PlatformImpl.Handle.Handle);
            _DwmHandle = windowApiService.SetDesktopBackgroundToWindow(
                _Handle, (int)window.Width, (int)window.Height);
        }

        private void ParentWindow_Opened(object? sender, EventArgs e)
        {
            if (window != null)
                Show();
        }

        private void ParentWindow_GotFocus(object? sender, Avalonia.Input.GotFocusEventArgs e)
        {
            window.Topmost = true;
            window.Topmost = false;
            ParentWindow.Topmost = true;
            ParentWindow.Topmost = false;
        }

        private void ParentWindow_Closing(object? sender, ComponentModel.CancelEventArgs e)
        {
            if (window != null)
                window.Hide();
        }

        private void Parent_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            window.Position = this.PointToScreen(this.Bounds.Position);
        }

        private void EmptyControl_LayoutUpdated(object? sender, System.EventArgs e)
        {
            window.Position = this.PointToScreen(this.Bounds.Position);
            window.Width = this.Bounds.Width;
            window.Height = this.Bounds.Height;
            windowApiService.BackgroundUpdate(_DwmHandle, (int)window.Width, (int)window.Height);
            //NativeMethods.SetWindowPos(HWND, NativeMethods.HWND_TOPMOST, window.Position.X, window.Position.Y, (int)window.Width, (int)window.Height, 0);
        }

        public WindowState WindowState
        {
            get { return window.WindowState; }
            set { window.WindowState = value; }
        }

        public void Show()
        {
            window.Show();
        }

        public IntPtr Handle
        {
            get { return _Handle; }
        }

        public void Close()
        {
            window.Close();
        }
    }
}
