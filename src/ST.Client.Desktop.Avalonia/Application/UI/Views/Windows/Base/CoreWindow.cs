using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using Avalonia.Rendering;
using Avalonia.Styling;
using FluentAvalonia.Interop;
using ReactiveUI;
using System;
using System.Application.Services.Implementation;
using System.Application.Settings;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Controls;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Avalonia.Controls
{
    // Special Win32 window impl for a better extended window frame.
    // Not intended for outside use

    public static class WindowImplSolver
    {
        public static IWindowImpl GetWindowImpl()
        {
            if (Design.IsDesignMode)
                return PlatformManager.CreateWindow();

            if (OperatingSystem2.IsWindows10AtLeast)
            {
                return new AvaloniaWin32WindowImpl();
            }

            return PlatformManager.CreateWindow();
        }
    }


    public abstract class CoreWindow : Window, IStyleable
    {
        public CoreWindow()
        : base(WindowImplSolver.GetWindowImpl())
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                PseudoClasses.Set(":windows", true);

                if (this.PlatformImpl is AvaloniaWin32WindowImpl cwi)
                {
                    cwi.SetOwner(this);
                }

                ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
                ExtendClientAreaToDecorationsHint = true;
                //TransparencyLevelHint = WindowTransparencyLevel.Mica;
                TransparencyLevelHint = (WindowTransparencyLevel)UISettings.WindowBackgroundMateria.Value;
            }

        }

        Type IStyleable.StyleKey => typeof(Window);

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

        internal bool HitTestTitleBarRegion(Point windowPoint)
        {
            return _defaultTitleBar?.HitTestCustom(windowPoint) ?? false;
        }

        internal bool HitTestCaptionButtons(Point pos)
        {
            if (pos.Y < 1)
                return false;

            var result = _systemCaptionButtons?.HitTestCustom(pos) ?? false;
            return result;
        }

        internal bool HitTestMaximizeButton(Point pos)
        {
            return _systemCaptionButtons.HitTestMaxButton(pos);
        }

        internal void FakeMaximizeHover(bool hover)
        {
            _systemCaptionButtons.FakeMaximizeHover(hover);
        }

        internal void FakeMaximizePressed(bool pressed)
        {
            _systemCaptionButtons.FakeMaximizePressed(pressed);
        }

        internal void FakeMaximizeClick()
        {
            _systemCaptionButtons.FakeMaximizeClick();
        }

        private Control? _defaultTitleBar;
        private MinMaxCloseControl? _systemCaptionButtons;
    }

    public class CoreWindow<TViewModel> : CoreWindow, IViewFor<TViewModel> where TViewModel : class
    {
        public static readonly StyledProperty<TViewModel?> ViewModelProperty = AvaloniaProperty
           .Register<ReactiveWindow<TViewModel>, TViewModel?>(nameof(ViewModel));

        /// <summary>
        /// The ViewModel.
        /// </summary>
        public TViewModel? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TViewModel?)value;
        }

        private void OnDataContextChanged(object? value)
        {
            if (value is TViewModel viewModel)
            {
                ViewModel = viewModel;
            }
            else
            {
                ViewModel = null;
            }
        }

        private void OnViewModelChanged(object? value)
        {
            if (value == null)
            {
                ClearValue(DataContextProperty);
            }
            else if (DataContext != value)
            {
                DataContext = value;
            }
        }

        public CoreWindow()
        {

            // This WhenActivated block calls ViewModel's WhenActivated
            // block if the ViewModel implements IActivatableViewModel.
            this.WhenActivated(disposables => { });
            this.GetObservable(DataContextProperty).Subscribe(OnDataContextChanged);
            this.GetObservable(ViewModelProperty).Subscribe(OnViewModelChanged);
        }
    }

}
