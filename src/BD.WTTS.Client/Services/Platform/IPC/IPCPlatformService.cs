using dotnetCampus.Ipc.CompilerServices.Attributes;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 用于 IPC 的 <see cref="IPlatformService"/>
/// </summary>
[IpcPublic(Timeout = 1000, IgnoresIpcException = false)]
public partial interface IPCPlatformService
{

}