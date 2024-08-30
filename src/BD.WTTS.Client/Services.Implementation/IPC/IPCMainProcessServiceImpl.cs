using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using dotnetCampus.Ipc.Context;
using dotnetCampus.Ipc.Pipes;
using dotnetCampus.Ipc.Utils.Logging;
using IPCLogLevel = dotnetCampus.Ipc.Utils.Logging.LogLevel;
using MSEXLogLevel = Microsoft.Extensions.Logging.LogLevel;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// 主进程的 IPC 服务实现
/// </summary>
public sealed partial class IPCMainProcessServiceImpl : IPCMainProcessService
{
    bool disposedValue;
    IpcProvider? ipcProvider;
    readonly ILogger logger;
    readonly List<string> moduleNames = new();
    readonly ILoggerFactory loggerFactory;

    public IPCMainProcessServiceImpl(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
        logger = loggerFactory.CreateLogger<IPCMainProcessServiceImpl>();
    }

    sealed class IpcLogger_ : IpcLogger
    {
        readonly ILogger logger;

        public IpcLogger_(ILoggerFactory loggerFactory, string name) : base(name)
        {
            logger = loggerFactory.CreateLogger(name);
        }

        static MSEXLogLevel Convert(IPCLogLevel logLevel)
        {
            if (logLevel <= IPCLogLevel.Debug)
                return MSEXLogLevel.Debug;
            return (MSEXLogLevel)logLevel;
        }

        protected override void Log<TState>(
            IPCLogLevel logLevel,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            logger.Log(Convert(logLevel), default, state, exception, formatter);
        }
    }

    readonly ConcurrentDictionary<string, Process> subProcesses = new();
    readonly ConcurrentDictionary<string, Func<IPCMainProcessService, ValueTask<Process?>>> startSubProcesses = new();
    readonly ConcurrentBag<string> isReconnected = new();

    void AddSubProcess(string moduleName, Process? process)
    {
        if (process == null)
            return;

        if (subProcesses.TryGetValue(moduleName, out var process1))
        {
            bool hasExited = false;
            try
            {
                hasExited = process1.HasExited;
            }
            catch
            {

            }
            if (!hasExited)
            {
                try
                {
                    subProcesses.TryRemove(moduleName, out var _);
                    process1.KillEntireProcessTree();
                }
                catch
                {

                }
            }
            subProcesses[moduleName] = process;
        }
        else
        {
            subProcesses.TryAdd(moduleName, process);
        }
    }

    public Process? AddDaemonWithStartSubProcess(string moduleName, Func<IPCMainProcessService, Process?> @delegate)
    {
        ValueTask<Process?> StartSubProcessDelegate(IPCMainProcessService ipc)
        {
            var process = @delegate(ipc);
            return ValueTask.FromResult(process);
        }

        if (startSubProcesses.ContainsKey(moduleName))
        {
            startSubProcesses[moduleName] = StartSubProcessDelegate;
        }
        else
        {
            startSubProcesses.TryAdd(moduleName, StartSubProcessDelegate);
        }
        var process = @delegate?.Invoke(this);
        AddSubProcess(moduleName, process);
        return process;
    }

    public async ValueTask<Process?> AddDaemonWithStartSubProcessAsync(string moduleName, Func<IPCMainProcessService, ValueTask<Process?>> @delegate)
    {
        if (startSubProcesses.ContainsKey(moduleName))
        {
            startSubProcesses[moduleName] = @delegate;
        }
        else
        {
            startSubProcesses.TryAdd(moduleName, @delegate);
        }
        var process = await @delegate.Invoke(this);
        AddSubProcess(moduleName, process);
        return process;
    }

