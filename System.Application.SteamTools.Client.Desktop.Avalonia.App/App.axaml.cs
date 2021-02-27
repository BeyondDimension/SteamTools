using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System.Application.Mvvm;
using System.Application.UI.ViewModels;
using System.Application.UI.Views;
using System.Collections.Generic;
using System.IO;
using System.Properties;
using System.Windows;
using AvaloniaApplication = Avalonia.Application;

namespace System.Application.UI
{
    public class App : AvaloniaApplication, IDisposableHolder
    {
        public static App Instance => Current is App app ? app : throw new Exception("Impossible");

        [Obsolete("use IOPath.AppDataDirectory", true)]
        public DirectoryInfo LocalAppData => new DirectoryInfo(IOPath.AppDataDirectory);

        public static DirectoryInfo RootDirectory => new DirectoryInfo(AppContext.BaseDirectory);

        [Obsolete("use AppHelper.ProgramName", true)]
        public string ProgramName => AppHelper.ProgramName;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        public ContextMenu? NotifyIconContextMenu { get; private set; }

        public override void OnFrameworkInitializationCompleted()
        {
            // 在UI预览中，ApplicationLifetime 为 null
            ViewModelBase.IsInDesignMode = ApplicationLifetime == null;
            Startup.Init();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (!AppHelper.IsOfficialChannelPackage)
                {
                    MessageBox.Show("The program currently running is not from the official channel.", "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                #region NotifyIcon
                var notifyIcon = INotifyIcon.Instance;
                notifyIcon.ToolTipText = ThisAssembly.AssemblyTrademark;
                switch (DI.Platform)
                {
                    case Platform.Windows:
                    case Platform.Linux:
                        notifyIcon.IconPath = "avares://Steam++/Assets/Icon.ico";
                        break;
                    case Platform.Apple:
                        notifyIcon.IconPath = "avares://Steam++/Assets/Icon_16.png";
                        break;
                }

                notifyIcon.DoubleClick += (s, e) =>
                {
                    RestoreMainWindow();
                };

                NotifyIconContextMenu = new ContextMenu();
                var menuItems = new List<MenuItem>
                {
                    new MenuItem()
                    {
                        Header = "NotifyIconContextMenuItemHeaderRestore",
                        Command = ReactiveCommand.Create(RestoreMainWindow)
                    },
                    new MenuItem()
                    {
                        Header = "NotifyIconContextMenuItemHeaderExit",
                        Command = ReactiveCommand.Create(Exit)
                    }
                };
                NotifyIconContextMenu.Items = menuItems;
                notifyIcon.ContextMenu = NotifyIconContextMenu;
                notifyIcon.Visible = true;
                notifyIcon.Click += NotifyIcon_Click;
                notifyIcon.DoubleClick += NotifyIcon_Click;
                this.compositeDisposable.Add(() =>
                {
                    notifyIcon.IconPath = string.Empty;
                });
                #endregion

                desktop.MainWindow = MainWindow;
                desktop.Exit += Desktop_Exit;
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }



            base.OnFrameworkInitializationCompleted();
        }

        private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            this.compositeDisposable.Dispose();
        }

        private void NotifyIcon_Click(object? sender, EventArgs e)
        {
            RestoreMainWindow();
        }

        public static async void SetClipboardText(string s) => await Current.Clipboard.SetTextAsync(s);

        public Window? MainWindow { get; set; }

        /// <summary>
        /// Restores the app's main window by setting its <c>WindowState</c> to
        /// <c>WindowState.Normal</c> and showing the window.
        /// </summary>
        public void RestoreMainWindow()
        {
            Window? mainWindow = null;

            if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                mainWindow = desktop.MainWindow;
            }

            if (mainWindow == null)
            {
                mainWindow = MainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            }

            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.BringIntoView();
            mainWindow.ActivateWorkaround(); // Extension method hack because of https://github.com/AvaloniaUI/Avalonia/issues/2975
            mainWindow.Focus();

            // Again, ugly hack because of https://github.com/AvaloniaUI/Avalonia/issues/2994
            mainWindow.Width += 0.1;
            mainWindow.Width -= 0.1;
        }

        /// <summary>
        /// Exits the app by calling <c>Shutdown()</c> on the <c>IClassicDesktopStyleApplicationLifetime</c>.
        /// </summary>
        public static void Shutdown()
        {
            if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown(0);
            }
        }
        /// <summary>
        /// Exits the app by calling <c>Shutdown()</c> on the <c>IClassicDesktopStyleApplicationLifetime</c>.
        /// </summary>
        public static void Exit()
        {
            Shutdown();
        }

        #region IDisposable members
        public readonly CompositeDisposable compositeDisposable = new CompositeDisposable();
        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => this.compositeDisposable;

        void IDisposable.Dispose()
        {
            this.compositeDisposable.Dispose();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}