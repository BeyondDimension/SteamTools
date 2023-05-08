using dotnetCampus.Ipc.CompilerServices.Attributes;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 子进程模块的 IPC 服务，调用 <see cref="IDisposable.Dispose"/> 退出子进程
/// </summary>
[IpcPublic(Timeout = 1000, IgnoresIpcException = false)]
public interface IPCModuleService : IDisposable
{
    static class Constants
    {
        public static string GetClientPipeName(string moduleName, string pipeName)
            => $"{pipeName}_{moduleName}";
    }
}
