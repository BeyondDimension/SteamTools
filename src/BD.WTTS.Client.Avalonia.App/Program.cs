// ReSharper disable once CheckNamespace
namespace BD.WTTS;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
partial class Program
{
    [STAThread]
    static int Main(string[] args) // Main 函数需要 STA 线程不可更改为 async Task
    {
        instance = new(args);
        var exitCode = instance.StartAsync().GetAwaiter().GetResult();
        return exitCode;
    }

    /// <summary>
    /// 自定义 .NET Host 入口点
    /// <para>https://github.com/dotnet/runtime/blob/v7.0.3/docs/design/features/native-hosting.md#loading-and-calling-managed-components</para>
    /// </summary>
    /// <param name="args"></param>
    /// <param name="sizeBytes"></param>
    /// <returns></returns>
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0060 // 删除未使用的参数
    public static int CustomEntryPoint(nint args, int sizeBytes)
#pragma warning restore IDE0060 // 删除未使用的参数
#pragma warning restore IDE0079 // 请删除不必要的忽略
    {
        AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", Environment.CurrentDirectory);
        instance = new()
        {
            IsCustomEntryPoint = true,
        };
        var exitCode = instance.StartAsync().GetAwaiter().GetResult();
        return exitCode;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static AppBuilder BuildAvaloniaApp()
    {
        if (instance == null)
        {
            instance = new()
            {
                IsDesignMode = true,
            };
            instance.StartAsync().GetAwaiter().GetResult();
            instance.RunUIApplication(AppServicesLevel.UI);
        }

#if DEBUG && WINDOWS
        if (!instance.IsDesignMode)
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
            var builder = AppBuilder.Configure(() => UI.App.Instance)
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
                .UseReactiveUI()
                .With(new FontManagerOptions
                {
                    DefaultFamilyName = "Microsoft YaHei UI",
                    FontFallbacks = new[]
                    {
                        new FontFallback
                        {
                            FontFamily = "Microsoft YaHei UI",
                        },
                    },
                });

            var useGpu = !IApplication.DisableGPU && GeneralSettings.UseGPURendering.Value;
#if MACOS
            builder.With(new AvaloniaNativePlatformOptions
            {
                UseGpu = useGpu,
            });
#elif LINUX
            builder.With(new X11PlatformOptions
            {
                UseGpu = useGpu,
                EnableMultiTouch = true,
                UseDBusMenu = true,
                EnableIme = true,
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
        catch (Exception ex)
        {
#if DEBUG
            throw new Exception(
                $"Design.IsDesignMode: {Design.IsDesignMode}, " +
                $"CallingAssemblyName: {Assembly.GetCallingAssembly().GetName().Name}, " +
                $"ExecutingAssemblyName: {Assembly.GetExecutingAssembly()}",
                ex);
#else
            throw;
#endif
        }
    }

    protected override void StartUIApplication()
    {
        if (!Environment.UserInteractive)
            return;

        var builder = BuildAvaloniaApp();
        builder.StartWithClassicDesktopLifetime2(
            Array.Empty<string>(),
            ShutdownMode.OnLastWindowClose);
    }
}