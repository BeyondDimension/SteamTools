using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using dotnetCampus.Ipc.Context;
using dotnetCampus.Ipc.Pipes;
using dotnetCampus.Ipc.Utils.Logging;
using IPCLogLevel = dotnetCampus.Ipc.Utils.Logging.LogLevel;
using MSEXLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace BD.WTTS.Services.Implementation;

public sealed class IPCSubProcessServiceImpl : IPCSubProcessService
{
    bool disposedValue;
    IpcProvider? ipcProvider;
    PeerProxy? peer;
    TaskCompletionSource? tcs;
    readonly ILoggerFactory loggerFactory;

    public IPCSubProcessServiceImpl(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool TryGetProcessById(int pid, [NotNullWhen(true)] out Process? process)
    {
        try
        {
            process = Process.GetProcessById(pid);
            return true;
        }
        catch
        {
            // 异常不记录日志
            process = null;
            return false;
        }
    }

    static bool IsAllowedLinuxBundledHost(string localPath, string peerPath, int peerPid)
    {
        if (!OperatingSystem.IsLinux())
            return false;

        var hostIsLocal = string.Equals(Path.GetFileName(localPath), "dotnet", StringComparison.Ordinal);
        var hostPath = hostIsLocal ? localPath : peerPath;
        var acceleratorPath = hostIsLocal ? peerPath : localPath;
        if (!string.Equals(Path.GetFileName(hostPath), "dotnet", StringComparison.Ordinal) ||
            !string.Equals(Path.GetFileName(acceleratorPath), "Steam++.Accelerator", StringComparison.Ordinal))
            return false;

        var hostPid = hostIsLocal ? Environment.ProcessId : peerPid;
        try
        {
            var args = File.ReadAllText($"/proc/{hostPid}/cmdline").Split('\0');
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
                return false;
            var workingDirectory = Directory.ResolveLinkTarget($"/proc/{hostPid}/cwd", true)?.FullName;
            if (workingDirectory == null)
                return false;
            return IsAllowedLinuxBundledHostPath(hostPath, acceleratorPath, args[1], workingDirectory);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"[IPC startup] Could not inspect bundled host PID {hostPid}: {ex.Message}");
            return false;
        }
    }

    static bool IsAllowedLinuxBundledHostPath(string hostPath, string acceleratorPath, string assemblyPath, string workingDirectory)
    {
        hostPath = Path.GetFullPath(hostPath);
        acceleratorPath = Path.GetFullPath(acceleratorPath);
        var runtimeDirectory = Path.GetDirectoryName(hostPath);
        if (runtimeDirectory == null || !string.Equals(Path.GetFileName(runtimeDirectory), "dotnet", StringComparison.Ordinal))
            return false;
        var appDirectory = Path.GetDirectoryName(runtimeDirectory);
        if (appDirectory == null)
            return false;

        return string.Equals(hostPath, Path.Combine(appDirectory, "dotnet", "dotnet"), StringComparison.Ordinal) &&
            string.Equals(acceleratorPath, Path.Combine(appDirectory, "modules", "Accelerator", "Steam++.Accelerator"), StringComparison.Ordinal) &&
            string.Equals(Path.GetFullPath(assemblyPath, workingDirectory), Path.Combine(appDirectory, "assemblies", "Steam++.dll"), StringComparison.Ordinal);
    }

    internal static bool CheckBePid(Process proc)
    {
        if (proc.Id == Environment.ProcessId)
        {
            return true;
        }

        var procPath = Environment.ProcessPath;
        ArgumentException.ThrowIfNullOrWhiteSpace(procPath);
        var fn = proc.TryGetMainModule()?.FileName;
        if (fn == null)
            return false;

        string? debugAcceleratorPath = null;
#if DEBUG
        // Debug builds load the accelerator from the project output rather than modules/.
        var mainName = OperatingSystem.IsWindows() ? "Steam++.exe" : "Steam++";
        var mainPath = string.Equals(Path.GetFileName(procPath), mainName, StringComparison.OrdinalIgnoreCase) ? procPath : fn;
        var directory = Directory.GetParent(Path.GetFullPath(mainPath));
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "WattToolkit.slnx")))
            directory = directory.Parent;
        if (directory != null)
            debugAcceleratorPath = Path.Combine(directory.FullName, "src", "BD.WTTS.Client.Plugins.Accelerator.ReverseProxy", "bin", "Debug", $"net{Environment.Version.Major}.{Environment.Version.Minor}", OperatingSystem.IsWindows() ? "Steam++.Accelerator.exe" : "Steam++.Accelerator");
