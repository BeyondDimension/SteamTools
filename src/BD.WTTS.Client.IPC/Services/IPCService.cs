using dotnetCampus.Ipc.Pipes;

namespace BD.WTTS.Services;

/// <summary>
/// 子进程的 IPC 服务实现
/// </summary>
public interface IPCService : IDisposable
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
        if (!args.Any())
            return ExitCode_EmptyArrayArgs;
        var pipeName = args[0];
        if (string.IsNullOrWhiteSpace(pipeName))
            return ExitCode_EmptyPipeName;

        using var ipc = new IPCServiceImpl();

        Ioc.ConfigureServices(services =>
        {
            services.AddSingleton<IPCService>(ipc);
            configureServices?.Invoke(services);
        });

        TaskCompletionSource tcs = new();
        await ipc.RunAsync(moduleName, tcs, pipeName, configureIpcProvider);
        await tcs.Task;

        return 0;
    }
}