using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System.Application.Services;
using System.Application.Settings;
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

#if WINDOWS
            //var wp = this.FindControl<WallpaperControl>("DesktopBackground");
            var panel = this.FindControl<Panel>("Panel");
            var wp = new WallpaperControl();
            wp.Bind(IsVisibleProperty, new Binding
            {
                Source = UISettings.EnableDesktopBackground,
                Mode = BindingMode.OneWay,
                Path = nameof(UISettings.EnableDesktopBackground.Value)
            });
            panel.Children.Insert(0, wp);
            _backHandle = wp.Handle;
#endif

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

        //        protected override void OnClosed(EventArgs e)
        //        {
        //#if !UI_DEMO
        //            if (StartupOptions.Value.HasNotifyIcon)
        //            {
        //                if (App.Current!.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        //                {
        //                    desktop.MainWindow = App.Instance.MainWindow = null;
        //                }

        //                if (ViewModel is not null)
        //                    foreach (var tab in ViewModel.TabItems)
        //                        tab.Deactivation();
        //            }
        //#endif
        //            base.OnClosed(e);
        //        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if (ViewModel is not null && ViewModel.SelectedItem.IsDeactivation)
            {
                ViewModel.SelectedItem.Activation();
            }

            IApplicationUpdateService.Instance.OnMainOpenTryShowNewVersionWindow();
        }

        //protected override void FluentWindow_Opened(object? sender, EventArgs e)
        //{
        //    if (ViewModel is not null)
        //        foreach (var tab in from tab in ViewModel.TabItems
        //                            where tab.IsDeactivation
        //                            select tab)
        //        {
        //            tab.Activation();
        //        }

        //    base.FluentWindow_Opened(sender, e);
        //}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}