    public async ValueTask<Process?> StartSubProcessAsync(
        string fileName,
        bool isAdministrator = false,
        Action<ProcessStartInfo>? configure = null)
    {
        var pipeName = ipcProvider.ThrowIsNull().IpcContext.PipeName;
        var pid = Environment.ProcessId;
        //        const bool useShellExecute =
        //#if DEBUG
        //            true; // 调试模式下此子进程与主进程不使用同一个控制台窗口
        //#else
        //            false;
        //#endif
        // System.InvalidOperationException: The Process object must have the UseShellExecute property set to false in order to use environment variables.
        const bool useShellExecute = false;
        const bool createNoWindow =
#if DEBUG
            false; // 调试模式下显示窗口
#else
            true;
#endif

#if LINUX 
        // 构建要执行的 shell 命令
        var shellStr =
            $"if [ -x \"{fileName}\" ]; then echo \"文件具有执行权限。\"; else chmod +x \"{fileName}\"; echo \"文件没有执行权限。\"; fi";
        // Linux 启动子模块前需要判断是否有 Exec 执行权限 
        Process.Start(Process2.BinBash, new string[]
        {
            "-c",
            shellStr,
        }).WaitForExit();

#endif

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = useShellExecute,
            CreateNoWindow = createNoWindow,
        };
        psi.ArgumentList.Add(pipeName);
        psi.ArgumentList.Add(pid.ToString());
        psi.ArgumentList.Add(mSubProcessArgumentIndex2Model.Value);
        configure?.Invoke(psi);
#if !MACOS
        DotNetRuntimeHelper.AddEnvironment(psi);
#endif
        var nativeLibraryPath = Startup.NativeLibraryPath;
        if (!string.IsNullOrWhiteSpace(nativeLibraryPath))
        {
            psi.Environment.TryAdd(
                IPCSubProcessService.EnvKey_NativeLibraryPath,
                nativeLibraryPath);
        }
        if (isAdministrator
#if WINDOWS
            && !WindowsPlatformServiceImpl.IsPrivilegedProcess
#endif
            )
        {
            var psi_ = Serializable.SMP2(psi);
            var startPid = await IPlatformService.Instance.StartProcessAsAdministratorAsync(psi_);
            if (startPid == default)
                return default;
            try
            {
                var process = Process.GetProcessById(startPid);
                return process;
            }
            catch
            {
                return default;
            }
        }
        else
        {
            var process = Process.Start(psi);
            return process;
        }
    }

    readonly Lazy<string> mSubProcessArgumentIndex2Model = new(() =>
    {
        var m = new SubProcessArgumentIndex2Model
        {
            AppDataDirectory = IOPath.AppDataDirectory,
            CacheDirectory = IOPath.CacheDirectory,
        };
        var b = Serializable.SMP2(m);
        var s = b.Base64UrlEncode();
        return s;
    });

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
        var tickCount64 = Environment.TickCount64;
        var pid = Environment.ProcessId;
        ipcProvider = new IpcProvider(
            $"ipc_{_()}{tickCount64}{pid / 3}{pid % 3}",
            new IpcConfiguration
            {
                AutoReconnectPeers = true, // 允许重连
                IpcLoggerProvider = _ => new IpcLogger_(loggerFactory, nameof(IPCMainProcessServiceImpl)),
            });
        ConfigureServices();
        ipcProvider.StartServer();
        ipcProvider.PeerConnected += IpcProvider_PeerConnected;

#if WINDOWS
        // 启动管理员权限服务进程
        Task2.InBackground(async () =>
        {
            var processPath = Environment.ProcessPath;
            processPath.ThrowIsNull();
            var pipeName = ipcProvider.ThrowIsNull().IpcContext.PipeName;
            const string arguments_ =
                $"-clt {IPlatformService.IPCRoot.CommandName} {IPlatformService.IPCRoot.args_PipeName} {{0}} {IPlatformService.IPCRoot.args_ProcessId} {{1}}";
            var arguments = string.Format(arguments_, pipeName, pid);
            await AddDaemonWithStartSubProcessAsync(IPlatformService.IPCRoot.moduleName, async _ =>
            {
                return await Policy.HandleResult<Process?>(x => x == null)
                    .RetryAsync(3)
                    .ExecuteAsync(async () =>
                {
                    var process = await WindowsPlatformServiceImpl.StartAsAdministrator(processPath, arguments);
                    return process;
                });
            });
        });
