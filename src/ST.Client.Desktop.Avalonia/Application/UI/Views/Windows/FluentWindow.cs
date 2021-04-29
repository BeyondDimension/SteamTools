using Avalonia.Controls.Primitives;
using Avalonia.Platform;
using Avalonia.Styling;
using System;
using System.Application.UI.Resx;
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
        }

        public FluentWindow(IWindowImpl impl) : base(impl)
        {
            Constructor();
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