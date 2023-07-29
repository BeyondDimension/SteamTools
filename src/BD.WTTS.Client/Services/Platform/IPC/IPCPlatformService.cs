using dotnetCampus.Ipc.CompilerServices.Attributes;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 用于 IPC 的平台实现的服务
/// </summary>
[IpcPublic(Timeout = AssemblyInfo.IpcTimeout, IgnoresIpcException = false)]
public partial interface IPCPlatformService
{
#if DEBUG
    string GetDebugString()
    {
        return $"Pid: {Environment.ProcessId}, Exe: {Environment.ProcessPath}, Asm: {Assembly.GetAssembly(GetType())?.FullName}";
    }
#endif
}