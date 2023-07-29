// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPCPlatformService
{
    /// <summary>
    /// 使用管理员权限启动进程
    /// </summary>
    /// <param name="processStartInfo"></param>
    /// <returns></returns>
    Task<int> StartProcessAsAdministratorAsync(byte[]? processStartInfo_)
    {
        if (processStartInfo_ == default)
            return Task.FromResult(0);
        var processStartInfo = Serializable.DMP2<ProcessStartInfo>(processStartInfo_);
        if (processStartInfo == default)
            return Task.FromResult(0);
        var process = Process.Start(processStartInfo);
        if (process == null)
            return Task.FromResult(0);
        return Task.FromResult(process.Id);
    }
}