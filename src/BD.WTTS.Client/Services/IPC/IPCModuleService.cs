// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 子进程模块的 IPC 服务，调用 <see cref="IDisposable.Dispose"/> 退出子进程
/// </summary>
public interface IPCModuleService : IDisposable
{
    static string GetClientPipeName(string moduleName, string pipeName)
        => $"{pipeName}_{moduleName}";
}
