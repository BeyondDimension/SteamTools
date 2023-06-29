#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using System.CommandLine;
#endif
#if WINDOWS_DESKTOP_BRIDGE
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
#endif

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

/// <summary>
/// 应用程序启动
/// </summary>
public abstract partial class Startup
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    readonly string[]? args;

    public Startup(string[]? args = null)
    {
        // 从 Main 函数中启动传递 string[] args，从其他地方启动传递 null
        this.args = args;
    }

    /// <summary>
    /// 获取命令行参数，返回 <see langword="null"/> 时执行退出
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    string[]? GetCommandLineArgs()
    {
        if (IsDesignMode)
            return Array.Empty<string>();

#if WINDOWS_DESKTOP_BRIDGE
        var activatedArgs = AppInstance.GetActivatedEventArgs();
        if (activatedArgs != null)
        {
            if (activatedArgs.Kind == ActivationKind.StartupTask)
            {
                return IPlatformService.SystemBootRunArguments.Split(' ');
            }
        }
#endif

        Span<string> args;
        if (this.args != null)
        {
            args = this.args.AsSpan();
        }
        else
        {
            // Environment.GetCommandLineArgs()[0] 为程序启动进程文件路径
            args = Environment.GetCommandLineArgs().AsSpan();
            args = args[1..];
        }

        IsMainProcess = args.IsEmpty;
        IsConsoleLineToolProcess = !IsMainProcess &&
            string.Equals(args[0], clt_, StringComparison.OrdinalIgnoreCase);

        if (IsConsoleLineToolProcess) // 命令行模式
        {
            if (args.Length == 2 &&
                string.Equals(args[1], command_main,
                StringComparison.OrdinalIgnoreCase))
            {
                // -clt main 禁止使用此参数
                return null;
            }
            else
            {
                args = args[1..];
                if (args.IsEmpty)
                {
                    // 无参数且不为主进程的清空使用 help 参数
                    return new[] { help_ };
                }
                return args.ToArray();
            }
        }
        else
        {
            // 返回启动主进程的参数
            return new[] { command_main };
        }
    }

    /// <summary>
    /// 启动应用程序
    /// </summary>
    /// <returns></returns>
    public virtual async Task<int> StartAsync()
    {
#if DEBUG && WINDOWS
        if (!IsDesignMode)
        {
            var apartmentState = Thread.CurrentThread.GetApartmentState();
            if (apartmentState != ApartmentState.STA)
            {
                throw new ArgumentOutOfRangeException("CurrentThread != ApartmentState.STA");
            }
        }
#endif

        try
        {
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Start();
#endif

            var args = GetCommandLineArgs();
            if (args == null)
                return 0;

            #region PlatformPrerequisite 平台先决条件

            if (!IsDesignMode) // 仅在非设计器中执行
            {
#if WINDOWS // Windows 需要检查兼容性
                if (!IsCustomEntryPoint && !CompatibilityCheck(AppContext.BaseDirectory))
                    return 0;
#elif MACOS // macOS 需要初始化 NSApplication
                NSApplication.Init();
#endif
#if WINDOWS_DESKTOP_BRIDGE
                if (!DesktopBridgeHelper.Init())
                    return 0;
#endif
#if STARTUP_WATCH_TRACE || DEBUG
                WatchTrace.Record("PlatformPrerequisite");
#endif
            }

            #endregion

            #region CustomAppDomain 自定义应用程序域

#if WINDOWS
#if DEBUG
            // 调试时移动本机库到 native，通常指定了单个 RID(RuntimeIdentifier)
            // 后本机库将位于程序根目录上否则将位于 runtimes 文件夹中
            GlobalDllImportResolver.MoveFiles();
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("CustomAppDomain.MoveFiles");
#endif
#endif
            // 监听当前应用程序域的程序集加载
            AppDomain.CurrentDomain.AssemblyLoad += (_, args)
                => CurrentDomain_AssemblyLoad(args.LoadedAssembly);
            static void CurrentDomain_AssemblyLoad(Assembly loadedAssembly)
            {
#if DEBUG
                if (DebugConsole.WriteAssemblyLoad)
                {
                    DebugConsole.WriteLine($"loadasm: {loadedAssembly}, location: {loadedAssembly.Location}");
                }
#endif
                // 使用 native 文件夹导入解析本机库
                try
                {
                    NativeLibrary.SetDllImportResolver(loadedAssembly, GlobalDllImportResolver.Delegate);
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
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("CustomAppDomain.AssemblyLoad");
#endif

            // 自定义当前应用程序域程序集解析
            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                try
                {
                    var fileNameWithoutEx = args.Name.Split(',',
                        StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    if (!string.IsNullOrEmpty(fileNameWithoutEx))
                    {
                        var isResources = fileNameWithoutEx.EndsWith(".resources");
                        if (isResources)
                        {
                            // System.Composition.Convention.resources
                            // 已包含默认资源，通过反射调用已验证成功
                            // typeof(ConventionBuilder).Assembly.GetType("System.SR").GetProperty("ArgumentOutOfRange_InvalidEnumInSet", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)
                            return null;
                        }
                        // 当前根目录下搜索程序集
                        var filePath = Path.Combine(AppContext.BaseDirectory,
                            $"{fileNameWithoutEx}.dll");
                        if (File.Exists(filePath)) return Assembly.LoadFile(filePath);
                        // 当前根目录下独立框架运行时中搜索程序集
                        filePath = Path.Combine(AppContext.BaseDirectory, "..",
                            "dotnet", "shared", "Microsoft.AspNetCore.App",
                            Environment.Version.ToString(), $"{fileNameWithoutEx}.dll");
                        if (File.Exists(filePath)) return Assembly.LoadFile(filePath);
                        // 当前已安装的运行时
                        filePath = Path.Combine(Environment.GetFolderPath(
                            Environment.SpecialFolder.ProgramFiles), "dotnet",
                            "shared", "Microsoft.AspNetCore.App",
                            Environment.Version.ToString(), $"{fileNameWithoutEx}.dll");
                        if (File.Exists(filePath)) return Assembly.LoadFile(filePath);
                        var dotnet_root = Environment.GetEnvironmentVariable("DOTNET_ROOT");
                        if (!string.IsNullOrWhiteSpace(dotnet_root) && Directory.Exists(dotnet_root))
                        {
                            filePath = Path.Combine(dotnet_root, "shared",
                                "Microsoft.AspNetCore.App", Environment.Version.ToString(),
                                $"{fileNameWithoutEx}.dll");
                            if (File.Exists(filePath)) return Assembly.LoadFile(filePath);
                        }

                    }
                }
                catch
                {

                }
#if DEBUG
                if (DebugConsole.WriteAssemblyResolve)
                {
                    DebugConsole.WriteLine($"asm-resolve fail, name: {args.Name}");
                }
#endif
                return null;
            };
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("CustomAppDomain.AssemblyResolve");
#endif
#endif

            #endregion

            #region RuntimeConfiguration 运行时配置

            // 注册 MemoryPack 某些自定义类型的格式化，如 Cookie, IPAddress, RSAParameters
            HashSet<Type> types = new();
            MemoryPackFormatters.OnRegister = type => types.Add(type);
            MemoryPackFormatterProvider.Register<MemoryPackFormatters>();
            MemoryPackFormatters.OnRegister = null;
            InitNJsonSerializer(types);

            // 添加 .NET Framework 中可用的代码页提供对编码提供程序
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // fix The request was aborted: Could not create SSL/TLS secure channel
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("RuntimeConfiguration");
#endif

            #endregion

            #region InitFileSystem 初始化文件系统

            // 设置 IOPath.AppDataDirectory 与 IOPath.CacheDirectory 的路径
#if MACOS || MACCATALYST || IOS
            MacCatalystFileSystem.InitFileSystem();
#elif LINUX
            LinuxFileSystem.InitFileSystem();
#elif WINDOWS && !WINDOWS_DESKTOP_BRIDGE
            WindowsFileSystem.InitFileSystem();
#elif ANDROID
            FileSystemEssentials.InitFileSystem();
#else
            FileSystem2.InitFileSystem();
#endif
            // 设置仓储层数据库文件存放路径
            Repository.DataBaseDirectory = IOPath.AppDataDirectory;
#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.Record("InitFileSystem");
#endif

            #endregion

            #region InitLog 初始化日志

            string? logDirPath = null;
            IPCSubProcessFileSystem.InitLog(
                ref logDirPath,
#if STARTUP_WATCH_TRACE || DEBUG
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1001 // Commas should be spaced correctly
#pragma warning disable SA1115 // Parameter should follow comma
#pragma warning disable SA1113 // Comma should be on the same line as previous parameter
                watchTrace: true
#pragma warning restore SA1113 // Comma should be on the same line as previous parameter
#pragma warning restore SA1115 // Parameter should follow comma
#pragma warning restore SA1001 // Commas should be spaced correctly
#pragma warning restore IDE0079 // 请删除不必要的忽略
#endif
                );
            IApplication.LogDirPath = logDirPath!;

            #endregion

            #region InitGlobalExceptionHandler 初始化全局异常处理

            GlobalExceptionHandler.Init();

            #endregion

            #region InitSettings 初始化设置项

            if (!IsDesignMode)
            {
#if DBREEZE
                throw new NotImplementedException("TODO");
                SettingsProviderV3.Migrate();
                if (isTrace) StartWatchTrace.Record("SettingsHost.Migrate");
                PreferencesPlatformServiceImplV2.Migrate();
                if (isTrace) StartWatchTrace.Record("Preferences.Migrate");
#endif
            }

            #endregion

#if STARTUP_WATCH_TRACE || DEBUG
            WatchTrace.StopWriteTotal();
#endif
            if (IsDesignMode)
                return 0;

            // https://docs.microsoft.com/zh-cn/archive/msdn-magazine/2019/march/net-parse-the-command-line-with-system-commandline
            var rootCommand = new RootCommand("命令行工具(Command Line Tools/CLT)");
            ConfigureCommands(rootCommand);

            var exitCode = rootCommand.InvokeAsync(args).GetAwaiter().GetResult();

            if (TryGetPlugins(out var plugins))
            {
                foreach (var plugin in plugins)
                {
                    try
                    {
                        await plugin.OnExit();
                    }
                    catch (Exception ex)
                    {
                        GlobalExceptionHandler.Handler(ex,
                            $"{plugin.Name}{nameof(IPlugin.OnExit)}");
                    }
                }
            }

            return exitCode;
        }
        catch (Exception ex)
        {
            GlobalExceptionHandler.Handler(ex, nameof(StartAsync));
            throw;
        }
        finally
        {
            if (!IsDesignMode)
            {
                try
                {
                    await DisposeAppAsync();
                }
                catch (Exception ex)
                {
                    GlobalExceptionHandler.Handler(ex, nameof(DisposeAppAsync));
                }

                try
                {
                    await Ioc.DisposeAsync();
                }
                catch (Exception ex)
                {
                    const string _name = $"{nameof(Ioc)}.{nameof(Ioc.DisposeAsync)}";
                    GlobalExceptionHandler.Handler(ex, _name);
                }

                try
                {
                    NLogManager.Shutdown();
                }
                catch
                {

                }

                switch (ModuleName)
                {
                    case IPlatformService.IPCRoot.moduleName:
#if WINDOWS
                        try
                        {
                            // 退出时执行覆盖更新
                            IAppUpdateService.OverwriteUpgrade();
                        }
                        catch
                        {
                            // 此处写不了日志
                        }
#endif
                        break;
                }
            }
        }
    }
#endif
}