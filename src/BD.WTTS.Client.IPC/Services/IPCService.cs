namespace BD.WTTS.Services;

public interface IPCService : IDisposable
{
    /// <summary>
    /// 启动 IPC 服务
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="tcs"></param>
    /// <param name="pipeName"></param>
    /// <returns></returns>
    Task RunAsync(string moduleName, TaskCompletionSource tcs, string pipeName);

    /// <summary>
    /// 获取服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? GetService<T>() where T : class;

    const int ExitCode_EmptyArrayArgs = 4001;
    const int ExitCode_EmptyPipeName = 4002;

    static async Task<int> MainAsync(
        string moduleName,
        Action<IServiceCollection>? configureServices,
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
        await ipc.RunAsync(moduleName, tcs, pipeName);
        await tcs.Task;

        return 0;
    }
}