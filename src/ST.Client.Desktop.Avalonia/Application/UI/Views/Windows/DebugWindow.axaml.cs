using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Styling;
using Avalonia.Input;

namespace System.Application.UI.Views.Windows
{
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();

            ExtendClientAreaToDecorationsHint = false;
            ExtendClientAreaTitleBarHeightHint = -1;
            TransparencyLevelHint = WindowTransparencyLevel.Mica;
            SystemDecorations = SystemDecorations.Full;
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.Default;

            //if (OperatingSystem2.IsWindows11AtLeast)
            //{
            //    AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().ForceNativeTitleBarToTheme(this, "Dark");
            //}

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
