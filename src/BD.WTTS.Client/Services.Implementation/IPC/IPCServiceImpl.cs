using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using dotnetCampus.Ipc.Pipes;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// 主进程的 IPC 服务实现
/// </summary>
public sealed partial class IPCServiceImpl : IPCService
{
    bool disposedValue;
    IpcProvider? ipcProvider;
    readonly ILogger logger;
    readonly List<string> moduleNames = new();

    public IPCServiceImpl(ILogger<IPCServiceImpl> logger)
    {
        this.logger = logger;
    }

    public Process? StartProcess(string fileName, Action<ProcessStartInfo>? configure = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = ipcProvider.ThrowIsNull().IpcContext.PipeName,
            UseShellExecute = false,
        };
        configure?.Invoke(psi);
        var process = Process.Start(psi);
        return process;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string _()
    {
#if DEBUG
        return "000";
#else
        return Random2.GenerateRandomString(randomChars: String2.LowerCaseLetters);
#endif
    }

    public void Run()
    {
        ipcProvider = new IpcProvider(
            $"wtts_ipc_{_()}_{Environment.TickCount64}_{Environment.ProcessId}");
        ConfigureServices();
        ipcProvider.StartServer();
    }

    public async ValueTask<T?> GetServiceAsync<T>(string moduleName) where T : class
    {
        if (ipcProvider == null)
            return default;

        try
        {
            var peerName = IPCModuleService.Constants.GetClientPipeName(
                moduleName, ipcProvider.IpcContext.PipeName);
            var peer = await ipcProvider.GetAndConnectToPeerAsync(peerName);

            if (peer != null)
            {
                var s = ipcProvider.CreateIpcProxy<T>(peer);
                return s;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "get service fail, moduleName: {moduleName}, T: {t}.",
                moduleName,
                typeof(T));
        }

        return default;
    }

    async Task<(bool result, string moduleName)> ExitModuleCoreAsync(string moduleName)
    {
        try
        {
            var module = await GetServiceAsync<IPCModuleService>(moduleName);
            module.ThrowIsNull().Dispose();

            return (true, moduleName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "exit module fail, moduleName: {moduleName}.", moduleName);
            return (false, moduleName);
        }
    }

    public Task StartModule(string moduleName)
    {
        moduleNames.Add(moduleName);
        return Task.CompletedTask;
    }

    public async Task<bool> ExitModule(string moduleName)
    {
        (var r, var _) = await ExitModuleCoreAsync(moduleName);
        if (r) moduleNames.Remove(moduleName);
        return r;
    }

    public async Task<bool> ExitModules(IEnumerable<string> moduleNames)
    {
        var tasks = moduleNames.Select(ExitModuleCoreAsync);
        var result = await Task.WhenAll(tasks);
        foreach ((bool r, string moduleName) in result)
        {
            if (r) this.moduleNames.Remove(moduleName);
        }
        return result.All(static x => x.result);
    }

    /// <summary>
    /// 配置服务
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ConfigureServices()
    {
        RegisterService<IPCPlatformService, IPlatformService>();
        RegisterService<IPCToastService>(this);
    }

    /// <summary>
    /// 注册 IPC 调用服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void RegisterService<T>()
        where T : class
        => ipcProvider!.CreateIpcJoint(Ioc.Get<T>());

    /// <summary>
    /// 注册 IPC 调用服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TImpl"></typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void RegisterService<T, TImpl>()
        where T : class
        where TImpl : T
        => ipcProvider!.CreateIpcJoint<T>(Ioc.Get<TImpl>());

    /// <summary>
    /// 注册 IPC 调用服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void RegisterService<T>(T value)
        where T : class
        => ipcProvider!.CreateIpcJoint(value);

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    async ValueTask DisposeAsyncCore()
    {
        if (!disposedValue)
        {
            if (moduleNames.Any())
            {
                await ExitModules(moduleNames);
            }

            // 释放托管状态(托管对象)
            try
            {
                ipcProvider?.Dispose();
            }
            catch (InvalidOperationException)
            {
                // Unhandled exception. System.InvalidOperationException: 未启动之前，不能获取 IpcServerService 属性的值
                // at dotnetCampus.Ipc.Pipes.IpcProvider.get_IpcServerService()
                // at dotnetCampus.Ipc.Pipes.IpcProvider.Dispose()
                // at BD.WTTS.Services.Implementation.IPCServiceImpl.Dispose(Boolean disposing)
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            ipcProvider = null;
            disposedValue = true;
        }
    }
}
