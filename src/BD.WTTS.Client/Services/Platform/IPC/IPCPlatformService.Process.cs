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

    /// <summary>
    /// 根据多个进程名结束进程
    /// </summary>
    /// <param name="processNames"></param>
    bool KillProcesses(params string[] processNames)
    {
        // IPC 调用不等待直接返回
        Task2.InBackground(() =>
        {
            foreach (var p in processNames)
            {
                try
                {
                    var process = Process.GetProcessesByName(p);
                    foreach (var item in process)
                    {
                        if (!item.HasExited)
                        {
                            item.Kill();
                            item.WaitForExit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(IPCPlatformService), ex,
                        "KillProcesses fail, name: {name}", p);
                }
            }
        });
        // 返回 false 必定为 IPC 调用失败
        return true;
    }
}