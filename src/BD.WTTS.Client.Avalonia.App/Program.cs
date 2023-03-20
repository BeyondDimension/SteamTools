using static BD.WTTS.CommandLineHost;

namespace BD.WTTS;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
static partial class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    internal static int Main(string[] args)
    {
        var baseDirectory = AppContext.BaseDirectory;
#if WINDOWS
        if (!IsCustomEntryPoint && !CompatibilityCheck(baseDirectory))
            return 0;
#endif

        // 注册 MemoryPack 某些自定义类型的格式化，如 Cookie, IPAddress, RSAParameters
        MemoryPackFormatterProvider.Register<MemoryPackFormatters>();

        // 添加 .NET Framework 中可用的代码页提供对编码提供程序
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // fix The request was aborted: Could not create SSL/TLS secure channel
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

#if WINDOWS
        // 在 Windows 上还原 .NET Framework 中网络请求跟随系统网络代理变化而动态切换代理行为
        HttpClient.DefaultProxy = DynamicHttpWindowsProxy.Instance;
#endif
#if WINDOWS_DESKTOP_BRIDGE
        if (!DesktopBridgeHelper.Init()) return 0;
        InitWithDesktopBridge(ref args);
#elif IOS || MACOS || MACCATALYST
        UIApplication.Main(args, null, typeof(AppDelegate));
#endif

        var host = Host.Instance;
        host.IsMainProcess = args.Length == 0;
        host.IsCLTProcess = !host.IsMainProcess && args.FirstOrDefault() == "-clt";

        OnCreateAppExecuting();

        try
        {
            string[] args_clt;
            if (host.IsCLTProcess) // 命令行模式
            {
                args_clt = args.Skip(1).ToArray();
                if (args_clt.Length == 1 && args_clt[0].Equals(command_main, StringComparison.OrdinalIgnoreCase))
                    return default;
            }
            else
            {
                args_clt = new[] { command_main };
            }
            return host.Run(args_clt);
        }
        catch (Exception ex)
        {
            GlobalExceptionHelpers.Handler(ex, nameof(Main));
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            PlatformApp?.Dispose();
            Ioc.Dispose();
            LogManager.Shutdown();
        }
    }

    public static bool IsCustomEntryPoint { get; internal set; }

    /// <summary>
    /// 自定义 .NET Host 入口点
    /// <para>https://github.com/dotnet/runtime/blob/v7.0.3/docs/design/features/native-hosting.md#loading-and-calling-managed-components</para>
    /// </summary>
    /// <param name="args"></param>
    /// <param name="sizeBytes"></param>
    /// <returns></returns>
#pragma warning disable IDE0060 // 删除未使用的参数
    public static int CustomEntryPoint(nint args, int sizeBytes)
#pragma warning restore IDE0060 // 删除未使用的参数
    {
        IsCustomEntryPoint = true;
        AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", Environment.CurrentDirectory);
        return Main(Environment.GetCommandLineArgs().Skip(1).ToArray());
    }

#if WINDOWS_DESKTOP_BRIDGE
    static void InitWithDesktopBridge(ref string[] args)
    {
        DesktopBridgeHelper.OnActivated(ref args);
    }
#endif

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        // 设计器模式不会执行 Main 函数所以以此区分来初始化文件系统
        if (Design.IsDesignMode)
        {
            OnCreateAppExecuting();
        }

        var builder = AppBuilder.Configure(() => Host.Instance.App)
#if WINDOWS
            .UseWin32()
#elif MACCATALYST || MACOS
            .UseAvaloniaNative()
#elif LINUX
            .UseX11()
#else
#endif
            .UseSkia()
            .LogToTrace()
            .UseReactiveUI();

        var useGpu = !IApplication.DisableGPU && GeneralSettings.UseGPURendering.Value;
#if MACOS
        builder.With(new AvaloniaNativePlatformOptions
        {
            UseGpu = useGpu,
            EnableMultiTouch = true,
            UseDBusMenu = true,
            EnableIme = true
        });
#elif LINUX
        builder.With(new X11PlatformOptions
        {
            UseGpu = useGpu,
            EnableMultiTouch = true,
            UseDBusMenu = true,
            EnableIme = true
        });
#elif WINDOWS
        var useWgl = IApplication.UseWgl || GeneralSettings.UseWgl.Value;
        var options = new Win32PlatformOptions
        {
            UseWgl = useWgl,
            AllowEglInitialization = useGpu,
            UseWindowsUIComposition = true,
            CompositionBackdropCornerRadius = 8f,
        };

        builder.With(options);

        var skiaOptions = new SkiaOptions
        {
            MaxGpuResourceSizeBytes = 1024000000,
        };

        builder.With(skiaOptions);
#else
        throw new PlatformNotSupportedException("Avalonia.Desktop package was referenced on non-desktop platform or it isn't supported");
#endif
        return builder;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void StartAvaloniaApp(string[] args,
        ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)
    {
        if (!Environment.UserInteractive) return;
        var builder = BuildAvaloniaApp();
        builder.StartWithClassicDesktopLifetime2(args, shutdownMode);
    }
}
