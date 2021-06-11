using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;
using System.ComponentModel;

namespace System.Application.UI.Views
{
    public class MainWindow : FluentWindow<MainWindowViewModel>
    {
        public MainWindow() : base()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools2();
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
                _isOpenWindow = false;
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