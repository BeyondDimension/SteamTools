using Avalonia.Controls.Primitives;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using System;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Avalonia.Controls
{
    public class FluentWindow : Window, IStyleable
    {
        Type IStyleable.StyleKey => typeof(Window);

        public FluentWindow(bool isSaveStatus = true)
        {
            //if (DI.Platform == System.Platform.Windows)
            //{
            ExtendClientAreaToDecorationsHint = true;
            //ExtendClientAreaTitleBarHeightHint = -1;

            //}
            //SystemDecorations = SystemDecorations.BorderOnly;
            TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;

            if (DI.Platform == System.Platform.Windows)
            {
                DI.Get<IDesktopPlatformService>().FixFluentWindowStyleOnWin7(PlatformImpl.Handle.Handle);
            }

            this.GetObservable(WindowStateProperty)
            .Subscribe(x =>
            {
                PseudoClasses.Set(":maximized", x == WindowState.Maximized);
                PseudoClasses.Set(":fullscreen", x == WindowState.FullScreen);
            });

            this.GetObservable(IsExtendedIntoWindowDecorationsProperty)
                .Subscribe(x =>
                {
                    if (!x)
                    {
                        SystemDecorations = SystemDecorations.Full;
                        //TransparencyLevelHint = WindowTransparencyLevel.Blur;
                    }
                });

            if (isSaveStatus)
            {
                this.Opened += FluentWindow_Opened;
                this.PositionChanged += FluentWindow_PositionChanged;
            }
        }

        private void FluentWindow_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            if (_isOpenWindow == false)
                return;
            if (this.DataContext is WindowViewModel vm)
            {
                vm.SizePosition.X = e.Point.X;
                vm.SizePosition.Y = e.Point.Y;
            }
        }

        protected bool _isOpenWindow;
        private void FluentWindow_Opened(object? sender, EventArgs e)
        {
            _isOpenWindow = true;
            if (this.DataContext is WindowViewModel vm)
            {
                if (vm.SizePosition.X > 0 && vm.SizePosition.Y > 0)
                {
                    var point = new PixelPoint(vm.SizePosition.X, vm.SizePosition.Y);
                    if (Screens.Primary.WorkingArea.Contains(point))
                        this.Position = point;
                }

                if (vm.SizePosition.Width > 0
                    && Screens.Primary.WorkingArea.Width >= vm.SizePosition.Width)
                    this.Width = vm.SizePosition.Width;

                if (vm.SizePosition.Height > 0
                    && Screens.Primary.WorkingArea.Height >= vm.SizePosition.Height)
                    this.Height = vm.SizePosition.Height;

                HandleResized(new Size(this.Width, this.Height));

                this.GetObservable(WidthProperty).Subscribe(v =>
                {
                    vm.SizePosition.Width = v;
                });
                this.GetObservable(HeightProperty).Subscribe(v =>
                {
                    vm.SizePosition.Height = v;
                });
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (DI.Platform == System.Platform.Windows)
            {
                ExtendClientAreaChromeHints =
                ExtendClientAreaChromeHints.PreferSystemChrome;
            }
            else if (DI.Platform == System.Platform.Apple)
            {
                ExtendClientAreaChromeHints =
                    ExtendClientAreaChromeHints.OSXThickTitleBar;
            }
            else
            {
                ExtendClientAreaChromeHints =
                    ExtendClientAreaChromeHints.SystemChrome;
            }
        }
    }

    public class FluentWindow<TViewModel> : ReactiveWindow<TViewModel>, IStyleable where TViewModel : class
    {
        Type IStyleable.StyleKey => typeof(Window);

        public FluentWindow(bool isSaveStatus = true)
        {
            //if (DI.Platform == System.Platform.Windows)
            //{
            ExtendClientAreaToDecorationsHint = true;
            //ExtendClientAreaTitleBarHeightHint = -1;

            //}
            //SystemDecorations = SystemDecorations.BorderOnly;
            TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;

            if (DI.Platform == System.Platform.Windows)
            {
                DI.Get<IDesktopPlatformService>().FixFluentWindowStyleOnWin7(PlatformImpl.Handle.Handle);
            }

            this.GetObservable(WindowStateProperty)
            .Subscribe(x =>
            {
                PseudoClasses.Set(":maximized", x == WindowState.Maximized);
                PseudoClasses.Set(":fullscreen", x == WindowState.FullScreen);
            });

            this.GetObservable(IsExtendedIntoWindowDecorationsProperty)
                .Subscribe(x =>
                {
                    if (!x)
                    {
                        SystemDecorations = SystemDecorations.Full;
                        //TransparencyLevelHint = WindowTransparencyLevel.Blur;
                    }

                });

            if (isSaveStatus)
            {
                this.Opened += FluentWindow_Opened;
                if (this.CanResize)
                    this.PositionChanged += FluentWindow_PositionChanged;
            }
        }

        private void FluentWindow_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            if (_isOpenWindow == false)
                return;
            if (this.DataContext is WindowViewModel vm)
            {
                vm.SizePosition.X = e.Point.X;
                vm.SizePosition.Y = e.Point.Y;
            }
        }

        protected bool _isOpenWindow;
        protected virtual void FluentWindow_Opened(object? sender, EventArgs e)
        {
            _isOpenWindow = true;
            if (this.DataContext is WindowViewModel vm)
            {
                if (vm.SizePosition.X > 0 && vm.SizePosition.Y > 0)
                {
                    var point = new PixelPoint(vm.SizePosition.X, vm.SizePosition.Y);
                    if (Screens.Primary.WorkingArea.Contains(point))
                        this.Position = point;
                }

                if (vm.SizePosition.Width > 0
                    && Screens.Primary.WorkingArea.Width >= vm.SizePosition.Width)
                    this.Width = vm.SizePosition.Width;

                if (vm.SizePosition.Height > 0
                    && Screens.Primary.WorkingArea.Height >= vm.SizePosition.Height)
                    this.Height = vm.SizePosition.Height;

                HandleResized(new Size(this.Width, this.Height));

                this.GetObservable(WidthProperty).Subscribe(v =>
                {
                    vm.SizePosition.Width = v;
                });
                this.GetObservable(HeightProperty).Subscribe(v =>
                {
                    vm.SizePosition.Height = v;
                });
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (DI.Platform == System.Platform.Windows)
            {
                ExtendClientAreaChromeHints =
                ExtendClientAreaChromeHints.PreferSystemChrome;
            }
            else if (DI.Platform == System.Platform.Apple)
            {
                ExtendClientAreaChromeHints =
                    ExtendClientAreaChromeHints.OSXThickTitleBar;
            }
            else
            {
                ExtendClientAreaChromeHints =
                    ExtendClientAreaChromeHints.SystemChrome;
            }
        }
    }
}