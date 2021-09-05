using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using System;
using System.Application.Models.Settings;
using System.Application.Services;

namespace System.Application.UI.Views.Controls
{
    public class EmptyControl : TemplatedControl
    {
        Window window;
        Window Parent;
        IntPtr _Handle;
        public EmptyControl()
        {
            //this.InitializeComponent();

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
                                Background = Avalonia.Media.Brushes.Transparent,
                                WindowStartupLocation = WindowStartupLocation.Manual,
                                WindowState = WindowState.Normal,
                                ExtendClientAreaToDecorationsHint = true,
                                ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.PreferSystemChrome,
                                SystemDecorations = SystemDecorations.Full,
                                CanResize = false,
                                TransparencyLevelHint = WindowTransparencyLevel.Transparent,
                                ShowInTaskbar = false,
                                Focusable = false
                            };
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

        private void EmptyControl_DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (OperatingSystem2.IsWindows && UISettings.EnableDesktopBackground.Value)
            {
                DI.Get<ISystemWindowApiService>().ReleaseBackground(_Handle);
                Parent.PositionChanged -= Parent_PositionChanged;
                Close();
            }
        }

        private void EmptyControl_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (OperatingSystem2.IsWindows && UISettings.EnableDesktopBackground.Value)
            {
                Show();
                Parent = (Window)e.Root;
                Parent.PositionChanged += Parent_PositionChanged;
                _Handle = Handle;
                _Handle = DI.Get<ISystemWindowApiService>().SetDesktopBackgroundToWindow(
                    _Handle, (int)window.Width, (int)window.Height);

                DI.Get<ISystemWindowApiService>().SetWindowPenetrate(Handle);
            }
        }

        private void Parent_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            window.Position = this.PointToScreen(this.Bounds.Position);
        }

        private void EmptyControl_LayoutUpdated(object? sender, System.EventArgs e)
        {
            if (OperatingSystem2.IsWindows && UISettings.EnableDesktopBackground.Value)
            {
                window.Position = this.PointToScreen(this.Bounds.Position);
                window.Width = this.Bounds.Width;
                window.Height = this.Bounds.Height;
                DI.Get<ISystemWindowApiService>().BackgroundUpdate(_Handle, (int)window.Width, (int)window.Height);
            }
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
            get { return window.PlatformImpl.Handle.Handle; }
        }

        public void Close()
        {
            window.Close();
        }
    }
}
