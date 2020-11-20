using Livet;
using MetroTrilithon.Lifetime;
using SteamTools.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SteamTools.Services
{
    public class WindowService : NotificationObject, IDisposableHolder
    {
        public static WindowService Current { get; } = new WindowService();
        private MainWindowViewModel mainWindow;
        private AchievementWindowViewModel achievementWindow;
        private readonly LivetCompositeDisposable compositeDisposable = new LivetCompositeDisposable();

        /// <summary>
        /// 获取为当前主窗口提供的数据。
        /// </summary>
        public MainWindowViewModelBase MainWindow { get; private set; }


        public void Initialize(int appid = 0)
        {
            if (appid < 1)
            {
                this.mainWindow = new MainWindowViewModel(true);
                this.MainWindow = mainWindow;
            }
            else
            {
                this.achievementWindow = new AchievementWindowViewModel(true);
                achievementWindow.Initialize(appid);
                this.MainWindow = achievementWindow;
            }
        }

        public Window GetMainWindow()
        {
            if (this.MainWindow == this.mainWindow)
            {
                return new MainWindow { DataContext = this.MainWindow, };
            }
            if (this.MainWindow == this.achievementWindow)
            {
                return new AchievementWindow { DataContext = this.MainWindow, };
            }
            throw new InvalidOperationException();
        }

        #region disposable members

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => this.compositeDisposable;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.compositeDisposable.Dispose();
        }

        #endregion
    }
}
