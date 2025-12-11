#if DESIGNER
using Moq;
#endif

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
partial class Program
{
    [STAThread]
    static int Main(string[] args) // Main 函数需要 STA 线程不可更改为 async Task
    {
        #region 尝试修正 AppContext.BaseDirectory

        try
        {
            var appCtxBaseDir = AppContext.BaseDirectory;
            var parentDirInfo = Directory.GetParent(appCtxBaseDir);
            if (parentDirInfo != null)
            {
                if (string.Equals(parentDirInfo.Name, "assemblies", StringComparison.OrdinalIgnoreCase))
                {
                    if (parentDirInfo.FullName ==
                        Directory.GetParent(typeof(Program).Assembly.Location)?.FullName)
                    {
                        parentDirInfo = parentDirInfo.Parent;
                        if (parentDirInfo != null)
                        {
                            AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", parentDirInfo.FullName);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        #endregion

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
        var appCtxBaseDir = Path.GetDirectoryName(Environment.ProcessPath);
        if (string.IsNullOrWhiteSpace(appCtxBaseDir))
            throw new ArgumentOutOfRangeException(nameof(appCtxBaseDir),
                appCtxBaseDir ?? "null", null);
        AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", appCtxBaseDir);
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
#if DESIGNER
        static object? Fallback(Type serviceType, bool required)
        {
            var moq = (Mock)Activator.CreateInstance(typeof(Mock<>).MakeGenericType(serviceType))!;
            return moq.Object;
        }
        Ioc.Fallback = Fallback;
#endif
        try
        {
            Common.UI.Helpers.UIFrameworkHelper.Init(isAvaloniaUI: true);

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
#endif
                                        .UseSkia()
                                        .LogToTrace()
                                        .UseReactiveUI();

                builder.With(new FontManagerOptions
                {
                    DefaultFamilyName = UI.App.DefaultFontFamilyName,
                    FontFallbacks = new[]
                    {
                        new FontFallback
                        {
                            FontFamily = UI.App.DefaultFontFamily,
                        },
                        new FontFallback
                        {
                            FontFamily = FontFamily.Default,
                        },
                    },
                });

                var useGpu = !IApplication.DisableGPU && GeneralSettings.GPU.Value;
#if WINDOWS
                var useWgl = IApplication.UseWgl || GeneralSettings.NativeOpenGL.Value;
#endif
#if WINDOWS
                IEnumerable<Win32RenderingMode> GetRenderingMode()
                {
                    if (useGpu)
                        yield return Win32RenderingMode.AngleEgl;
                    if (useWgl)
                        yield return Win32RenderingMode.Wgl;
                    yield return Win32RenderingMode.Software;
                }
#elif MACOS
                IEnumerable<AvaloniaNativeRenderingMode> GetRenderingMode()
                {
                    if (useGpu)
                        yield return AvaloniaNativeRenderingMode.OpenGl;
                    yield return AvaloniaNativeRenderingMode.Software;
                }
#elif LINUX
                IEnumerable<X11RenderingMode> GetRenderingMode()
                {
                    if (useGpu)
                    {
                        yield return X11RenderingMode.Glx;
                        yield return X11RenderingMode.Egl;
                    }
                    yield return X11RenderingMode.Software;
                }
#endif

#if MACOS
                builder.With(new AvaloniaNativePlatformOptions
                {
                    RenderingMode = GetRenderingMode().ToArray(),
                });
                builder.With(new MacOSPlatformOptions
                {
                    DisableDefaultApplicationMenuItems = true,
                });
#elif LINUX
                builder.With(new X11PlatformOptions
                {
                    RenderingMode = GetRenderingMode().ToArray(),
                    EnableMultiTouch = true,
                    UseDBusMenu = true,
                    EnableIme = true,
                });
#elif WINDOWS
                var options = new Win32PlatformOptions
                {
                    RenderingMode = GetRenderingMode().ToArray(),
                    WinUICompositionBackdropCornerRadius = 8f,
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
        catch (Exception? ex)
        {
#if DESIGNER
            byte counter = 0;
            StringBuilder builder = new();
            do
            {
                if (counter++ == sbyte.MaxValue) break;
                builder.AppendLine(ex.ToString());
                builder.AppendLine();
                ex = ex.InnerException;
            } while (ex != null);
            var errorString = builder.ToString();
            Debug.WriteLine(errorString);
            Console.Error.WriteLine(errorString);
            Console.Error.Flush();
            var bytes = Encoding.UTF8.GetBytes(errorString);
            using (var fileStream = new FileStream(
                Path.Combine(ProjectUtils.GetProjectPath(typeof(Program).Assembly.Location), "designer.log"),
                FileMode.OpenOrCreate,
                FileAccess.Write,
                FileShare.ReadWrite | FileShare.Delete))
            {
                fileStream.Write(bytes);
                fileStream.Flush();
                fileStream.SetLength(fileStream.Position);
            }
#endif
            throw;
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