#pragma warning disable IDE1006 // 命名样式

// https://github.com/dotnet/runtime/blob/main/docs/design/features/host-error-codes.md
// https://github.com/dotnet/samples/blob/91355ef22a10ec614a2e8daefd68785066860d57/core/hosting/src/NativeHost/nativehost.cpp

#if NETFRAMEWORK
using System.Configuration;
#endif
using static BD.WTTS.AssemblyInfo;
using static BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static unsafe partial class Program
{
    static Program()
    {
        dotnet_version_major = "9";
        dotnet_version_minor = "0";
        dotnet_version_build = "0";
        dotnet_version = $"{dotnet_version_major}.{dotnet_version_minor}.{dotnet_version_build}";
    }

    //#if NETFRAMEWORK
    public static string dotnet_version_major, dotnet_version_minor, dotnet_version_build, dotnet_version;
    //#else
    //    public const string dotnet_version_major = "7";
    //    public const string dotnet_version_minor = "0";
    //    public const string dotnet_version_build = "11";
    //    public const string dotnet_version = $"{dotnet_version_major}.{dotnet_version_minor}.{dotnet_version_build}";
    //#endif

    const string dotnet_runtime = "Microsoft.NETCore.App";
    const string aspnetcore_runtime = "Microsoft.AspNetCore.App";

    const string dotnet_dll_name = "Steam++";
    const string dotnet_type = "BD.WTTS.Program, Steam++";
    const string dotnet_type_method = "CustomEntryPoint";

    //    /// <summary>
    //    /// 是否依赖 AspNetCore
    //    /// </summary>
    //    /// <returns></returns>
    //    static bool RequireAspNetCore(string baseDirectory)
    //    {
    //        foreach (var moduleName in new[] {
    //            // 依赖 AspNetCore 的模块名
    //            Accelerator,
    //            ArchiSteamFarmPlus,
    //        })
    //        {
    //            var module_path =
    //#if NET35
    //                PathCombine
    //#else
    //                Path.Combine
    //#endif
    //                (baseDirectory, "modules", moduleName);
    //            if (Directory.Exists(module_path) && !PathIsDirectoryEmpty(module_path))
    //            {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }

#if NETFRAMEWORK
    [DllImport("shlwapi.dll", EntryPoint = "PathIsDirectoryEmptyW", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool PathIsDirectoryEmpty([MarshalAs(UnmanagedType.LPTStr)] string pszPath);
#elif NET7_0_OR_GREATER && WINDOWS
    [LibraryImport("shlwapi.dll", EntryPoint = "PathIsDirectoryEmptyW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PathIsDirectoryEmpty([MarshalAs(UnmanagedType.LPTStr)] string pszPath);
#else
#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    static bool PathIsDirectoryEmpty(string pszPath)
    {
        try
        {
            //修复 Files 获取不到文件夹数量 被认为是空文件夹的问题
            return !Directory.EnumerateFileSystemEntries(pszPath).Any();
        }
        catch (DirectoryNotFoundException)
        {
            return true;
        }
    }
#endif

    /// <summary>
    /// 获取当前正在运行的应用的进程架构。
    /// </summary>
    /// <returns></returns>
#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    static Architecture GetProcessArchitecture()
    {
        Architecture processArchitecture;
#if NET471_OR_GREATER || NETCOREAPP
        processArchitecture = RuntimeInformation.ProcessArchitecture;
#else
        try
        {
            processArchitecture = (Architecture)Type.GetType("System.Runtime.InteropServices.RuntimeInformation").GetProperty("ProcessArchitecture", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
        }
        catch
        {
            processArchitecture =
#if NET35
                IntPtr.Size == 8
#else
                Environment.Is64BitProcess
#endif
                ? Architecture.X64 : Architecture.X86;
        }
#endif
        return processArchitecture;
    }

    /// <summary>
    /// 将处理器体系结构转换为显示字符串
    /// </summary>
    /// <param name="architecture"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    static string ToString(Architecture architecture) => architecture switch
    {
        Architecture.X86 => "x86",
        Architecture.X64 => "x64",
        Architecture.Arm => "Arm32",
        Architecture.Arm64 => "Arm64",
#if !NETFRAMEWORK
        Architecture.Armv6 => "Armv6",
#endif
        _ => throw new ArgumentOutOfRangeException(nameof(architecture), architecture, null),
    };

#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    static void OpenCoreByProcess(string url)
    {
        try
        {
            var psi = new ProcessStartInfo(url)
            {
                UseShellExecute = true,
            };
            Process.Start(psi);
        }
        catch (Win32Exception e)
        {
            var text = string.Format(OpenCoreByProcess_Win32Exception_,
                Convert.ToString(e.NativeErrorCode, 16));
            ShowErrMessageBox(text);
        }
    }

    /// <summary>
    /// 下载 .NET 运行时
    /// </summary>
#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    static void DownloadDotNetRuntime()
    {
        string urlFormat1 = $"https://dotnet.microsoft.com/{{0}}/download/dotnet/{dotnet_version_major}.{dotnet_version_minor}";
        var url = string.Format(urlFormat1, GetLang());
        OpenCoreByProcess(url);
    }

    enum DotNetRootType : byte
    {
        BaseDir,
#if NETFRAMEWORK || WINDOWS
        ProgramFiles,
#endif
        EnvironmentVariable,
    }

#if NET40_OR_GREATER
    [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
#endif
    [STAThread]
    static int Main(string[] args)
#if DEBUG
    {
        try
        {
            var exitCode = MainCore(args);
            Console.ReadLine();
            return exitCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.ReadLine();
            return (int)ExitCode.InternalServerError;
        }
    }
#endif

#if DEBUG
    static int MainCore(string[] args)
#endif
    {
        // TODO 合并 32 位与 64 位本机库？检查进程是否是 x64，加载不同的运行库与程序集
        // 允许在 64 位系统上使用 32 位运行
        // 这会使程序包体积变得相当庞大，体积增加 1.x ~ 2 倍？
        // 像 macOS 上的库就是合在一起，但 Mac 上的存储可比 Au 还贵，Windows 上也许还好？

        // 配置垃圾回收器来节省内存，但代价是垃圾回收更频繁，并且暂停时间可能更长。
        // 除了默认值 0 以外，介于 1 至 9（含）的值都有效。 值越高，垃圾回收器越会试图节省内存，进而使堆保持较小。
        // https://learn.microsoft.com/zh-cn/dotnet/core/runtime-config/garbage-collector#conserve-memory
        Environment.SetEnvironmentVariable("DOTNET_GCConserveMemory", "9", EnvironmentVariableTarget.Process);

#if DEBUG
        Console.WriteLine($"Environment.Version: {Environment.Version}");
#endif

#if NETFRAMEWORK
        try
        {
            var dotnet_version_ = ConfigurationManager.AppSettings["d"];
            if (!
#if !NET35
                string.
#endif
                IsNullOrWhiteSpace(dotnet_version_) &&
                dotnet_version_.Length < sbyte.MaxValue)
            {
                var dotnet_version__ = dotnet_version_.Split('.');
                int major, minor, build;
                switch (dotnet_version__.Length)
                {
                    case 1:
                        dotnet_version_major = (int.TryParse(dotnet_version__[0], out major) ? major : 0).ToString();
                        dotnet_version_minor = dotnet_version_build = "0";
                        dotnet_version = $"{dotnet_version_major}.0.0";
                        break;
                    case 2:
                        dotnet_version_major = (int.TryParse(dotnet_version__[0], out major) ? major : 0).ToString();
                        dotnet_version_minor = (int.TryParse(dotnet_version__[1], out minor) ? minor : 0).ToString();
                        dotnet_version_build = "0";
                        dotnet_version = $"{dotnet_version_major}.{dotnet_version_minor}.0";
                        break;
                    case 3:
                        dotnet_version_major = (int.TryParse(dotnet_version__[0], out major) ? major : 0).ToString();
                        dotnet_version_minor = (int.TryParse(dotnet_version__[1], out minor) ? minor : 0).ToString();
                        dotnet_version_build = (int.TryParse(dotnet_version__[2], out build) ? build : 0).ToString();
                        dotnet_version = $"{dotnet_version_major}.{dotnet_version_minor}.{dotnet_version_build}";
                        break;
                }
            }
        }
        catch
        {

        }

        if (dotnet_version == default)
            return (int)ExitCode.Failure_read_dotnet_version;
#endif

        var baseDirectory =
#if NET46_OR_GREATER || NETCOREAPP
                AppContext.BaseDirectory;
#else
                AppDomain.CurrentDomain.BaseDirectory;
#endif

        if (args.Length == 0 && !CompatibilityCheck(baseDirectory))
            return 0;

        //var requireAspNetCore = RequireAspNetCore(baseDirectory);
        const bool requireAspNetCore = true;
        string hostfxr_path, dotnet_runtime_path, aspnetcore_runtime_path, config_path, dotnetlib_path;

        // STEP 0: Search HostFxr
        for (byte i = 0; true; i++)
        {
            var dotnet_root = i switch
            {
                (byte)DotNetRootType.BaseDir => Path.Combine(baseDirectory, "dotnet"), // 优先使用根目录上的运行时
#if NETFRAMEWORK || WINDOWS
                (byte)DotNetRootType.ProgramFiles => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet"), // 查找已安装的运行时
#endif
                (byte)DotNetRootType.EnvironmentVariable => Environment.GetEnvironmentVariable("DOTNET_ROOT") ?? string.Empty, // 检查环境变量中设定的路径
                _ => null,
            };
            if (dotnet_root == null)
            {
                // 此应用程序必须安装 {0} 才能运行，你想现在就下载并安装运行时吗？
                var archStr = ToString(GetProcessArchitecture());
                var _NetRuntime = string.Format(NetRuntimeFormat1, archStr);
                string _Runtime;
                if (requireAspNetCore)
                {
                    var _AspNetCoreRuntime = string.Format(AspNetCoreRuntimeFormat1, archStr);
                    _Runtime = $"{_AspNetCoreRuntime} {And} {_NetRuntime}";
                }
                else
                {
                    _Runtime = _NetRuntime;
                }
                var text = string.Format(FrameworkMissingFailureFormat1, _Runtime);
                var result = ShowErrMessageBox(text, WPFMessageBoxButton.YesNo);
                if (result == WPFMessageBoxResult.Yes)
                {
                    DownloadDotNetRuntime();
                }
                return (int)ExitCode.FrameworkMissingFailure;
            }
            if (
#if !NET35
                string.
#endif
                IsNullOrWhiteSpace(dotnet_root))
                continue;
            try
            {
                if (Directory.Exists(dotnet_root) && !PathIsDirectoryEmpty(dotnet_root))
                {
                    var dir_hostfxr_path =
#if NET35
                        PathCombine
#else
                        Path.Combine
#endif
                        (dotnet_root, "host", "fxr");
                    string usable_dotnet_version = dotnet_version;
                    var dotnet_version_max = Directory.GetDirectories(dir_hostfxr_path, $"{dotnet_version_major}.*").Select(x =>
                      {
                          try
                          {
                              return new Version(Path.GetFileName(x));
                          }
                          catch
                          {
                              return null;
                          }
                      }).Where(x => x != null).Max();
                    if (dotnet_version_max != null)
                    {
                        usable_dotnet_version = dotnet_version_max.ToString();
                    }

                    hostfxr_path =
#if NET35
                        PathCombine
#else
                        Path.Combine
#endif
                        (dir_hostfxr_path, usable_dotnet_version,
#if NETFRAMEWORK || WINDOWS
                        "hostfxr.dll"
#elif MACOS || MACCATALYST
                        "libhostfxr.dylib"
#else
                        "libhostfxr.so"
#endif
                        );

                    dotnet_runtime_path =
#if NET35
                        PathCombine
#else
                        Path.Combine
#endif
                        (dotnet_root, "shared", dotnet_runtime, usable_dotnet_version);

                    aspnetcore_runtime_path = requireAspNetCore ?
#if NET35
                        PathCombine
#else
                        Path.Combine
#endif
                        (dotnet_root, "shared", aspnetcore_runtime, usable_dotnet_version) : null!;
                    if (File.Exists(hostfxr_path) &&
                        Directory.Exists(dotnet_runtime_path) && !PathIsDirectoryEmpty(dotnet_runtime_path) &&
                        (!requireAspNetCore || (Directory.Exists(aspnetcore_runtime_path) && !PathIsDirectoryEmpty(aspnetcore_runtime_path))))
                    {
                        break;
                    }

                }
            }
            catch
            {

            }
        }

        config_path =
#if NET35
            PathCombine
#else
            Path.Combine
#endif
            (baseDirectory, "assemblies", $"{dotnet_dll_name}.runtimeconfig.json");
        dotnetlib_path =
#if NET35
            PathCombine
#else
            Path.Combine
#endif
            (baseDirectory, "assemblies", $"{dotnet_dll_name}.dll");
        if (!File.Exists(config_path) || !File.Exists(dotnetlib_path))
        {
#if DEBUG
            config_path = string.Join(Path.DirectorySeparatorChar.ToString(), new[] { ProjectUtils.ProjPath, "src", "BD.WTTS.Client.Avalonia.App", "bin", "Debug", $"net{dotnet_version_major}.{dotnet_version_minor}-windows10.0.19041", $"{dotnet_dll_name}.runtimeconfig.json" });
            dotnetlib_path = string.Join(Path.DirectorySeparatorChar.ToString(), new[] { ProjectUtils.ProjPath, "src", "BD.WTTS.Client.Avalonia.App", "bin", "Debug", $"net{dotnet_version_major}.{dotnet_version_minor}-windows10.0.19041", $"{dotnet_dll_name}.dll" });
#else
            ShowErrMessageBox($"Loading assembly failed \"{dotnetlib_path}\"");
            return (int)ExitCode.EntryPointFileNotFound;
#endif
        }

        var fvi = FileVersionInfo.GetVersionInfo(dotnetlib_path);
        if (fvi.Comments != Description)
            return (int)ExitCode.Failure_fvi_Description;
        if (fvi.CompanyName != Company)
            return (int)ExitCode.Failure_fvi_Company;
        if (fvi.LegalCopyright != Copyright)
            return (int)ExitCode.Failure_fvi_Copyright;
        if (fvi.LegalTrademarks != Trademark)
            return (int)ExitCode.Failure_fvi_Trademark;

        delegate* unmanaged[Cdecl]<nint, nint, out nint, int> init_fptr;
        delegate* unmanaged[Cdecl]<nint, hostfxr_delegate_type, out delegate* unmanaged[Cdecl]<nint, nint, nint, nint, nint, out delegate* unmanaged[Cdecl]<nint, int, int>, int>, int> get_delegate_fptr;
        delegate* unmanaged[Cdecl]<nint, int> close_fptr;

        // STEP 1: Load HostFxr and get exported hosting functions

        #region load_hostfxr Using the nethost library, discover the location of hostfxr and get exports

        // Pre-allocate a large buffer for the path to hostfxr
        // Load hostfxr and get desired exports
        var lib = NativeLibrary.Load(hostfxr_path);
        init_fptr = (delegate* unmanaged[Cdecl]<nint, nint, out nint, int>)
            NativeLibrary.GetExport(lib, "hostfxr_initialize_for_runtime_config");
        get_delegate_fptr = (delegate* unmanaged[Cdecl]<nint, hostfxr_delegate_type, out delegate* unmanaged[Cdecl]<nint, nint, nint, nint, nint, out delegate* unmanaged[Cdecl]<nint, int, int>, int>, int>)
            NativeLibrary.GetExport(lib, "hostfxr_get_runtime_delegate");
        close_fptr = (delegate* unmanaged[Cdecl]<nint, int>)
            NativeLibrary.GetExport(lib, "hostfxr_close");

        #endregion

        if (!(init_fptr != default && get_delegate_fptr != default && close_fptr != default))
        {
            Debug.WriteLine("Failure: load_hostfxr()");
            return (int)ExitCode.Failure_load_hostfxr;
        }

        // STEP 2: Initialize and start the .NET Core runtime

        #region get_dotnet_load_assembly Load and initialize .NET Core and get desired function pointer for scenario

        delegate* unmanaged[Cdecl]<nint, nint, nint, nint, nint,
            out delegate* unmanaged[Cdecl]<nint, int, int>, int>
            load_assembly_and_get_function_pointer = default;

        // Load .NET Core
        var config_path_ = Marshal.StringToHGlobalUni(config_path);
        int rc = default;
        nint cxt = default;
        try
        {
            rc = init_fptr(config_path_, default, out cxt);
        }
        finally
        {
            Marshal.FreeHGlobal(config_path_);
        }
        if (rc != 0 || cxt == default)
        {
#if NET35
            DebugWriteLine
#else
            Debug.WriteLine
#endif
                ("Init failed: 0x{0}", new object[] { Convert.ToString(rc, 16), });
            close_fptr(cxt);
        }
        else
        {
            // Get the load assembly function pointer
            rc = get_delegate_fptr(
                cxt,
                hostfxr_delegate_type.hdt_load_assembly_and_get_function_pointer,
                out load_assembly_and_get_function_pointer);
            if (rc != 0 || load_assembly_and_get_function_pointer == default)
            {
#if NET35
                DebugWriteLine
#else
                Debug.WriteLine
#endif
                        ("Get delegate failed: 0x{0}", new object[] { Convert.ToString(rc, 16), });
            }
            close_fptr(cxt);
        }

        #endregion

        if (load_assembly_and_get_function_pointer == default)
        {
            Debug.WriteLine("Failure: get_dotnet_load_assembly()");
            return (int)ExitCode.Failure_get_dotnet_load_assembly;
        }

        // STEP 3: Load managed assembly and get function pointer to a managed method
        var dotnetlib_path_ = Marshal.StringToHGlobalUni(dotnetlib_path);
        var dotnet_type_ = Marshal.StringToHGlobalUni(dotnet_type);
        var dotnet_type_method_ = Marshal.StringToHGlobalUni(dotnet_type_method);
        delegate* unmanaged[Cdecl]<nint, int, int> main = default;
        try
        {
            rc = load_assembly_and_get_function_pointer(
                dotnetlib_path_,
                dotnet_type_,
                dotnet_type_method_,
                default,
                default,
                out main);
        }
        finally
        {
            Marshal.FreeHGlobal(dotnetlib_path_);
            Marshal.FreeHGlobal(dotnet_type_);
            Marshal.FreeHGlobal(dotnet_type_method_);
        }
        if (rc != 0 || main == default)
        {
            Debug.WriteLine("Failure: load_assembly_and_get_function_pointer()");
            return (int)ExitCode.Failure_load_assembly_and_get_function_pointer;
        }

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        var exitCode = main(default, default);
        return exitCode;
    }

    struct hostfxr_initialize_parameters
    {
        public int size;
        public string host_path;
        public string dotnet_root;
    }

    enum hostfxr_delegate_type
    {
        hdt_com_activation,
        hdt_load_in_memory_assembly,
        hdt_winrt_activation,
        hdt_com_register,
        hdt_com_unregister,
        hdt_load_assembly_and_get_function_pointer,
        hdt_get_function_pointer,
    }

    enum ExitCode
    {
        InternalServerError = 5500,

        Failure_load_hostfxr = 5701,
        Failure_get_dotnet_load_assembly,
        Failure_load_assembly_and_get_function_pointer,
        FrameworkMissingFailure,
        EntryPointFileNotFound,

        Failure_read_dotnet_version,

        Failure_fvi_Description,
        Failure_fvi_Company,
        Failure_fvi_Copyright,
        Failure_fvi_Trademark,
    }

#if NETFRAMEWORK
#if !NET471_OR_GREATER
    enum Architecture
    {
        X86 = 0,
        X64 = 1,
        Arm = 2,
        Arm64 = 3,
        //Wasm = 4,
        //S390x = 5,
        //LoongArch64 = 6, // SkiaSharp incompatibility.
        Armv6 = 7,
        //Ppc64le = 8,
    }
#endif
#endif
}
