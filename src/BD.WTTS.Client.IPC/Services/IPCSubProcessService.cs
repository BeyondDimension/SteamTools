using dotnetCampus.Ipc.Pipes;
using static BD.WTTS.Services.IPCSubProcessModuleService.Constants;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SubProcessArgumentIndex2Model? GetSubProcessArgumentIndex2Model(string s)
    {
        try
        {
            var b = s.Base64UrlDecodeToByteArray();
            var m = Serializable.DMP2<SubProcessArgumentIndex2Model>(b);
            return m;
        }
        catch
        {
            return null;
        }
    }

    static string LogDirPath { get; private set; } = null!;

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

    /// <summary>
    /// 子进程 IPC 程序启动通用函数
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="pluginName"></param>
    /// <param name="configureServices">配置子进程的依赖注入服务</param>
    /// <param name="configureIpcProvider">配置 IPC 服务</param>
    /// <param name="args"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static async Task<int> MainAsync(
        string moduleName,
        string? pluginName,
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
        if (!TryGetProcessById(pid, out var mainProcess))
            return (int)CommandExitCode.NotFoundMainProcessId;

#if LIB_CLIENT_IPC
        var nativeLibraryPath = Environment.GetEnvironmentVariable(EnvKey_NativeLibraryPath);
        if (!string.IsNullOrWhiteSpace(nativeLibraryPath))
        {
            var nativeLibraryPaths = nativeLibraryPath.Split(';',
                StringSplitOptions.RemoveEmptyEntries);

            // 监听当前应用程序域的程序集加载
            AppDomain.CurrentDomain.AssemblyLoad += (_, args)
                => CurrentDomain_AssemblyLoad(args.LoadedAssembly);
            void CurrentDomain_AssemblyLoad(Assembly loadedAssembly)
            {
                try
                {
                    nint Delegate(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
                    {
                        static string GetLibraryFileName(string libraryName, string? fileExtension = null)
                        {
                            string fileExtension_ = ".so";
                            if (OperatingSystem.IsWindows())
                            {
                                fileExtension_ = ".dll";
                            }
                            else if (OperatingSystem.IsMacCatalyst() || OperatingSystem.IsMacOS())
                            {
                                fileExtension_ = ".dylib";
                            }
                            fileExtension ??= fileExtension_;
                            if (!libraryName.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                                libraryName += fileExtension;
                            return libraryName;
                        }
                        var libraryFileName = GetLibraryFileName(libraryName);
                        string libraryPath = libraryName;
                        try
                        {
                            foreach (var nativeLibraryPath in nativeLibraryPaths)
                            {
                                libraryPath = Path.Combine(nativeLibraryPath, libraryFileName);
                                if (File.Exists(libraryPath) &&
                                    NativeLibrary.TryLoad(libraryPath, out var handle))
                                {
                                    return handle;
                                }
                                else if (!OperatingSystem.IsWindows() && !libraryFileName.StartsWith("lib", StringComparison.OrdinalIgnoreCase))
                                {
                                    libraryFileName = $"lib{libraryFileName}";
                                    libraryPath = Path.Combine(nativeLibraryPath, libraryFileName);
                                    if (File.Exists(libraryPath) &&
                                        NativeLibrary.TryLoad(libraryPath, out handle))
                                    {
                                        return handle;
                                    }
                                }
                            }
                            return NativeLibrary.Load(libraryName, assembly, searchPath);
                        }
                        catch (Exception ex)
                        {
                            throw new FileNotFoundException(null, libraryPath, ex);
                        }
                    }
                    NativeLibrary.SetDllImportResolver(loadedAssembly, Delegate);
                }
                catch
                {
                    // ArgumentNullException assembly 或 resolver 为 null。
                    // ArgumentException 已为此程序集设置解析程序。
                    // 此每程序集解析程序是第一次尝试解析此程序集启动的本机库加载。
                    // 此方法的调用方应仅为自己的程序集注册解析程序。
                    // 每个程序集只能注册一个解析程序。 尝试注册第二个解析程序失败并出现 InvalidOperationException。
                    // https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.interopservices.nativelibrary.setdllimportresolver
                }
            }
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
                CurrentDomain_AssemblyLoad(assembly);
        }
#endif

        SubProcessArgumentIndex2Model? m = default;
        if (pluginName != null)
        {
            if (args.Length < 3)
                return (int)CommandExitCode.EmptyArrayArgs;
            m = GetSubProcessArgumentIndex2Model(args[2]);
            if (m == null)
                return (int)CommandExitCode.SubProcessArgumentIndex2ModelIsNull;

            IPCSubProcessFileSystem.InitFileSystem(m, pluginName);
        }

        string? logDirPath = null;
        IPCSubProcessFileSystem.InitLog(ref logDirPath, moduleName, cacheDirectory: m?.CacheDirectory);
        LogDirPath = logDirPath!;

        TaskCompletionSource tcs = new();
        mainProcess.EnableRaisingEvents = true;
        mainProcess.Exited += (_, _) =>
        {
            tcs.TrySetResult(); // 监听主进程退出时关闭当前子进程
        };

#if DEBUG
        Console.WriteLine("mainProcess");
#endif

        try
        {
#if LIB_CLIENT_IPC
            Ioc.ConfigureServices(services =>
            {
                services.AddSingleton(_ => Instance);
                configureServices?.Invoke(services);
            });
#endif
            var loggerFactory = Ioc.Get<ILoggerFactory>();
            iPCSubProcessServiceImpl = new(loggerFactory);
#if DEBUG
            Console.WriteLine("iPCSubProcessServiceImpl");
#endif
            await iPCSubProcessServiceImpl
                .RunAsync(moduleName, tcs,
                pipeName, configureIpcProvider);
#if DEBUG
            Console.WriteLine("RunAsync");
#endif
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }

        try
        {
#if DEBUG
            Console.WriteLine("tcs");
#endif
            await tcs.Task;
#if DEBUG
            Console.WriteLine("tcs2");
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return (int)CommandExitCode.HttpStatusCodeInternalServerError;
        }
        finally
        {
#if DEBUG
            Console.WriteLine("finally");
#endif
            await Ioc.DisposeAsync();
            iPCSubProcessServiceImpl?.Dispose();
        }

        return 0;
    }

    const string EnvKey_NativeLibraryPath = "WTTS_NATIVE_LIBRARY_PATH";
}

