using dotnetCampus.Ipc.Pipes;

namespace BD.WTTS.Plugins.Abstractions;

/// <summary>
/// 插件接口
/// </summary>
public partial interface IPlugin
{
    /// <summary>
    /// 判断插件是否满足要求，如没有任何要求则返回 <see angword="true"/>
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    bool HasValue([NotNullWhen(false)] out string? error);

    /// <summary>
    /// 获取当前插件需要加载的菜单项视图模型
    /// </summary>
    /// <returns></returns>
    IEnumerable<MenuTabItemViewModel>? GetMenuTabItems();

    /// <summary>
    /// 获取插件的配置项
    /// </summary>
    /// <param name="directoryExists"></param>
    /// <returns></returns>
    IEnumerable<(Action<IServiceCollection>? @delegate, bool isInvalid, string name)>? GetConfiguration(bool directoryExists);

    /// <summary>
    /// MainWindowViewModel.Initialize
    /// </summary>
    /// <returns></returns>
    ValueTask OnInitializeAsync();

    /// <summary>
    /// 配置按需使用的依赖注入服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="startup"></param>
    void ConfigureDemandServices(
        IServiceCollection services,
        Startup startup);

    /// <summary>
    /// 配置任何进程都必要的依赖注入服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="startup"></param>
    void ConfigureRequiredServices(
        IServiceCollection services,
        Startup startup);

    void ConfigureServices(IpcProvider ipcProvider,
        Startup startup)
    { }

    /// <summary>
    /// 配置 AutoMapper
    /// </summary>
    /// <param name="cfg"></param>
    void OnAddAutoMapper(IMapperConfigurationExpression cfg);

    /// <summary>
    /// 未处理的异常
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="name"></param>
    void OnUnhandledException(Exception ex, string name, bool? isTerminating = null);

    ValueTask OnExit();

    /// <summary>
    /// 启动子进程 IPC 程序的 Main 函数
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="pipeName"></param>
    /// <param name="processId"></param>
    /// <param name="encodedArgs"></param>
    /// <returns></returns>
    Task<int> RunSubProcessMainAsync(string moduleName, string pipeName, string processId, string encodedArgs);

    /// <summary>
    /// 当子进程 IPC 管道连接中
    /// </summary>
    /// <param name="isReconnected">是否为重连</param>
    /// <returns></returns>
    ValueTask OnPeerConnected(bool isReconnected);

    /// <summary>
    /// 解析程序带参数启动时执行指令
    /// </summary>
    /// <returns></returns>
    ValueTask OnCommandRun(params string[] commandParams);
}
