// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public interface IPCService : IAsyncDisposable
{
    /// <summary>
    /// 启动 IPC 服务
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
}