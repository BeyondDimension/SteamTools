// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 视图模型管理服务
/// </summary>
public interface IViewModelManager : IDisposable
{
    static IViewModelManager Instance => Ioc.Get<IViewModelManager>();

    /// <summary>
    /// 获取视图模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Get<T>() where T : ViewModelBase;

    /// <summary>
    /// 获取视图模型
    /// </summary>
    /// <param name="vmType"></param>
    /// <returns></returns>
    ViewModelBase Get(Type vmType);

    /// <summary>
    /// 释放一个视图模型
    /// </summary>
    /// <param name="viewModel"></param>
    void Dispose(ViewModelBase viewModel);

    /// <summary>
    /// 获取为当前主窗口提供的数据
    /// </summary>
    WindowViewModel? MainWindow { get; }

    /// <summary>
    /// 获取为当前主窗口提供的数据
    /// </summary>
    MainWindowViewModel? MainWindow2 => MainWindow as MainWindowViewModel;

    //TaskBarWindowViewModel? TaskBarWindow { get; }

    void InitViewModels(IEnumerable<TabItemViewModel> tabItems, ImmutableArray<TabItemViewModel> footerTabItems);

    //void InitUnlockAchievement(int appid);

    //void InitCloudManageMain(int appid);

    //void InitTaskBarWindowViewModel();

    //void DispoeTaskBarWindowViewModel();

    ///// <summary>
    ///// 打开托盘菜单窗口
    ///// </summary>
    //void ShowTaskBarWindow(int x = 0, int y = 0);

    //T GetMainPageViewModel<T>() where T : TabItemViewModel
    //{
    //    if (MainWindow is MainWindowViewModel mainWindowViewModel)
    //    {
    //        return mainWindowViewModel.GetTabItemVM<T>();
    //    }
    //    return null!;
    //}
}