[MP2Obj(SerializeLayout.Explicit)]
sealed partial record class SubProcessArgumentIndex2Model
{
    [MP2Key(0)]
    public string AppDataDirectory { get; init; } = null!;

    [MP2Key(1)]
    public string CacheDirectory { get; init; } = null!;
}

sealed class IPCSubProcessFileSystem : IOPath.FileSystemBase
{
    private IPCSubProcessFileSystem() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InitFileSystem(SubProcessArgumentIndex2Model model, string pluginName)
    {
        var appDataDirectory = GetPluginsDirectory(pluginName, model.AppDataDirectory);
        var cacheDirectory = GetPluginsDirectory(pluginName, model.CacheDirectory);
        InitFileSystem(() => appDataDirectory, () => cacheDirectory);
    }

    public const LogLevel DefaultLoggerMinLevel = AssemblyInfo.Debuggable ? LogLevel.Debug : LogLevel.Error;

    public static readonly NLogLevel DefaultNLoggerMinLevel = ConvertLogLevel(DefaultLoggerMinLevel);

    /// <summary>
    /// Convert log level to NLog variant.
    /// </summary>
    /// <param name="logLevel">level to be converted.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NLogLevel ConvertLogLevel(LogLevel logLevel) => logLevel switch
    {
        // https://github.com/NLog/NLog.Extensions.Logging/blob/v1.7.0/src/NLog.Extensions.Logging/Logging/NLogLogger.cs#L416
        LogLevel.Trace => NLogLevel.Trace,
        LogLevel.Debug => NLogLevel.Debug,
        LogLevel.Information => NLogLevel.Info,
        LogLevel.Warning => NLogLevel.Warn,
        LogLevel.Error => NLogLevel.Error,
        LogLevel.Critical => NLogLevel.Fatal,
        LogLevel.None => NLogLevel.Off,
        _ => NLogLevel.Debug,
    };

