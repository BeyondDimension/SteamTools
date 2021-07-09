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
                _isOpenWindow = false;
                e.Cancel = true;
                Hide();

                if (this.ViewModel is not null)
                    foreach (var tab in this.ViewModel.TabItems)
                    {
                        tab.Deactivation();
                    }
            }
#endif
            base.OnClosed(e);
        }

        protected override void FluentWindow_Opened(object? sender, EventArgs e)
        {
            if (this.ViewModel is not null)
                foreach (var tab in this.ViewModel.TabItems)
                    if (tab.IsDeactivation)
                        tab.Activation();

            base.FluentWindow_Opened(sender, e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}