#endif
    }

    async void IpcProvider_PeerConnected(object? sender, PeerConnectedArgs e)
    {
#if DEBUG
        logger.LogError("收到 {peerName} 连接", e.Peer.PeerName);
#endif
        var pipeName = ipcProvider.ThrowIsNull().IpcContext.PipeName;
        var moduleName = e.Peer.PeerName.TrimStart($"{pipeName}_");

        var isReconnected = this.isReconnected.Contains(moduleName);
        if (!isReconnected) this.isReconnected.Add(moduleName);

        switch (moduleName)
        {
            case IPlatformService.IPCRoot.moduleName:
                if (!isReconnected)
                {
                    await IPlatformService.IPCRoot.SetIPC(this);
                }
                return;
        }

        if (Startup.Instance.TryGetPlugins(out var plugins))
        {
            foreach (var plugin in plugins)
            {
                if (plugin.UniqueEnglishName != moduleName)
                    continue;
                try
                {
                    await plugin.OnPeerConnected(isReconnected);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "IpcProvider_PeerConnected fail.");
                }
            }
        }
    }

    readonly HashSet<int> peerHashCodes = new();
    //readonly ConcurrentDictionary<string, DateTime> peerConnectionBrokenTimer = new();

    public async ValueTask<T?> GetServiceAsync<T>(string moduleName) where T : class
    {
        if (ipcProvider == null)
            return default;

        try
        {
            var peerName = IPCSubProcessModuleService.Constants.GetClientPipeName(
                moduleName, ipcProvider.IpcContext.PipeName);
            var peer = await ipcProvider.GetAndConnectToPeerAsync(peerName);

            if (peer != null)
            {
                var peerHashCode = peer.GetHashCode();
                if (peerHashCodes.Add(peerHashCode)) // peer 委托避免重复 +=
                {
#if DEBUG
                    peer.MessageReceived += (_, e) =>
                    {

                    };
                    peer.PeerReconnected += (_, _) =>
                    {
#if DEBUG
                        logger.LogError("断开重连 {peerName}", peer.PeerName);
#endif
                    };
#endif

                    peer.PeerConnectionBroken += async (_, _) =>
                    {
                        if (disposedValue || ipcProvider == null)
                            return; // 被释放或者 ipc 提供者为 null 则跳过
#if DEBUG
                        logger.LogError("连接断开 {peerName}", peer.PeerName);
#endif
                        //try
                        //{
                        //    if (peerConnectionBrokenTimer.TryGetValue(moduleName, out var time))
                        //    {
                        //        if ((DateTime.Now - time).TotalMilliseconds >= 3700.5D)
                        //        {
                        //            // 4.7s 内多次触发不执行重新启动进程
                        //            return;
                        //        }
                        //    }
                        //}
                        //finally
                        //{
                        //    // 记录此模块当前断开连接的时间
                        //    peerConnectionBrokenTimer.AddOrUpdate(moduleName,
                        //        static _ => DateTime.Now,
                        //        static (_, _) => DateTime.Now);
                        //}

                        if (subProcesses.TryGetValue(moduleName, out var subProcess))
                        {
                            subProcesses.TryRemove(moduleName, out var _);
                            if (!subProcess.HasExited)
                            {
                                subProcess.WaitForExit(3700);
                                if (!subProcess.HasExited)
                                {
                                    try
                                    {
                                        subProcess.KillEntireProcessTree();
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }

                        if (startSubProcesses.TryGetValue(moduleName, out var startSubProcess))
                        {
                            // 连接断开时重新启动进程
                            await startSubProcess.Invoke(this);
                        }
                    };
                }

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
            var module = await GetServiceAsync<IPCSubProcessModuleService>(moduleName);
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

    public void WriteMessage(string? moduleName, byte[] bytes)
    {
#if !ANDROID && !IOS
        LogConsoleService.Current.WriteMessage(moduleName, bytes);
#endif
    }

    /// <summary>
    /// 配置服务
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ConfigureServices()
    {
        RegisterService<IPCPlatformService, IPlatformService>();
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
        IHostsFileService? hostsFileService = Ioc.Get_Nullable<IHostsFileService>();
        if (hostsFileService != null)
            RegisterService(hostsFileService);
#endif
        RegisterService<IPCToastService>(this);

        var s = Startup.Instance;
        if (s.TryGetPlugins(out var plugins))
        {

            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.ConfigureServices(ipcProvider!, s);
                }
                catch (Exception ex)
                {
                    //GlobalExceptionHandler.Handler(ex, $"{plugin.UniqueEnglishName}.ConfigureRequiredServices");
                }
            }
        }
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
