using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Controls;
using System.ComponentModel;
using System.Linq;

namespace System.Application.UI.Views
{
    public class MainWindow : FluentWindow<MainWindowViewModel>
    {
        readonly IntPtr _backHandle;

        public IntPtr BackHandle => _backHandle;

        public MainWindow() : base()
        {
            InitializeComponent();

            var background = this.FindControl<WallpaperControl>("DesktopBackground");
            _backHandle = background.Handle;

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
            if (StartupOptions.Value.HasNotifyIcon)
            {
                IsHideWindow = true;
                e.Cancel = true;
                Hide();

                if (ViewModel is not null)
                    foreach (var tab in ViewModel.TabItems)
                        tab.Deactivation();
            }
#endif
            base.OnClosing(e);
        }

        protected override void FluentWindow_Opened(object? sender, EventArgs e)
        {
            if (ViewModel is not null)
                foreach (var tab in from tab in ViewModel.TabItems
                                    where tab.IsDeactivation
                                    select tab)
                {
                    tab.Activation();
                }

            base.FluentWindow_Opened(sender, e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}