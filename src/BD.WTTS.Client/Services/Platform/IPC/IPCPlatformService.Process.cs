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
    bool? KillProcesses(params string[] processNames)
    {
        var processes = processNames.Select(static x =>
        {
            try
            {
                var process = Process.GetProcessesByName(x);
                return process;
            }
            catch
            {
                return Array.Empty<Process>();
            }
        }).SelectMany(static x => x).ToArray();

        static ApplicationException? KillProcess(Process? process)
        {
            if (process == null)
                return default;
            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                return new ApplicationException(
                    $"KillProcesses fail, name: {process?.ProcessName}", ex);
            }
            return default;
        }

        try
        {
            if (processes.Any())
            {
                var tasks = processes.Select(x =>
                {
                    return Task.Run(() =>
                    {
                        return KillProcess(x);
                    });
                }).ToArray();
                Task.WaitAll(tasks);

                var innerExceptions = tasks.Select(x => x.Result!)
                    .Where(x => x != null).ToArray();
                if (innerExceptions.Any())
                {
                    throw new AggregateException(
                        "KillProcess fail", innerExceptions);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(nameof(IPCPlatformService), ex, "KillSteamProcess fail");
            return false;
        }

        return true;
    }
}