#endif
        return IsAllowedPeerPath(procPath, fn, debugAcceleratorPath) ||
            IsAllowedLinuxBundledHost(procPath, fn, proc.Id) ||
            AssemblyInfo.ValidateAssembly(fn);
    }

    static bool IsAllowedPeerPath(string localPath, string peerPath, string? debugAcceleratorPath)
    {
        localPath = Path.GetFullPath(localPath);
        peerPath = Path.GetFullPath(peerPath);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (string.Equals(localPath, peerPath, comparison))
            return true;

        var mainName = OperatingSystem.IsWindows() ? "Steam++.exe" : "Steam++";
        var acceleratorName = OperatingSystem.IsWindows() ? "Steam++.Accelerator.exe" : "Steam++.Accelerator";
        var mainPath = string.Equals(Path.GetFileName(localPath), mainName, comparison) ? localPath : peerPath;
        var acceleratorPath = mainPath == localPath ? peerPath : localPath;
        if (!string.Equals(Path.GetFileName(mainPath), mainName, comparison) ||
            !string.Equals(Path.GetFileName(acceleratorPath), acceleratorName, comparison))
            return false;

        var mainDirectory = Path.GetDirectoryName(mainPath)!;
        var contentsDirectory = Directory.GetParent(mainDirectory);
        var modulesDirectory = Path.Combine(mainDirectory, "modules");
        if (OperatingSystem.IsMacOS() &&
            string.Equals(Path.GetFileName(mainDirectory), "MacOS", comparison) &&
            contentsDirectory != null &&
            string.Equals(contentsDirectory.Name, "Contents", comparison))
        {
            modulesDirectory = Path.Combine(contentsDirectory.FullName, "MonoBundle", "modules");
        }
        var packagedPath = Path.GetFullPath(Path.Combine(modulesDirectory, "Accelerator", acceleratorName));
        return string.Equals(acceleratorPath, packagedPath, comparison) ||
            (debugAcceleratorPath != null && string.Equals(acceleratorPath, Path.GetFullPath(debugAcceleratorPath), comparison));
    }

    internal static bool CheckBePid(int pid)
    {
        if (pid == Environment.ProcessId)
        {
            return true;
        }
        if (!TryGetProcessById(pid, out var proc))
        {
            return false;
        }
        return CheckBePid(proc);
    }

    internal static void ValidateNamedPipeClientConnection(object? s, NamedPipeClientConnectingEventArgs e)
    {
#if WINDOWS
#if DEBUG
        Log.Info(nameof(IPCSubProcessServiceImpl), "服务端正在校验命名管道客户端进程，{ClientProcessId}", e.ClientProcessId);
#endif
        try
        {
            // 验证连接的客户端进程必须是自己的程序
            if (!e.ClientProcessId.HasValue ||
                !CheckBePid(e.ClientProcessId.Value))
            {
                e.AllowConnection = false;
                return;
            }
        }
        catch (Exception ex)
        {
            Log.Error(nameof(IPCSubProcessServiceImpl), ex, "IpcConfiguration.NamedPipeClientConnecting fail");
            e.AllowConnection = false;
        }
#endif
    }

    public async Task RunAsync(string moduleName, TaskCompletionSource tcs, string pipeName,
        Action<IpcProvider>? configureIpcProvider = null)
    {
        this.tcs = tcs;
        var ipcCfg = new IpcConfiguration
        {
            IpcLoggerProvider = _ => new IpcLogger_(loggerFactory, nameof(IPCSubProcessServiceImpl)),
        };
        ipcCfg.NamedPipeClientConnecting += ValidateNamedPipeClientConnection;
        ipcProvider = new IpcProvider(IPCSubProcessModuleService.Constants.GetClientPipeName(moduleName, pipeName), ipcCfg);
        ipcProvider.CreateIpcJoint<IPCSubProcessModuleService>(new IPCSubProcessModuleServiceImpl(this));
        configureIpcProvider?.Invoke(ipcProvider);
        ipcProvider.StartServer();

        peer = await ipcProvider.GetAndConnectToPeerAsync(pipeName);
    }

    sealed class IPCSubProcessModuleServiceImpl : IPCSubProcessModuleService
    {
        readonly IPCSubProcessServiceImpl impl;

        public IPCSubProcessModuleServiceImpl(IPCSubProcessServiceImpl impl)
        {
            this.impl = impl;
        }

        public void Dispose() => impl.Dispose();
    }

    public T? GetService<T>() where T : class
    {
        if (ipcProvider != null && peer != null)
        {
            return ipcProvider.CreateIpcProxy<T>(peer);
        }
        return default;
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
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
                tcs?.TrySetResult();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            peer = null;
            ipcProvider = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
