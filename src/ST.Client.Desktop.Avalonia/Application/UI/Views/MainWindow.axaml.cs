using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Controls;
using System.ComponentModel;
using System.Properties;

namespace System.Application.UI.Views
{
    public class AppSplashScreen : IApplicationSplashScreen
    {
        public AppSplashScreen()
        {
            using (var s = AvaloniaLocator.Current.GetService<IAssetLoader>()?.Open(new Uri("avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Assets/Icon.ico")))
                AppIcon = new Bitmap(s);
        }

        string IApplicationSplashScreen.AppName { get; } = ThisAssembly.AssemblyProduct;

        public IImage AppIcon { get; }

        object IApplicationSplashScreen.SplashScreenContent { get; }

        int IApplicationSplashScreen.MinimumShowTime => 0;

        void IApplicationSplashScreen.RunTasks()
        {
            if (IApplication.IsDesktopPlatform)
            {
                AdvertiseService.Current.InitAdvertise();
            }
        }
    }

    public class MainWindow : FluentWindow<MainWindowViewModel>
    {
        readonly IntPtr _backHandle;

        public IntPtr BackHandle => _backHandle;

        public MainWindow() : base()
        {
            InitializeComponent();
            SplashScreen = new AppSplashScreen();

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
#if StartWatchTrace
            StartWatchTrace.Record("MainWindow.ctor");
#endif
        }

        protected override void OnClosing(CancelEventArgs e)
        {
#if !UI_DEMO
            if (OperatingSystem2.IsWindows() && StartupOptions.Value.HasNotifyIcon)
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

        protected override void OnClosed(EventArgs e)
        {
#if !UI_DEMO
            if (!OperatingSystem2.IsWindows() && StartupOptions.Value.HasNotifyIcon)
            {
                if (App.Current!.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.MainWindow = App.Instance.MainWindow = null;
                }

                if (ViewModel is not null)
                    foreach (var tab in ViewModel.TabItems)
                        tab.Deactivation();
            }
#endif
            base.OnClosed(e);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if (ViewModel?.SelectedItem?.IsDeactivation == true)
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