using Avalonia.Controls.Primitives;
using Avalonia.Platform;
using Avalonia.Styling;
using System;
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

        public FluentWindow()
        {
            Constructor();
            this.Opened += FluentWindow_Opened;
            this.PositionChanged += FluentWindow_PositionChanged;
        }

        public FluentWindow(IWindowImpl impl) : base(impl)
        {
            Constructor();
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

        bool _isOpenWindow;
        private void FluentWindow_Opened(object? sender, EventArgs e)
        {
            _isOpenWindow = true;
            if (this.DataContext is WindowViewModel vm)
            {
                if (vm.SizePosition.X > 0 && vm.SizePosition.Y > 0)
                    this.Position = new PixelPoint(vm.SizePosition.X, vm.SizePosition.Y);

                if (vm.SizePosition.Width > 0)
                    this.Width = vm.SizePosition.Width;

                if (vm.SizePosition.Height > 0)
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

        void Constructor()
        {
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaTitleBarHeightHint = -1;
            //SystemDecorations = SystemDecorations.BorderOnly;
            TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;

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
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            ExtendClientAreaChromeHints =
                ExtendClientAreaChromeHints.PreferSystemChrome |
                ExtendClientAreaChromeHints.OSXThickTitleBar;
        }
    }
}