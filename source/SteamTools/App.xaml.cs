using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Reflection;
using System.Security.Principal;
using MetroRadiance.UI;
using MetroTrilithon.Lifetime;
using System.Runtime.CompilerServices;
using Livet;
using SteamTool.Proxy;
using System.Threading;
using System.IO;
using MetroTrilithon.Desktop;
using SteamTool.Core.Common;
using SteamTools.Services;
using MetroTrilithon.Mvvm;
using Hardcodet.Wpf.TaskbarNotification;
using SteamTools.Models;
using SteamTools.Models.Settings;
using SteamTools.ViewModels;
using SteamTool.Core;
using Microsoft.Win32;

namespace SteamTools
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : INotifyPropertyChanged, IDisposableHolder
    {
        public static App Instance => Current as App;

#if NETCOREAPP
        public string ProgramName => Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
#else
        public string ProgramName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
#endif

        public DirectoryInfo LocalAppData = new DirectoryInfo(
        Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        ProductInfo.Company,
        ProductInfo.Title));

        private void IsRenameProgram()
        {
            if ($"{ProductInfo.Title}.exe" != ProgramName)
            {
                //MessageBox.Show(SteamTools.Properties.Resources.ReNameErrorInfo, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Shutdown();
            }
        }

        /// <summary>
        /// 设置WebBrowser IE版本
        /// </summary>
        private void SetWebBrowserIeVersion()
        {
            try
            {
                SteamToolCore.Instance.Get<RegistryKeyService>().AddOrUpdateRegistryKey(Registry.CurrentUser, @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", ProgramName, "10001", RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                Logger.Error("Program -> Main -> Registry and environment modifications resulted in an exception.", ex);
            }
        }

        /// <summary>
        /// 启动时
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
#if !DEBUG
            var appInstance = new MetroTrilithon.Desktop.ApplicationInstance().AddTo(this);
            if (appInstance.IsFirst)
#endif
            {
#if DEBUG
                if (e.Args.ContainsArg("-app"))
                {
                    this.ProcessCommandLineParameter(e.Args);
                    base.OnStartup(e);
                    return;
                }
                Logger.EnableTextLog = true;
#endif
                App.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                this.DispatcherUnhandledException += App_DispatcherUnhandledException;
                DispatcherHelper.UIDispatcher = this.Dispatcher;
                SettingsHost.Load();
                this.compositeDisposable.Add(SettingsHost.Save);
                this.compositeDisposable.Add(ProxyService.Current.Shutdown);
                this.compositeDisposable.Add(SteamConnectService.Current.Shutdown);
                this.compositeDisposable.Add(() =>
                {
                    if (TaskbarService.Current.Taskbar != null)
                    {
                        //TaskbarService.Current.Taskbar.Icon = null; //避免托盘图标没有自动消失
                        TaskbarService.Current.Taskbar.Icon.Dispose();
                    }
                });

                Microsoft.Win32.SystemEvents.SessionEnding += SystemEvents_SessionEnding;

                if (e.Args.ContainsArg("-log") || GeneralSettings.IsEnableLogRecord)
                {
                    Logger.EnableTextLog = true;
                }
                //每次启动都覆盖设置一次开机自启确保路径变换后开机自启依然生效
                if (GeneralSettings.WindowsStartupAutoRun)
                {
                    var steamService = SteamToolCore.Instance.Get<SteamToolService>();
                    steamService.SetWindowsStartupAutoRun(GeneralSettings.WindowsStartupAutoRun.Value, ProductInfo.Title);
                }


                GeneralSettings.Culture.Subscribe(x => ResourceService.Current.ChangeCulture(x)).AddTo(this);
                WindowService.Current.AddTo(this).Initialize();
                ProxyService.Current.Initialize();
                SteamConnectService.Current.Initialize();
                AuthService.Current.Initialize();
                if (GeneralSettings.IsAutoCheckUpdate.Value)
                {
                    AutoUpdateService.Current.CheckUpdate();
                }

                //托盘加载
                TaskbarService.Current.Taskbar = (TaskbarIcon)FindResource("Taskbar");
                ThemeService.Current.Register(this, Theme.Windows, Accent.Windows);

                SetWebBrowserIeVersion();

                this.MainWindow = WindowService.Current.GetMainWindow();
                if (e.Args.ContainsArg("-minimized") || GeneralSettings.IsStartupAppMinimized.Value)
                {
                    //this.MainWindow.Show();
                    //(WindowService.Current.MainWindow as MainWindowViewModel).IsVisible = false;
                    (WindowService.Current.MainWindow as MainWindowViewModel).Initialize();
                }
                else
                    this.MainWindow.Show();

                if (GeneralSettings.IsAutoRunSteam.Value && Process.GetProcessesByName("steam").Length < 1)
                {
                    var steamTool = SteamToolCore.Instance.Get<SteamToolService>();
                    if (!string.IsNullOrEmpty(steamTool.SteamPath))
                    {
                        steamTool.StartSteam("-silent " + GeneralSettings.SteamStratParameter.Value);
                    }
                }

#if !DEBUG
                appInstance.CommandLineArgsReceived += (sender, args) =>
                {
                    // 检测到多次启动时将主窗口置于最前面
                    this.Dispatcher.Invoke(() =>
                    {
                        (WindowService.Current.MainWindow as MainWindowViewModel).IsVisible = true;
                    });
                    //this.ProcessCommandLineParameter(args.CommandLineArgs);
                };
#endif

                base.OnStartup(e);
            }
#if !DEBUG
            else
            {
                if (e.Args.Length > 0)
                {
                    this.ProcessCommandLineParameter(e.Args);
                }
                else
                {
                    appInstance.SendCommandLineArgs(e.Args);
                    App.Current.Shutdown();
                }
            }
#endif
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            this.compositeDisposable.Dispose();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Logger.Error($"{Assembly.GetExecutingAssembly().GetName().Name} Run Error : {Environment.NewLine}", e.Exception);
                if (e.Exception.InnerException != null)
                    Logger.Error($"InnerException Error : {Environment.NewLine}", e.Exception.InnerException);
                MessageBox.Show(e.Exception.ToString(), $"{ProductInfo.Title} {ProductInfo.VersionString} Run Error");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString(), $"{ProductInfo.Title} {ProductInfo.VersionString} Error");
            }

            //Current.Shutdown();
        }

        /// <summary>
        /// 程序退出
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            //if (TaskbarService.Current.Taskbar != null)
            //{
            //    //TaskbarService.Current.Taskbar.Icon = null; //避免托盘图标没有自动消失
            //    TaskbarService.Current.Taskbar.Icon.Dispose();
            //}
            this.compositeDisposable.Dispose();
            base.OnExit(e);
        }

        private void ProcessCommandLineParameter(string[] args)
        {
#if DEBUG
            Debug.WriteLine("多重启动通知: " + args.ToString(" "));
#endif
            // 当使用命令行参数多次启动时，可以执行某些操作
            if (args.Length == 0)
            {
                this.Shutdown();
            }
            if (args.ContainsArg("-log"))
            {
                Logger.EnableTextLog = true;
                this.DispatcherUnhandledException += App_DispatcherUnhandledException;
                DispatcherHelper.UIDispatcher = this.Dispatcher;
            }
            if (args.ContainsArg("-app", out int appid))
            {
                App.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                ThemeService.Current.Register(this, Theme.Windows, Accent.Windows);
                new SettingsPageViewModel();
                WindowService.Current.AddTo(this).Initialize(appid);
                //SteamConnectService.Current.Initialize();

                this.MainWindow = WindowService.Current.GetMainWindow();
                if (args.ContainsArg("-hide"))
                {
                    //this.MainWindow.Show();
                    this.MainWindow.Hide();
                }
                else
                {
                    this.MainWindow.Show();
                }
            }
        }


        #region INotifyPropertyChanged members

        private event PropertyChangedEventHandler PropertyChangedInternal;
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { this.PropertyChangedInternal += value; }
            remove { this.PropertyChangedInternal -= value; }
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChangedInternal?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IDisposable members
        public readonly LivetCompositeDisposable compositeDisposable = new LivetCompositeDisposable();
        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => this.compositeDisposable;

        void IDisposable.Dispose()
        {
            this.compositeDisposable.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
