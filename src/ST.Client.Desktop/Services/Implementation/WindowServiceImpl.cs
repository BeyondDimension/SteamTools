using ReactiveUI;
using System;
using System.Application.Mvvm;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.Services.Implementation
{
    public class WindowServiceImpl : ReactiveObject, IWindowService, IDisposableHolder
    {
        private MainWindowViewModel mainWindow;
        private AchievementWindowViewModel achievementWindow;
        private readonly CompositeDisposable compositeDisposable = new();

        /// <summary>
        /// 获取为当前主窗口提供的数据。
        /// </summary>
        public WindowViewModel MainWindow { get; private set; }

        public void Init()
        {
            if (appidUnlockAchievement.HasValue)
            {
                achievementWindow = new AchievementWindowViewModel(appidUnlockAchievement.Value);
                MainWindow = achievementWindow;
            }
            else
            {
                mainWindow = new MainWindowViewModel();
                MainWindow = mainWindow;
            }
        }

        int? appidUnlockAchievement;
        public void InitUnlockAchievement(int appid)
        {
            appidUnlockAchievement = appid;
        }

        //public Window GetMainWindow()
        //{
        //    if (this.MainWindow == this.mainWindow)
        //    {
        //        this.MainWindow
        //                   .Subscribe(nameof(MainWindowViewModel.SelectedItem),
        //                   () => this.MainWindow.StatusBar = (this.MainWindow as MainWindowViewModel).SelectedItem)
        //                   .AddTo(this);
        //        return new MainWindow { DataContext = this.MainWindow, };
        //    }
        //    if (this.MainWindow == this.achievementWindow)
        //    {
        //        return new AchievementWindow { DataContext = this.MainWindow, };
        //    }
        //    throw new InvalidOperationException();
        //}

        #region disposable members

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => this.compositeDisposable;

        public void Dispose()
        {
            this.compositeDisposable.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
