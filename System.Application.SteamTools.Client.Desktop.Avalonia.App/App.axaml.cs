#pragma warning disable CA1416 // 验证平台兼容性
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
using ShutdownMode = Avalonia.Controls.ShutdownMode;
using Window = Avalonia.Controls.Window;
using WindowState = Avalonia.Controls.WindowState;
#if WINDOWS
using System.Windows.Shell;
using WpfApplication = System.Windows.Application;
#endif

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

        static void IsNotOfficialChannelPackageWarning()
        {
            var text = "The program currently running is not from the official channel.";
            var title = "Warning";
            MessageBoxCompat.Show(text, title, MessageBoxButtonCompat.OK, MessageBoxImageCompat.Warning);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // 在UI预览中，ApplicationLifetime 为 null
            ViewModelBase.IsInDesignMode = ApplicationLifetime == null;
            Startup.Init();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (!AppHelper.IsOfficialChannelPackage)
                {
                    IsNotOfficialChannelPackageWarning();
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

#if WINDOWS
                AddJumpTask();
#endif

                desktop.MainWindow = MainWindow;
                desktop.Exit += Desktop_Exit;
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

#if DEBUG
                AreYouOk();
                static async void AreYouOk()
                {
                    await MessageBoxCompat.ShowAsync(
                        "Are You Ok?",
                        "title",
                        MessageBoxButtonCompat.OK,
                        MessageBoxImageCompat.Information);
                }
#endif
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
#if WINDOWS
            WpfApplication.Current.Shutdown();
#endif
        }

#if WINDOWS

        // JumpList
        // 表示作为菜单显示在 Windows 7 任务栏按钮上的项和任务的列表。
        // https://docs.microsoft.com/zh-cn/dotnet/api/system.windows.shell.jumplist?view=net-5.0

        // Taskbar Extensions
        // https://docs.microsoft.com/zh-cn/windows/win32/shell/taskbar-extensions?redirectedfrom=MSDN

        static void AddJumpTask()
        {
            // Configure a new JumpTask.
            var jumpTask1 = new JumpTask
            {
                // Get the path to Calculator and set the JumpTask properties.
                ApplicationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "calc.exe"),
                IconResourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "calc.exe"),
                Title = "Calculator",
                Description = "Open Calculator.",
                CustomCategory = "User Added Tasks"
            };
            // Get the JumpList from the application and update it.
            JumpList jumpList1 = JumpList.GetJumpList(WpfApplication.Current);
            jumpList1.JumpItems.Add(jumpTask1);
            JumpList.AddToRecentCategory(jumpTask1);
            jumpList1.Apply();
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
#pragma warning restore CA1416 // 验证平台兼容性