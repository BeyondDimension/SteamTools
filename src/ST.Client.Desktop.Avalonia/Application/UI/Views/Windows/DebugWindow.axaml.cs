using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Styling;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;

namespace System.Application.UI.Views.Windows
{
    public partial class DebugWindow : CoreWindow
    {
        public DebugWindow()
        {
            InitializeComponent();

            //ExtendClientAreaToDecorationsHint = false;
            //ExtendClientAreaTitleBarHeightHint = -1;
            TransparencyLevelHint = WindowTransparencyLevel.Mica;
            SystemDecorations = SystemDecorations.BorderOnly;
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.SystemChrome;

            if (OperatingSystem2.IsWindows)
            {
                AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().ForceWin32WindowToTheme(this);
            }

            DragDrop.SetAllowDrop(this, true);
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
