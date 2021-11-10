using Avalonia.Controls.Primitives;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using ReactiveUI;
using System;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.Settings;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Controls;
using System.ComponentModel;

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

                if (OperatingSystem2.IsWindows10AtLeast)
                {
                    if (PlatformImpl is AvaloniaWin32WindowingPlatformImpl.WindowImpl2 cwi)
                    {
#pragma warning disable CA1416 // 验证平台兼容性
                        cwi.SetOwner(this);
#pragma warning restore CA1416 // 验证平台兼容性
                    }
                }
            }

            TransparencyLevelHint = (WindowTransparencyLevel)UISettings.WindowBackgroundMateria.Value;

            if (TransparencyLevelHint == WindowTransparencyLevel.Transparent ||
                TransparencyLevelHint == WindowTransparencyLevel.None ||
                TransparencyLevelHint == WindowTransparencyLevel.Blur)
            {
                PseudoClasses.Set(":transparent", true);
            }
            else
            {
                PseudoClasses.Set(":transparent", false);
            }

            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaTitleBarHeightHint = -1;
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

            if (isSaveStatus)
            {
                Opened += FluentWindow_Opened;
                Closing += FluentWindow_Closing;
                //PositionChanged += FluentWindow_PositionChanged;
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

        private void FluentWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (IsHideWindow || e.Cancel)
                return;
            if (DataContext is WindowViewModel vm)
            {
                vm.SizePosition.X = Position.X;
                vm.SizePosition.Y = Position.Y;
            }
        }

        //private void FluentWindow_PositionChanged(object? sender, PixelPointEventArgs e)
        //{
        //    if (IsHideWindow)
        //        return;
        //    if (DataContext is WindowViewModel vm)
        //    {
        //        vm.SizePosition.X = e.Point.X;
        //        vm.SizePosition.Y = e.Point.Y;
        //    }
        //}

        protected virtual void FluentWindow_Opened(object? sender, EventArgs e)
        {
            if (DataContext is WindowViewModel vm)
            {
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

                if (CanResize && !IsHideWindow)
                {
                    if (vm.SizePosition.Width > 0 &&
                        Screens.Primary.WorkingArea.Width >= vm.SizePosition.Width)
                        Width = vm.SizePosition.Width;

                    if (vm.SizePosition.Height > 0 &&
                        Screens.Primary.WorkingArea.Height >= vm.SizePosition.Height)
                        Height = vm.SizePosition.Height;

                    if (ClientSize.Width != Width || ClientSize.Height != Height)
                    {
                        HandleResized(new Size(Width += 16, Height += 8), PlatformResizeReason.Application);
                    }

                    this.WhenAnyValue(x => x.ClientSize)
                        .Subscribe(x =>
                        {
                            vm.SizePosition.Width = x.Width;
                            vm.SizePosition.Height = x.Height;
                        });
                }
            }

            IsHideWindow = false;
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

        public void Resized(Size clientSize, PlatformResizeReason reason)
        {
            HandleResized(clientSize, reason);
        }

        Window ICoreWindow.Window => this;

        MinMaxCloseControl? ICoreWindow.SystemCaptionButtons => _systemCaptionButtons;

        public bool IsNewSizeWindow { get; set; }

        public bool IsHideWindow { get; set; }
        #endregion
    }

    public abstract class FluentWindow : FluentWindow<WindowViewModel>
    {
        public FluentWindow(bool isSaveStatus = true) : base(isSaveStatus)
        {

        }
    }
}