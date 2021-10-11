using ReactiveUI;
using System;
using System.Application.Mvvm;
using System.Reactive.Disposables;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    public class DesktopWindowViewModelManager : ReactiveObject, IDesktopWindowViewModelManager
    {
        MainWindowViewModel? mainWindow;
        AchievementWindowViewModel? achievementWindow;
        TaskBarWindowViewModel? taskbarWindow;
        readonly CompositeDisposable compositeDisposable = new();

        WindowViewModel? mMainWindow;
        public WindowViewModel MainWindow
        {
            get => mMainWindow ?? throw new NullReferenceException("MainWindowViewModel is null.");
            private set => mMainWindow = value;
        }

        public TaskBarWindowViewModel? TaskBarWindow => taskbarWindow;

        public void InitViewModels()
        {
            try
            {
                if (appidUnlockAchievementHasValue)
                {
                    achievementWindow = new AchievementWindowViewModel(appidUnlockAchievement);
                    MainWindow = achievementWindow;
                }
                else
                {
                    mainWindow = new MainWindowViewModel();
                    MainWindow = mainWindow;
                }
            }
            catch (Exception ex)
            {
                Log.Error(nameof(DesktopWindowViewModelManager), ex, "Init WindowViewModel");
                throw;
            }
            finally
            {
                taskbarWindow = new TaskBarWindowViewModel();
            }
        }

        int appidUnlockAchievement;
        bool appidUnlockAchievementHasValue;
        public void InitUnlockAchievement(int appid)
        {
            appidUnlockAchievement = appid;
            appidUnlockAchievementHasValue = true;
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

        public void ShowTaskBarWindow(int x = 0, int y = 0)
        {
            try
            {
                //if (!taskbarWindow.IsVisible)
                //{
                taskbarWindow.Show(x, y);
                //}
                //else
                //{
                //    taskbarWindow.SetPosition(x, y);
                //}
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
