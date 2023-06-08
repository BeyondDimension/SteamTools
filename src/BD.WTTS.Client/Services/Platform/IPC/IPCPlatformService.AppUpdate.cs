// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPCPlatformService
{
    /// <summary>
    /// 设置更新包的路径与 SHA384
    /// </summary>
    /// <param name="path"></param>
    /// <param name="sha384"></param>
    void SetUpdatePackageInfo(string path, byte[] sha384)
    {
#if !LIB_CLIENT_IPC
        IAppUpdateService.UpdatePackageInfo = (path, sha384);
#endif
    }
}
