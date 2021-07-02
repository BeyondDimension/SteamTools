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
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        private MainWindowViewModel mainWindow;
        private AchievementWindowViewModel achievementWindow;
        private TaskBarWindowViewModel taskbarWindow;
        private readonly CompositeDisposable compositeDisposable = new();

        /// <summary>
        /// 获取为当前主窗口提供的数据。
        /// </summary>
        public WindowViewModel MainWindow { get; private set; }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        /// <summary>
        /// 获取为当前主窗口提供的数据。
        /// </summary>
        public TaskBarWindowViewModel? TaskBarWindow => taskbarWindow;

        public void Init()
        {
            try
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
                taskbarWindow = new TaskBarWindowViewModel();
            }
            catch (Exception ex)
            {
                Log.Error("WindowService", ex, "Init WindowViewModel");
                throw;
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

        /// <summary>
        /// 打开托盘菜单窗口
        /// </summary>
        public void ShowTaskBarWindow(int x = 0, int y = 0)
        {
            try
            {
                if (!taskbarWindow.IsVisible)
                {
                    taskbarWindow.Show(x, y);
                }
                else
                {
                    taskbarWindow.SetPosition(x, y);
                }
            }
            catch (Exception ex)
            {
                // https://appcenter.ms/orgs/BeyondDimension/apps/Steam/crashes/errors/1377813613u/overview
                Log.Error("WindowService", ex,
                    "ShowTaskBarWindow, taskbarWindow: {0}", taskbarWindow?.ToString() ?? "null");
                throw;
            }
        }

        #region disposable members

        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => compositeDisposable;

        public void Dispose()
        {
            compositeDisposable.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
