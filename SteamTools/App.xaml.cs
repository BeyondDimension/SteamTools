using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Reflection;
using System.Security.Principal;
using MetroRadiance.UI;
using Application = System.Windows.Application;
using MetroTrilithon.Lifetime;
using System.Runtime.CompilerServices;
using Livet;
using Hardcodet.Wpf.TaskbarNotification;
using SteamTool.Proxy;
using System.Threading;

namespace SteamTools
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged, IDisposableHolder
    {
        public static Application Instance => Current as Application;

        #region 托盘图标
        private TaskbarIcon _TaskBar;

        public TaskbarIcon Taskbar
        {
            get { return this._TaskBar; }
            set
            {
                if (this._TaskBar != value)
                {
                    this._TaskBar = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        // 检查是否是管理员身份 
        //  VS 不是管理员模式时 会导致调试时程序重启
        private void CheckAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            bool runAsAdmin = wp.IsInRole(WindowsBuiltInRole.Administrator);

            if (!runAsAdmin)
            {
                // It is not possible to launch a ClickOnce app as administrator directly,
                // so instead we launch the app as administrator in a new process.
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase)
                {
                    // The following properties run the new process as administrator
                    UseShellExecute = true,
                    Verb = "runas"
                };

                // Start the new process
                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception)
                {
                }

                Application.Current.Shutdown();
            }
        }

        private void CheckProgramRuning()
        {
            Mutex mutex = new Mutex(true, System.Diagnostics.Process.GetCurrentProcess().ProcessName, out var isAppRunning);
            if (!isAppRunning)
            {
                MessageBox.Show("程序已运行，不能再次打开！");
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// 启动时
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            #region Release Code
#if !DEBUG
            CheckAdministrator();
            ThemeService.Current.Register(this, Theme.Windows, Accent.Windows);
#else
            ThemeService.Current.Register(this, Theme.Dark, Accent.Blue);
#endif
            #endregion
            CheckProgramRuning();

            #region Initialize SteamService

            #endregion

            #region 托盘加载

            this.Taskbar = (TaskbarIcon)FindResource("Taskbar");

            #endregion

            base.OnStartup(e);
        }

        /// <summary>
        /// 程序退出
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {

            this.Taskbar.Icon = null; //避免托盘图标没有自动消失
            this.Taskbar.Dispose();
            HttpProxy.Current.Dispose();
            base.OnExit(e);
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
        private readonly LivetCompositeDisposable compositeDisposable = new LivetCompositeDisposable();
        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => this.compositeDisposable;

        void IDisposable.Dispose()
        {
            this.compositeDisposable.Dispose();
        }

        #endregion
    }
}
