using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.ComponentModel;

namespace System.Application.UI.Views
{
    public class MainWindow : FluentWindow
    {
        public MainWindow() : base()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
#if StartupTrace
            StartupTrace.Restart("MainWindow.ctor");
#endif
        }

        protected override void OnClosing(CancelEventArgs e)
        {
#if !UI_DEMO
            if (Startup.HasNotifyIcon)
            {
                e.Cancel = true;
                Hide();
            }
#endif
            base.OnClosed(e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}