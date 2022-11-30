using Avalonia.Platform;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using ReactiveUI;
using System;
using System.Application.Settings;
using System.Application.UI.ViewModels;
using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace Avalonia.Controls
{
    public abstract class FluentWindow<TViewModel> : ReactiveCoreWindow<TViewModel>, IStyleable where TViewModel : class
    {
        Type IStyleable.StyleKey => typeof(Window);

        public FluentWindow() : this(true)
        {

        }

        public FluentWindow(bool isSaveStatus)
        {
#if DEBUG
            if (Design.IsDesignMode)
                return;
#endif

            //#if WINDOWS
            //            PseudoClasses.Set(":windows", true);

            //#pragma warning disable CA1416 // 验证平台兼容性
            //            if (OperatingSystem2.IsWindows10AtLeast)
            //            {
            //                if (PlatformImpl is AvaloniaWin32WindowingPlatformImpl.Window10Impl cwi)
            //                {
            //                    cwi.SetOwner(this);
            //                }
            //            }
            //#pragma warning restore CA1416 // 验证平台兼容性
            //#endif

            TransparencyLevelHint = (WindowTransparencyLevel)UISettings.WindowBackgroundMateria.Value;

            if (TransparencyLevelHint == WindowTransparencyLevel.Transparent ||
                TransparencyLevelHint == WindowTransparencyLevel.Blur)
            {
                PseudoClasses.Set(":transparent", true);
            }
            else
            {
                PseudoClasses.Set(":transparent", false);
            }

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

            Opened += (s, e) =>
            {
                if (TitleBar != null)
                {
                    TitleBar.ExtendsContentIntoTitleBar = true;

                    //TitleBar.LayoutMetricsChanged += OnApplicationTitleBarLayoutMetricsChanged;

                    //if (this.FindControl<Control>("TitleBarHost") is IControl t)
                    //{
                    //    SetTitleBar(t);
                    //}
                }

                if (OperatingSystem2.IsWindows())
                {
                    AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>()?.ForceWin32WindowToTheme(this);
                }
            };

#pragma warning disable CA1416 // 验证平台兼容性
            if (OperatingSystem2.IsWindows())
            {
                if (OperatingSystem2.IsWindows10AtLeast())
                {
                    //ExtendClientAreaToDecorationsHint = true;
                    //ExtendClientAreaTitleBarHeightHint = -1;

                    ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                }
                else
                {
                    ExtendClientAreaToDecorationsHint = true;
                    ExtendClientAreaTitleBarHeightHint = System.Application.UI.Views.Controls.TitleBar.DefaultHeight;

                    ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome;
                    PseudoClasses.Add(":windows7");
                }
                PseudoClasses.Add(":windows");
            }
            else if (OperatingSystem2.IsMacOS())
            {
                ExtendClientAreaToDecorationsHint = true;
                ExtendClientAreaTitleBarHeightHint = System.Application.UI.Views.Controls.TitleBar.DefaultHeight;

                ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome;
            }
            else
            {
                ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
            }
#pragma warning restore CA1416 // 验证平台兼容性

            //if (!ViewModelBase.IsInDesignMode)
            //{
            //    //if (OperatingSystem2.IsWindows())
            //    //{
            //    //    //                    if (OperatingSystem2.IsWindows7())
            //    //    //                    {
            //    //    //#pragma warning disable CA1416 // 验证平台兼容性
            //    //    //                        IPlatformService.Instance.FixAvaloniaFluentWindowStyleOnWin7(PlatformImpl.Handle.Handle);
            //    //    //#pragma warning restore CA1416 // 验证平台兼容性
            //    //    //                    }
            //    //    //else if (OperatingSystem2.IsWindows10AtLeast())
            //    //    //{
            //    //    //    AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().ForceNativeTitleBarToTheme(this);
            //    //    //}
            //    //}
            //}
        }

        private void FluentWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (DataContext is WindowViewModel vm && WindowState == WindowState.Normal)
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
                var primaryScreen = Screens?.Primary;
                if (primaryScreen != null)
                {
                    var primaryScreenBounds = primaryScreen.Bounds;

                    if (CanResize && IsHideWindow == false)
                    {
                        if (vm.SizePosition.Width > 0 &&
                            primaryScreenBounds.Width >= vm.SizePosition.Width)
                            Width = vm.SizePosition.Width;

                        if (vm.SizePosition.Height > 0 &&
                            primaryScreenBounds.Height >= vm.SizePosition.Height)
                            Height = vm.SizePosition.Height;

                        if (ClientSize.Width != Width || ClientSize.Height != Height)
                        {
                            HandleResized(new Size(Width, Height), PlatformResizeReason.Application);
                        }

                        if (vm.SizePosition.X > 0 && vm.SizePosition.Y > 0)
                        {
                            var leftTopPoint = new PixelPoint(vm.SizePosition.X, vm.SizePosition.Y);
                            var rightBottomPoint = new PixelPoint(vm.SizePosition.X + (int)Width, vm.SizePosition.Y + (int)Height);
                            if (primaryScreenBounds.Contains(leftTopPoint) &&
                               primaryScreenBounds.Contains(rightBottomPoint))
                            {
                                Position = leftTopPoint;
                            }
                        }

                        this.WhenAnyValue(x => x.ClientSize)
                            .Subscribe(x =>
                            {
                                if (this.WindowState == WindowState.Normal)
                                {
                                    vm.SizePosition.Width = x.Width;
                                    vm.SizePosition.Height = x.Height;
                                }
                            });
                    }
                }
            }

            IsHideWindow = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (DataContext is IDisposable disposable) disposable.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        #region CoreWindow

        public void Resized(Size clientSize, PlatformResizeReason reason)
        {
            HandleResized(clientSize, reason);
        }

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