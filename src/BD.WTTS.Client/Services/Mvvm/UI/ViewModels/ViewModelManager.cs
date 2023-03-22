// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

internal sealed class ViewModelManager : ReactiveObject, IViewModelManager
{
    //MainWindowViewModel? mainWindow;
    TaskBarWindowViewModel? taskbarWindow;
    readonly CompositeDisposable compositeDisposable = new();

    WindowViewModel? mMainWindow;

    public WindowViewModel? MainWindow
    {
        get => mMainWindow;
    }

    public TaskBarWindowViewModel? TaskBarWindow => taskbarWindow;

    public void InitViewModels()
    {
        // TODO: CloudArchiveWindowViewModel/AchievementWindowViewModel
        //try
        //{
        //    if (isCloudManageMain)
        //    {
        //        mMainWindow = new CloudArchiveWindowViewModel(steamaAppid);
        //    }
        //    else if (isUnlockAchievementMain)
        //    {
        //        mMainWindow = new AchievementWindowViewModel(steamaAppid);
        //    }
        //    else
        //    {
        //        //mainWindow = new MainWindowViewModel();
        //        //mMainWindow = mainWindow;
        //        mMainWindow = new MainWindowViewModel();
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Log.Error(nameof(ViewModelManager), ex, "Init WindowViewModel");
        //    throw;
        //}
        //finally
        //{
        //    InitTaskBarWindowViewModel();
        //}
    }

    int steamaAppid;
    bool isUnlockAchievementMain;
    bool isCloudManageMain;

    public void InitUnlockAchievement(int appid)
    {
        steamaAppid = appid;
        isUnlockAchievementMain = true;
    }

    public void InitCloudManageMain(int appid)
    {
        steamaAppid = appid;
        isCloudManageMain = true;
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

    public void InitTaskBarWindowViewModel()
    {
        try
        {
            if (OperatingSystem.IsWindows() && StartupOptions.Value.HasNotifyIcon && taskbarWindow == null && mMainWindow != null)
            {
                taskbarWindow = new TaskBarWindowViewModel(mMainWindow);
            }
        }
        catch (Exception ex)
        {
            Log.Error(nameof(ViewModelManager), ex, "Init TaskBarWindowViewModel");
            throw;
        }
    }

    public void DispoeTaskBarWindowViewModel()
    {
        if (taskbarWindow != null)
        {
            taskbarWindow.Dispose();
            taskbarWindow = null;
        }
    }

    public void ShowTaskBarWindow(int x = 0, int y = 0)
    {
        try
        {
            //if (!taskbarWindow.IsVisible)
            //{
            taskbarWindow?.Show(x, y);
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

    #region DisposableHolder/Disposable

    ICollection<IDisposable> IDisposableHolder.CompositeDisposable => compositeDisposable;

    bool disposedValue;

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
                compositeDisposable.Dispose();
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            disposedValue = true;
            DispoeTaskBarWindowViewModel();
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}