using dotnetCampus.Ipc.Pipes;

namespace BD.WTTS.Services;

/// <summary>
/// 子进程的 IPC 服务实现
/// </summary>
public interface IPCSubProcessService : IDisposable
{
    /// <summary>
    /// 启动子进程的 IPC 服务
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="tcs"></param>
    /// <param name="pipeName"></param>
    /// <param name="configureIpcProvider"></param>
    /// <returns></returns>
    Task RunAsync(string moduleName, TaskCompletionSource tcs, string pipeName, Action<IpcProvider>? configureIpcProvider = null);

    /// <summary>
    /// 获取主进程的 IPC 远程服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? GetService<T>() where T : class;

    private static IPCSubProcessServiceImpl? iPCSubProcessServiceImpl;

    static IPCSubProcessService Instance => iPCSubProcessServiceImpl.ThrowIsNull();

    /// <summary>
    /// 子进程 IPC 程序启动通用函数
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="configureServices">配置子进程的依赖注入服务</param>
    /// <param name="configureIpcProvider">配置 IPC 服务</param>
    /// <param name="args"></param>
    /// <returns></returns>
    static async Task<int> MainAsync(
        string moduleName,
        Action<IServiceCollection>? configureServices,
        Action<IpcProvider>? configureIpcProvider,
        params string[] args)
    {
        if (args.Length < 2)
            return (int)CommandExitCode.EmptyArrayArgs;
        var pipeName = args[0];
        if (string.IsNullOrWhiteSpace(pipeName))
            return (int)CommandExitCode.EmptyPipeName;
        if (!int.TryParse(args[1], out var pid))
            return (int)CommandExitCode.EmptyMainProcessId;
        var mainProcess = Process.GetProcessById(pid);
        if (mainProcess == null)
            return (int)CommandExitCode.NotFoundMainProcessId;

        TaskCompletionSource tcs = new();
        mainProcess.EnableRaisingEvents = true;
        mainProcess.Exited += (_, _) =>
        {
            tcs.TrySetResult(); // 监听主进程退出时关闭当前子进程
        };

#if LIB_CLIENT_IPC
        Ioc.ConfigureServices(services =>
        {
            services.AddSingleton(_ => Instance);
            configureServices?.Invoke(services);
        });
#endif

        try
        {
            iPCSubProcessServiceImpl = new(Ioc.Get<ILoggerFactory>());
            await iPCSubProcessServiceImpl
                .RunAsync(moduleName, tcs,
                pipeName, configureIpcProvider);
            await tcs.Task;
        }
        finally
        {
            await Ioc.DisposeAsync();
            iPCSubProcessServiceImpl?.Dispose();
        }

        return 0;
    }
}