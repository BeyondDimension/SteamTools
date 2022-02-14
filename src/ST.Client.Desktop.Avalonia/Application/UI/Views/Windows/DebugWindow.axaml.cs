using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Styling;

namespace System.Application.UI.Views.Windows
{
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();

            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaTitleBarHeightHint = -1;
            TransparencyLevelHint = WindowTransparencyLevel.Transparent;
            SystemDecorations = SystemDecorations.Full;
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.PreferSystemChrome;

            //if (OperatingSystem2.IsWindows11AtLeast)
            //{
            //    AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().ForceNativeTitleBarToTheme(this, "Dark");
            //}
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
