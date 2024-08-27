// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 主进程的 IPC 服务
/// </summary>
public interface IPCMainProcessService : IAsyncDisposable
{
    static IPCMainProcessService Instance => Ioc.Get<IPCMainProcessService>();

    /// <summary>
    /// 添加模块的守护进程并启动进程
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="delegate"></param>
    /// <param name="callStart"></param>
    /// <returns></returns>
    Process? AddDaemonWithStartSubProcess(string moduleName, Func<IPCMainProcessService, Process?> @delegate);

    /// <summary>
    /// 添加模块的守护进程并启动进程
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="delegate"></param>
    /// <param name="callStart"></param>
    /// <returns></returns>
    ValueTask<Process?> AddDaemonWithStartSubProcessAsync(string moduleName, Func<IPCMainProcessService, ValueTask<Process?>> @delegate);

    /// <summary>
    /// 启动子进程
    /// </summary>
    ValueTask<Process?> StartSubProcessAsync(
        string fileName,
        bool isAdministrator = false,
        Action<ProcessStartInfo>? configure = null);

    /// <summary>
    /// 启动主进程的 IPC 服务
    /// </summary>
    void Run();

    /// <summary>
    /// 启动模块
    /// </summary>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    Task StartModule(string moduleName);

    /// <summary>
    /// 根据模块名称退出模块
    /// </summary>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    Task<bool> ExitModule(string moduleName);

    /// <summary>
    /// 根据多个模块名称退出模块
    /// </summary>
    /// <param name="moduleNames"></param>
    /// <returns></returns>
    Task<bool> ExitModules(IEnumerable<string> moduleNames);

    /// <summary>
    /// 获取子进程实现的 IPC 远程服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    ValueTask<T?> GetServiceAsync<T>(string moduleName) where T : class;

    void WriteMessage(string? moduleName, byte[] bytes);
}