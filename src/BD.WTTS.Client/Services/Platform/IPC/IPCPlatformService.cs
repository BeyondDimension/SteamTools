using dotnetCampus.Ipc.CompilerServices.Attributes;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 用于 IPC 的平台实现的服务
/// </summary>
[IpcPublic(Timeout = 55000, IgnoresIpcException = false)]
public partial interface IPCPlatformService
{

}