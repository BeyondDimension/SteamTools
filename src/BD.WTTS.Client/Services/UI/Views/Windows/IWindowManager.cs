// ReSharper disable once CheckNamespace

namespace BD.WTTS.Services;

/// <summary>
/// 窗口管理
/// </summary>
public interface IWindowManager
{
    static IWindowManager Instance => Ioc.Get<IWindowManager>();

    /// <summary>
    /// 显示一个页内弹窗
    /// </summary>
    /// <typeparam name="TPageViewModel"></typeparam>
    /// <param name="viewModel"></param>
    /// <param name="title"></param>
    /// <param name="subHeader"></param>
    /// <param name="isDialog"></param>
    /// <param name="isFooterExpanded"></param>
    /// <returns></returns>
    Task<bool> ShowTaskDialogAsync<TPageViewModel>(
        TPageViewModel viewModel,
        string title = "",
        string? header = null,
        string? subHeader = null,
        bool isDialog = false,
        bool showProgressBar = false,
        bool isRememberChooseFooter = false,
        bool isOkButton = true,
        bool isCancelButton = false,
        bool isRetryButton = false,
        object? pageContent = null,
        string? okButtonText = null,
        string? retryButtonText = null,
        string? moreInfoText = null,
        Func<bool>? cancelCloseAction = null,
        bool disableScroll = false,
        bool wide = false)
        where TPageViewModel : ViewModelBase;

    /// <summary>
    /// 显示一个窗口
    /// </summary>
    /// <typeparam name="TWindowViewModel"></typeparam>
    /// <param name="appEndPoint"></param>
    /// <param name="viewModel"></param>
    /// <param name="title"></param>
    /// <param name="resizeMode"></param>
    /// <param name="isDialog"></param>
    /// <param name="isParent"></param>
    /// <returns></returns>
    Task ShowAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TWindowViewModel>(
        AppEndPoint appEndPoint,
        TWindowViewModel? viewModel = null,
        string title = "",
        ResizeMode resizeMode = ResizeMode.NoResize,
        bool isDialog = false,
        bool isParent = true)
        where TWindowViewModel : WindowViewModel, new();

    /// <summary>
    /// 显示一个窗口
    /// </summary>
    /// <param name="typeWindowViewModel"></param>
    /// <param name="appEndPoint"></param>
    /// <param name="viewModel"></param>
    /// <param name="title"></param>
    /// <param name="resizeMode"></param>
    /// <param name="isDialog"></param>
    /// <param name="isParent"></param>
    /// <returns></returns>
    Task ShowAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type typeWindowViewModel,
        AppEndPoint appEndPoint,
        WindowViewModel? viewModel = null,
        string title = "",
        ResizeMode resizeMode = ResizeMode.NoResize,
        bool isDialog = false,
        bool isParent = true);

    /// <summary>
    /// 显示一个窗口
    /// </summary>
    /// <param name="appEndPoint"></param>
    /// <param name="viewModel"></param>
    /// <param name="title"></param>
    /// <param name="resizeMode"></param>
    /// <param name="isDialog"></param>
    /// <param name="isParent"></param>
    /// <returns></returns>
    Task ShowAsync(AppEndPoint appEndPoint,
        WindowViewModel? viewModel = null,
        string title = "",
        ResizeMode resizeMode = ResizeMode.NoResize,
        bool isDialog = false,
        bool isParent = true);

    /// <summary>
    /// 显示一个弹窗，返回 <see langword="true"/> 确定(仅当ViewModel继承自<see cref="DialogWindowViewModel"/>时生效)，<see langword="false"/> 取消
    /// </summary>
    /// <typeparam name="TWindowViewModel"></typeparam>
    /// <param name="appEndPoint"></param>
    /// <param name="viewModel"></param>
    /// <param name="title"></param>
    /// <param name="resizeMode"></param>
    /// <param name="isDialog"></param>
    /// <param name="isParent"></param>
    /// <returns></returns>
    Task<bool> ShowDialogAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TWindowViewModel>(
        AppEndPoint appEndPoint,
        TWindowViewModel? viewModel = null,
        string title = "",
        ResizeMode resizeMode = ResizeMode.NoResize,
        bool isDialog = true,
        bool isParent = true)
        where TWindowViewModel : WindowViewModel, new();

    /// <summary>
    /// 显示一个弹窗，返回 <see langword="true"/> 确定(仅当ViewModel继承自<see cref="DialogWindowViewModel"/>时生效)，<see langword="false"/> 取消
    /// </summary>
    /// <param name="typeWindowViewModel"></param>
    /// <param name="appEndPoint"></param>
    /// <param name="viewModel"></param>
    /// <param name="title"></param>
    /// <param name="resizeMode"></param>
    /// <param name="isDialog"></param>
    /// <returns></returns>
    Task ShowDialogAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type typeWindowViewModel,
        AppEndPoint appEndPoint,
        WindowViewModel? viewModel = null,
        string title = "",
        ResizeMode resizeMode = ResizeMode.NoResize,
        bool isDialog = true);

    /// <summary>
    /// 显示一个弹窗，返回 <see langword="true"/> 确定(仅当ViewModel继承自<see cref="DialogWindowViewModel"/>时生效)，<see langword="false"/> 取消
    /// </summary>
    /// <param name="appEndPoint"></param>
    /// <param name="viewModel"></param>
    /// <param name="title"></param>
    /// <param name="resizeMode"></param>
    /// <param name="isDialog"></param>
    /// <returns></returns>
    Task ShowDialogAsync(AppEndPoint appEndPoint,
        WindowViewModel? viewModel = null,
        string title = "",
        ResizeMode resizeMode = ResizeMode.NoResize,
        bool isDialog = true);

    /// <summary>
    /// 根据视图模型关闭窗口
    /// </summary>
    /// <param name="vm"></param>
    void CloseWindow(WindowViewModel vm) { }

    /// <summary>
    /// 获取视图模型对应的窗口是否显示
    /// </summary>
    /// <param name="vm"></param>
    /// <returns></returns>
    bool IsVisibleWindow(WindowViewModel vm) => false;

    /// <summary>
    /// 根据视图模型隐藏窗口
    /// </summary>
    /// <param name="vm"></param>
    void HideWindow(WindowViewModel vm) { }

    /// <summary>
    /// 根据视图模型显示窗口
    /// </summary>
    /// <param name="vm"></param>
    async void ShowWindow(WindowViewModel vm)
    {
        var windowName = vm.GetType().Name.TrimEnd(nameof(WindowViewModel));
        if (Enum.TryParse<AppEndPoint>(windowName, out var appEndPoint))
        {
            await ShowAsync(appEndPoint, vm);
        }
    }
}
