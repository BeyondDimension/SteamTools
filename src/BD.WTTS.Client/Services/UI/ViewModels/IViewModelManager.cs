// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public interface IViewModelManager : IDisposableHolder, IDisposable
{
    static IViewModelManager Instance => Ioc.Get<IViewModelManager>();

    /// <summary>
    /// 获取为当前主窗口提供的数据
    /// </summary>
    WindowViewModel? MainWindow { get; }

    TaskBarWindowViewModel? TaskBarWindow { get; }

    void InitViewModels();

    void InitUnlockAchievement(int appid);

    void InitCloudManageMain(int appid);

    void InitTaskBarWindowViewModel();

    void DispoeTaskBarWindowViewModel();

    /// <summary>
    /// 打开托盘菜单窗口
    /// </summary>
    void ShowTaskBarWindow(int x = 0, int y = 0);

    T GetMainPageViewModel<T>() where T : TabItemViewModel
    {
        if (MainWindow is MainWindowViewModel mainWindowViewModel)
        {
            return mainWindowViewModel.GetTabItemVM<T>();
        }
        return null!;
    }
}