    public const string LogDirName = "Logs";

    static void InitializeTarget(LoggingConfiguration config, Target target, NLogLevel minLevel)
    {
        config.AddTarget(target);
        if (minLevel < NLogLevel.Warn)
        {
            config.LoggingRules.Add(new LoggingRule("Microsoft*", target) { FinalMinLevel = NLogLevel.Warn });
            config.LoggingRules.Add(new LoggingRule("Microsoft.Hosting.Lifetime*", target) { FinalMinLevel = NLogLevel.Info });
            config.LoggingRules.Add(new LoggingRule("System*", target) { FinalMinLevel = NLogLevel.Warn });
        }
        config.LoggingRules.Add(new LoggingRule("*", minLevel, target));
    }

    public static string GetLogDirPath(string? moduleName = null, string? cacheDirectory = null)
    {
        cacheDirectory ??= IOPath.CacheDirectory;

        string logDirPath;
        if (moduleName == null)
        {
            logDirPath = Path.Combine(cacheDirectory, LogDirName);
        }
        else
        {
            var dirName = GetDirectoryName(moduleName);
            logDirPath = Path.Combine(cacheDirectory, LogDirName, dirName);
        }

        IOPath.DirCreateByNotExists(logDirPath);
        return logDirPath;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InitLog(
        ref string? logDirPath,
        string? moduleName = null,
        string? alias = null,
        string? cacheDirectory = null
#if STARTUP_WATCH_TRACE || DEBUG
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1001 // Commas should be spaced correctly
#pragma warning disable SA1115 // Parameter should follow comma
#pragma warning disable SA1113 // Comma should be on the same line as previous parameter
         , bool watchTrace = false
#pragma warning restore SA1113 // Comma should be on the same line as previous parameter
#pragma warning restore SA1115 // Parameter should follow comma
#pragma warning restore SA1001 // Commas should be spaced correctly
#pragma warning restore IDE0079 // 请删除不必要的忽略
#endif
         )
    {
        if (!string.IsNullOrEmpty(logDirPath))
            return; // 已初始化过日志文件夹路径

        cacheDirectory ??= IOPath.CacheDirectory;

        logDirPath = GetLogDirPath(moduleName, cacheDirectory);

#if (STARTUP_WATCH_TRACE || DEBUG) && !LIB_CLIENT_IPC
        if (watchTrace) Startup.WatchTrace.Record("InitLog.IOPath");
#endif
        var logDirPath_ = logDirPath + Path.DirectorySeparatorChar;
        NInternalLogger.LogFile = logDirPath_ + "internal-nlog" + alias + ".txt";
        NInternalLogger.LogLevel = NLogLevel.Error;
        var objConfig = new LoggingConfiguration();
        var defMinLevel = DefaultNLoggerMinLevel;
        var logfile = new FileTarget("logfile")
        {
            FileName = logDirPath_ + "nlog-all-${shortdate}" + alias + ".log",
            Layout = "${longdate}|${level}|${logger}|${message} |${all-event-properties} ${exception:format=tostring}",
            ArchiveAboveSize = 10485760,
            MaxArchiveFiles = 14,
            MaxArchiveDays = 7,
        };
        objConfig.AddTarget(logfile);
        InitializeTarget(objConfig, logfile, defMinLevel);
#if (STARTUP_WATCH_TRACE || DEBUG) && !LIB_CLIENT_IPC
        if (watchTrace) Startup.WatchTrace.Record("InitLog.CreateConfig");
#endif
        NLogManager.Configuration = objConfig;
        Log.LoggerFactory = () => new LoggerFactory(new[] { new NLogLoggerProvider() });
#if (STARTUP_WATCH_TRACE || DEBUG) && !LIB_CLIENT_IPC
        if (watchTrace) Startup.WatchTrace.Record("InitLog.End");
#endif
    }
}