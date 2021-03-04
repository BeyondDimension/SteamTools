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
using Avalonia.Themes.Fluent;
using Avalonia.Markup.Xaml.Styling;

#if WINDOWS
using System.Windows.Shell;
using WpfApplication = System.Windows.Application;
#endif

namespace System.Application.UI
{
    public partial class App : AvaloniaApplication, IDisposableHolder
    {
        public static App Instance => Current is App app ? app : throw new Exception("Impossible");
        [Obsolete("use IOPath.AppDataDirectory", true)]
        public DirectoryInfo LocalAppData => new DirectoryInfo(IOPath.AppDataDirectory);

        public static DirectoryInfo RootDirectory => new DirectoryInfo(AppContext.BaseDirectory);

        [Obsolete("use AppHelper.ProgramName", true)]
        public string ProgramName => AppHelper.ProgramName;

        public FluentTheme? fluentTheme;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            Name = ThisAssembly.AssemblyTrademark;

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
#if MAC
                AppDelegate.Init();
#endif

                #region NotifyIcon

                var notifyIcon = INotifyIcon.Instance;
                notifyIcon.ToolTipText = ThisAssembly.AssemblyTrademark;
                switch (DI.Platform)
                {
                    case Platform.Windows:
                    case Platform.Linux:
                        notifyIcon.IconPath = "avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/Icon.ico";
                        break;
                    case Platform.Apple:
                        notifyIcon.IconPath = "avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/Icon_16.png";
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
                        Header = "Light",
                        Command = ReactiveCommand.Create(()=>{
                          Styles[0] = new FluentTheme(new Uri("avares://Avalonia.Themes.Fluent/FluentLight.xaml"))
                            { Mode= FluentThemeMode.Light };
                            var uri = new Uri("avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Styles/ThemeLight.xaml");
                          Styles[1] = new StyleInclude(uri){ Source = uri };
                        })
                    },
                    new MenuItem()
                    {
                        Header = "Dark",
                        Command = ReactiveCommand.Create(()=>{
                          Styles[0] = new FluentTheme(new Uri("avares://Avalonia.Themes.Fluent/FluentDark.xaml")){ Mode= FluentThemeMode.Dark };
                            var uri = new Uri("avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Styles/ThemeDark.xaml");
                          Styles[1] = new StyleInclude(uri){ Source = uri };
                        })
                    },
                    new MenuItem()
                    {
                        Header = "NotifyIconContextMenuItemHeaderRestore",
                        Command = ReactiveCommand.Create(RestoreMainWindow)
                    },
                    new MenuItem()
                    {
                        Header = "NotifyIconContextMenuItemHeaderExit",
                        Command = ReactiveCommand.Create(Shutdown)
                    }
                };
                NotifyIconContextMenu.Items = menuItems;
                notifyIcon.ContextMenu = NotifyIconContextMenu;
                notifyIcon.Visible = true;
                notifyIcon.Click += NotifyIcon_Click;
                notifyIcon.DoubleClick += NotifyIcon_Click;
                compositeDisposable.Add(() =>
                {
                    notifyIcon.IconPath = string.Empty;
                });

                #endregion

#if WINDOWS
                AddJumpTask();
#endif

                if (!AppHelper.IsOfficialChannelPackage)
                {
                    IsNotOfficialChannelPackageWarning();
                }

                desktop.MainWindow = MainWindow;
                desktop.Exit += ApplicationLifetime_Exit;
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }

            base.OnFrameworkInitializationCompleted();
        }

        void ApplicationLifetime_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            compositeDisposable.Dispose();
#if WINDOWS
            WpfApplication.Current.Shutdown();
#endif
        }

        void NotifyIcon_Click(object? sender, EventArgs e)
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

        #region IDisposable members

        public readonly CompositeDisposable compositeDisposable = new CompositeDisposable();

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => compositeDisposable;

        void IDisposable.Dispose()
        {
            compositeDisposable.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

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

#endif
    }
}
#pragma warning restore CA1416 // 验证平台兼容性