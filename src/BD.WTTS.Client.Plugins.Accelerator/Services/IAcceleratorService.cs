using dotnetCampus.Ipc.CompilerServices.Attributes;

namespace BD.WTTS.Services;

/// <summary>
/// 加速模块服务，连接前后端的胶水服务
/// </summary>
//[IpcPublic(Timeout = AssemblyInfo.IpcTimeout, IgnoresIpcException = false)]
public partial interface IAcceleratorService
{
    void InitStateSubscribe();
}
