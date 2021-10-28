using Avalonia.Controls.Primitives;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using ReactiveUI;
using System;
using System.Application.Settings;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Application.Services.Implementation;
using System.Application.UI.Views.Controls;

// ReSharper disable once CheckNamespace
namespace Avalonia.Controls
{
    public abstract class FluentWindow<TViewModel> : ReactiveWindow<TViewModel>, IStyleable, ICoreWindow where TViewModel : class
    {
        Control? _defaultTitleBar;
        MinMaxCloseControl? _systemCaptionButtons;

        Type IStyleable.StyleKey => typeof(Window);

        public FluentWindow() : this(true)
        {

        }

        public FluentWindow(bool isSaveStatus)
        {
            if (OperatingSystem2.IsWindows)
            {
                PseudoClasses.Set(":windows", true);

                if (this.PlatformImpl is AvaloniaWin32WindowingPlatformImpl.WindowImpl2 cwi)
                {
                    cwi.SetOwner(this);
                }

                ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
                ExtendClientAreaToDecorationsHint = true;
                //TransparencyLevelHint = WindowTransparencyLevel.Mica;
                TransparencyLevelHint = (WindowTransparencyLevel)UISettings.WindowBackgroundMateria.Value;
            }

            //if (OperatingSystem2.IsWindows)
            //{
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaTitleBarHeightHint = -1;

            //}
            TransparencyLevelHint = (WindowTransparencyLevel)UISettings.WindowBackgroundMateria.Value;
            SystemDecorations = SystemDecorations.Full;

            this.GetObservable(WindowStateProperty)
            .Subscribe(x =>
            {
                PseudoClasses.Set(":maximized", x == WindowState.Maximized);
                PseudoClasses.Set(":fullscreen", x == WindowState.FullScreen);
            });

            //this.GetObservable(IsExtendedIntoWindowDecorationsProperty)
            //    .Subscribe(x =>
            //    {
            //        SystemDecorations = SystemDecorations.Full;
            //        //TransparencyLevelHint = WindowTransparencyLevel.AcrylicBlur;
            //    });

            if (isSaveStatus && CanResize)
            {
                Opened += FluentWindow_Opened;
                PositionChanged += FluentWindow_PositionChanged;
            }

            if (OperatingSystem2.IsWindows)
            {
                ExtendClientAreaChromeHints =
                    ExtendClientAreaChromeHints.PreferSystemChrome;
            }
            else if (OperatingSystem2.IsMacOS)
            {
                ExtendClientAreaChromeHints =
                    ExtendClientAreaChromeHints.PreferSystemChrome;
            }
            else
            {
                ExtendClientAreaChromeHints =
                    ExtendClientAreaChromeHints.SystemChrome;
            }

            if (!ViewModelBase.IsInDesignMode)
            {
                if (OperatingSystem2.IsWindows)
                {
                    if (OperatingSystem2.IsWindows7)
                    {
#pragma warning disable CA1416 // 验证平台兼容性
                        IPlatformService.Instance.FixAvaloniaFluentWindowStyleOnWin7(PlatformImpl.Handle.Handle);
#pragma warning restore CA1416 // 验证平台兼容性
                    }
                    //else if (OperatingSystem2.IsWindows10AtLeast)
                    //{
                    //    AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().ForceNativeTitleBarToTheme(this);
                    //}
                }
            }
        }

        private void FluentWindow_PositionChanged(object? sender, PixelPointEventArgs e)
        {
            if (_isOpenWindow == false)
                return;
            if (DataContext is WindowViewModel vm)
            {
                vm.SizePosition.X = e.Point.X;
                vm.SizePosition.Y = e.Point.Y;
            }
        }

        protected bool _isOpenWindow;
        protected virtual void FluentWindow_Opened(object? sender, EventArgs e)
        {
            _isOpenWindow = true;
            if (DataContext is WindowViewModel vm)
            {
                if (vm.SizePosition.Width > 0
                    && Screens.Primary.WorkingArea.Width >= vm.SizePosition.Width)
                    Width = vm.SizePosition.Width;

                if (vm.SizePosition.Height > 0
                    && Screens.Primary.WorkingArea.Height >= vm.SizePosition.Height)
                    Height = vm.SizePosition.Height;

                if (vm.SizePosition.X > 0 && vm.SizePosition.Y > 0)
                {
                    var leftTopPoint = new PixelPoint(vm.SizePosition.X, vm.SizePosition.Y);
                    var rightBottomPoint = new PixelPoint(vm.SizePosition.X + (int)Width, vm.SizePosition.Y + (int)Height);
                    if (Screens.Primary.WorkingArea.Contains(leftTopPoint) &&
                        Screens.Primary.WorkingArea.Contains(rightBottomPoint))
                    {
                        Position = leftTopPoint;
                    }
                }

                HandleResized(new Size(Width, Height), PlatformResizeReason.Application);

                this.WhenAnyValue(x => x.ClientSize)
                    .Subscribe(x =>
                    {
                        vm.SizePosition.Width = x.Width;
                        vm.SizePosition.Height = x.Height;
                    });

                //this.GetObservable(WidthProperty).Subscribe(v =>
                //{
                //    vm.SizePosition.Width = v;
                //});
                //this.GetObservable(HeightProperty).Subscribe(v =>
                //{
                //    vm.SizePosition.Height = v;
                //});
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _systemCaptionButtons = e.NameScope.Find<MinMaxCloseControl>("SystemCaptionButtons");
            if (_systemCaptionButtons != null)
            {
                _systemCaptionButtons.Height = 30;
            }

            _defaultTitleBar = e.NameScope.Find<Control>("DefaultTitleBar");
            if (_defaultTitleBar != null)
            {
                _defaultTitleBar.Margin = new Thickness(0, 0, 138 /* 46x3 */, 0);
                _defaultTitleBar.Height = 30;
            }

        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (DataContext is IDisposable disposable) disposable.Dispose();
        }

        #region CoreWindow

        Window ICoreWindow.Window => this;

        MinMaxCloseControl? ICoreWindow.SystemCaptionButtons => _systemCaptionButtons;

        #endregion
    }

    public abstract class FluentWindow : FluentWindow<WindowViewModel>
    {
        public FluentWindow(bool isSaveStatus = true) : base(isSaveStatus)
        {

        }
    }
}