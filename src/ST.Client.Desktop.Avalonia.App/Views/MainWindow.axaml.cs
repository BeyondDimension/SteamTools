using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Styling;
using ReactiveUI;
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

            //var background = this.FindControl<EmptyControl>("DesktopBackground");
            //_backHandle = background.Handle;

            //if (OperatingSystem2.IsWindows && !OperatingSystem2.IsWindows11AtLeast)
            //{
            //    TransparencyLevelHint = WindowTransparencyLevel.Transparent;
            //}
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
                _isOpenWindow = false;
                e.Cancel = true;
                Hide();

                if (ViewModel is not null)
                    foreach (var tab in ViewModel.TabItems)
                        tab.Deactivation();
            }
#endif
            base.OnClosed(e);
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