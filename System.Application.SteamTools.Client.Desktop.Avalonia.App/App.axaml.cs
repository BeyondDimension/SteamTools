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
using System.Application.Services;
using System.Windows.Input;
using System.Linq;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Application.Models;
#if WINDOWS
using System.Windows.Shell;
using WpfApplication = System.Windows.Application;
#endif
using APIConst = System.Application.Services.CloudService.Constants;

namespace System.Application.UI
{
    public partial class App : AvaloniaApplication, IDisposableHolder, IDesktopAppService, IDesktopAvaloniaAppService
    {
        public static App Instance => Current is App app ? app : throw new Exception("Impossible");

        [Obsolete("use IOPath.AppDataDirectory", true)]
        public DirectoryInfo LocalAppData => new(IOPath.AppDataDirectory);

        public static DirectoryInfo RootDirectory => new(AppContext.BaseDirectory);

        [Obsolete("use AppHelper.ProgramName", true)]
        public string ProgramName => AppHelper.ProgramName;

        AppTheme mTheme = AppTheme.Dark;
        public AppTheme Theme
        {
            get
            {
                return mTheme;
            }
            set
            {
                if (value == mTheme) return;
                string? the;
                FluentThemeMode mode;

                if (value == AppTheme.FollowingSystem)
                {
                    var dps = DI.Get<IDesktopPlatformService>();
                    var isLightOrDarkTheme = dps.IsLightOrDarkTheme;
                    if (isLightOrDarkTheme.HasValue)
                    {
                        value = isLightOrDarkTheme.Value ? AppTheme.Light : AppTheme.Dark;
                    }
                }

                switch (value)
                {
                    case AppTheme.Light:
                        the = "Light";
                        mode = FluentThemeMode.Light;
                        break;
                    case AppTheme.Dark:
                        the = "Dark";
                        mode = FluentThemeMode.Dark;
                        break;
                    default:
                        the = "Dark";
                        mode = FluentThemeMode.Dark;
                        break;
                }
                var uri_0 = new Uri($"avares://Avalonia.Themes.Fluent/Fluent{the}.xaml");
                var uri_1 = new Uri($"avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Styles/Theme{the}.xaml");

                Styles[0] = new FluentTheme(uri_0)
                {
                    Mode = mode
                };
                Styles[1] = new StyleInclude(uri_1)
                {
                    Source = uri_1,
                };

                mTheme = value;
            }
        }

        readonly Dictionary<string, ICommand> mNotifyIconMenus = new();

        public IReadOnlyDictionary<string, ICommand> NotifyIconMenus => mNotifyIconMenus;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            Name = ThisAssembly.AssemblyTrademark;

            ViewModelBase.IsInDesignMode = ApplicationLifetime == null;
            Startup.Init();

            #region 启动时加载的资源
            SettingsHost.Load();
            compositeDisposable.Add(SettingsHost.Save);
            Theme = (AppTheme)UISettings.Theme.Value;
            UISettings.Theme.Subscribe(x => Theme = (AppTheme)x);
            UISettings.Language.Subscribe(x => R.ChangeLanguage(x));

            #endregion

            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        public ContextMenu? NotifyIconContextMenu { get; private set; }

        static void IsNotOfficialChannelPackageWarning()
        {
            var text = APIConst.IsNotOfficialChannelPackageWarning;
            var title = AppResources.Warning;
            MessageBoxCompat.Show(text, title, MessageBoxButtonCompat.OK, MessageBoxImageCompat.Warning);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // 在UI预览中，ApplicationLifetime 为 null
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

                mNotifyIconMenus.Add("Light", ReactiveCommand.Create(() =>
                {
                    Theme = AppTheme.Light;
                }));

                mNotifyIconMenus.Add("Dark", ReactiveCommand.Create(() =>
                {
                    Theme = AppTheme.Dark;
                }));

                mNotifyIconMenus.Add("Show",
                    ReactiveCommand.Create(RestoreMainWindow));

                mNotifyIconMenus.Add("Exit",
                    ReactiveCommand.Create(Shutdown));

                NotifyIconContextMenu.Items = mNotifyIconMenus
                    .Select(x => new MenuItem { Header = x.Key, Command = x.Value }).ToList();
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
                desktop.Startup += Desktop_Startup;
                desktop.Exit += ApplicationLifetime_Exit;
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void Desktop_Startup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            AppHelper.Initialized?.Invoke();
        }

        void ApplicationLifetime_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            compositeDisposable.Dispose();
#if WINDOWS
            WpfApplication.Current.Shutdown();
#endif
            AppHelper.Shutdown?.Invoke();
        }

        void NotifyIcon_Click(object? sender, EventArgs e)
        {
            RestoreMainWindow();
        }

        public static async void SetClipboardText(string s) => await Current.Clipboard.SetTextAsync(s);

        Window? mMainWindow;

        public Window MainWindow
        {
            get => mMainWindow ?? throw new ArgumentNullException(nameof(mMainWindow));
            set => mMainWindow = value;
        }

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

        void IDesktopAppService.Shutdown() => Shutdown();

        bool IDesktopAppService.IsCefInitComplete => CefNetApp.InitState == CefNetAppInitState.Complete;

        #region IDisposable members

        public readonly CompositeDisposable compositeDisposable = new();

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