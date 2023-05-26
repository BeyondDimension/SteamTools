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

    const int ExitCode_EmptyArrayArgs = 4001;
    const int ExitCode_EmptyPipeName = 4002;
    const int ExitCode_EmptyMainProcessId = 4003;
    const int ExitCode_NotFoundMainProcessId = 4004;

    /// <summary>
    /// 子进程 IPC 程序启动通用函数
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="configureServices"></param>
    /// <param name="configureIpcProvider"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    static async Task<int> MainAsync(
        string moduleName,
        Action<IServiceCollection>? configureServices,
        Action<IpcProvider>? configureIpcProvider,
        params string[] args)
    {
        if (args.Length < 2)
            return ExitCode_EmptyArrayArgs;
        var pipeName = args[0];
        if (string.IsNullOrWhiteSpace(pipeName))
            return ExitCode_EmptyPipeName;
        if (!int.TryParse(args[1], out var pid))
            return ExitCode_EmptyMainProcessId;
        var mainProcess = Process.GetProcessById(pid);
        if (mainProcess == null)
            return ExitCode_NotFoundMainProcessId;

        TaskCompletionSource tcs = new();
        mainProcess.EnableRaisingEvents = true;
        mainProcess.Exited += (_, _) =>
        {
            tcs.TrySetResult(); // 监听主进程退出时关闭当前子进程
        };

        IPCSubProcessServiceImpl? ipc = null;
        Ioc.ConfigureServices(services =>
        {
            services.AddSingleton<IPCSubProcessService>(_ => ipc!);
            configureServices?.Invoke(services);
        });

        try
        {
            ipc = new IPCSubProcessServiceImpl(Ioc.Get<ILoggerFactory>());
            await ipc.RunAsync(moduleName, tcs, pipeName, configureIpcProvider);
            await tcs.Task;
        }
        finally
        {
            await Ioc.DisposeAsync();
            ipc?.Dispose();
        }

        return 0;
    }
}