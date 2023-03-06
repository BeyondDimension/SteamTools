#pragma warning disable IDE1006 // 命名样式

// https://github.com/dotnet/runtime/blob/main/docs/design/features/host-error-codes.md
// https://github.com/dotnet/samples/blob/91355ef22a10ec614a2e8daefd68785066860d57/core/hosting/src/NativeHost/nativehost.cpp

using static BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class Program
{
    public const string dotnet_version_major = "7";
    public const string dotnet_version_minor = "0";
    public const string dotnet_version_build = "3";
    public const string dotnet_version = $"{dotnet_version_major}.{dotnet_version_minor}.{dotnet_version_build}";
    const string dotnet_runtime = "Microsoft.NETCore.App";
    const string aspnetcore_runtime = "Microsoft.AspNetCore.App";

    const string dotnet_dll_name = "Steam++";
    const string dotnet_type = "BD.WTTS.Program, Steam++";
    const string dotnet_type_method = "CustomEntryPoint";

#if NET7_0_OR_GREATER
    [LibraryImport("shlwapi.dll", EntryPoint = "PathIsDirectoryEmptyW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PathIsDirectoryEmpty_([MarshalAs(UnmanagedType.LPTStr)] string pszPath);

#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    static bool PathIsDirectoryEmpty(string pszPath)
    {
        if (OperatingSystem.IsWindows())
            return PathIsDirectoryEmpty_(pszPath);
        return Directory.EnumerateFiles(pszPath).Any();
    }
#elif NETFRAMEWORK
    [DllImport("shlwapi.dll", EntryPoint = "PathIsDirectoryEmptyW", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool PathIsDirectoryEmpty([MarshalAs(UnmanagedType.LPTStr)] string pszPath);
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
        Architecture.Arm64 => "Arm64",
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
        const string urlFormat1 = $"https://dotnet.microsoft.com/{{0}}/download/dotnet/{dotnet_version_major}.{dotnet_version_minor}";
        var url = string.Format(urlFormat1, GetLang());
        OpenCoreByProcess(url);
    }

    [STAThread]
    static int Main()
    {
        // TODO 合并32位与64位本机库？检查进程是否是x64，加载不同的运行库与程序集

#if DEBUG
        Console.WriteLine($"Environment.Version: {Environment.Version}");
#endif

        if (!CompatibilityCheck()) return 0;

        var baseDirectory =
#if NET46_OR_GREATER || NETCOREAPP
                AppContext.BaseDirectory;
#else
                AppDomain.CurrentDomain.BaseDirectory;
#endif
        string hostfxr_path, dotnet_runtime_path, aspnetcore_runtime_path, config_path, dotnetlib_path;

        // STEP 0: Search HostFxr
        for (int i = 0; true; i++)
        {
            var dotnet_root = i switch
            {
                0 => Path.Combine(baseDirectory, "dotnet"), // 优先使用根目录上的运行时
                1 => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet"), // 查找已安装的运行时
                2 => Environment.GetEnvironmentVariable("DOTNET_ROOT") ?? string.Empty, // 检查环境变量中设定的路径
                _ => null,
            };
            if (dotnet_root == null)
            {
                // 此应用程序必须安装 {0} 才能运行，你想现在就下载并安装运行时吗？
                var archStr = ToString(GetProcessArchitecture());
                var _AspNetCoreRuntime = string.Format(AspNetCoreRuntimeFormat1, archStr);
                var _NetRuntime = string.Format(NetRuntimeFormat1, archStr);
                var _Runtime = $"{_AspNetCoreRuntime} {And} {_NetRuntime}";
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
                    hostfxr_path =
#if NET35
                        PathCombine
#else
                        Path.Combine
#endif
                        (dotnet_root, "host", "fxr", dotnet_version, "hostfxr.dll");
                    dotnet_runtime_path =
#if NET35
                        PathCombine
#else
                        Path.Combine
#endif
                        (dotnet_root, "shared", dotnet_runtime, dotnet_version);
                    aspnetcore_runtime_path =
#if NET35
                        PathCombine
#else
                        Path.Combine
#endif
                        (dotnet_root, "shared", aspnetcore_runtime, dotnet_version);
                    if (File.Exists(hostfxr_path) &&
                        Directory.Exists(dotnet_runtime_path) && !PathIsDirectoryEmpty(dotnet_runtime_path) &&
                        Directory.Exists(aspnetcore_runtime_path) && !PathIsDirectoryEmpty(aspnetcore_runtime_path))
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
            config_path = string.Join(Path.DirectorySeparatorChar.ToString(), new[] { Utils.ProjPath, "src", "BD.WTTS.Client.Avalonia.App", "bin", "Debug", $"net{dotnet_version_major}.{dotnet_version_minor}-windows10.0.19041.0", "win-x64", $"{dotnet_dll_name}.runtimeconfig.json" });
            dotnetlib_path = string.Join(Path.DirectorySeparatorChar.ToString(), new[] { Utils.ProjPath, "src", "BD.WTTS.Client.Avalonia.App", "bin", "Debug", $"net{dotnet_version_major}.{dotnet_version_minor}-windows10.0.19041.0", "win-x64", $"{dotnet_dll_name}.dll" });
#else
            ShowErrMessageBox($"Loading assembly failed \"{dotnetlib_path}\"");
            return (int)ExitCode.EntryPointFileNotFound;
#endif
        }

        hostfxr_initialize_for_runtime_config_fn init_fptr;
        hostfxr_get_runtime_delegate_fn get_delegate_fptr;
        hostfxr_close_fn close_fptr;

        // STEP 1: Load HostFxr and get exported hosting functions

        #region load_hostfxr Using the nethost library, discover the location of hostfxr and get exports
        // Pre-allocate a large buffer for the path to hostfxr
        // Load hostfxr and get desired exports
#if NETCOREAPP
        var lib = NativeLibrary.Load(hostfxr_path);
#else
        var lib = LoadLibrary(hostfxr_path);
#endif
#if NET451_OR_GREATER
        init_fptr = Marshal.GetDelegateForFunctionPointer<hostfxr_initialize_for_runtime_config_fn>(GetProcAddress(lib, "hostfxr_initialize_for_runtime_config"));
        get_delegate_fptr = Marshal.GetDelegateForFunctionPointer<hostfxr_get_runtime_delegate_fn>(GetProcAddress(lib, "hostfxr_get_runtime_delegate"));
        close_fptr = Marshal.GetDelegateForFunctionPointer<hostfxr_close_fn>(GetProcAddress(lib, "hostfxr_close"));
#elif NETCOREAPP
        init_fptr = Marshal.GetDelegateForFunctionPointer<hostfxr_initialize_for_runtime_config_fn>(NativeLibrary.GetExport(lib, "hostfxr_initialize_for_runtime_config"));
        get_delegate_fptr = Marshal.GetDelegateForFunctionPointer<hostfxr_get_runtime_delegate_fn>(NativeLibrary.GetExport(lib, "hostfxr_get_runtime_delegate"));
        close_fptr = Marshal.GetDelegateForFunctionPointer<hostfxr_close_fn>(NativeLibrary.GetExport(lib, "hostfxr_close"));
#else
        init_fptr = (hostfxr_initialize_for_runtime_config_fn)Marshal.GetDelegateForFunctionPointer(GetProcAddress(lib, "hostfxr_initialize_for_runtime_config"), typeof(hostfxr_initialize_for_runtime_config_fn));
        get_delegate_fptr = (hostfxr_get_runtime_delegate_fn)Marshal.GetDelegateForFunctionPointer(GetProcAddress(lib, "hostfxr_get_runtime_delegate"), typeof(hostfxr_get_runtime_delegate_fn));
        close_fptr = (hostfxr_close_fn)Marshal.GetDelegateForFunctionPointer(GetProcAddress(lib, "hostfxr_close"), typeof(hostfxr_close_fn));
#endif

        #endregion

        if (!(init_fptr != default && get_delegate_fptr != default && close_fptr != default))
        {
            Debug.WriteLine("Failure: load_hostfxr()");
            return (int)ExitCode.Failure_load_hostfxr;
        }

        // STEP 2: Initialize and start the .NET Core runtime

        #region get_dotnet_load_assembly Load and initialize .NET Core and get desired function pointer for scenario

        load_assembly_and_get_function_pointer_fn? load_assembly_and_get_function_pointer = default;

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
                out var load_assembly_and_get_function_pointer_);
            if (rc != 0 || load_assembly_and_get_function_pointer_ == default)
            {
#if NET35
                DebugWriteLine
#else
                Debug.WriteLine
#endif
                        ("Get delegate failed: 0x{0}", new object[] { Convert.ToString(rc, 16), });
            }
            close_fptr(cxt);
#if NET451_OR_GREATER || NETCOREAPP
            load_assembly_and_get_function_pointer = Marshal.GetDelegateForFunctionPointer<load_assembly_and_get_function_pointer_fn>(load_assembly_and_get_function_pointer_);
#else
            load_assembly_and_get_function_pointer = (load_assembly_and_get_function_pointer_fn)Marshal.GetDelegateForFunctionPointer(load_assembly_and_get_function_pointer_, typeof(load_assembly_and_get_function_pointer_fn));
#endif
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
        nint main_ = default;
        try
        {
            rc = load_assembly_and_get_function_pointer(
                dotnetlib_path_,
                dotnet_type_,
                dotnet_type_method_,
                default,
                default,
                out main_);
        }
        finally
        {
            Marshal.FreeHGlobal(dotnetlib_path_);
            Marshal.FreeHGlobal(dotnet_type_);
            Marshal.FreeHGlobal(dotnet_type_method_);
        }
        if (rc != 0 || main_ == default)
        {
            Debug.WriteLine("Failure: load_assembly_and_get_function_pointer()");
            return (int)ExitCode.Failure_load_assembly_and_get_function_pointer;
        }

#if NET451_OR_GREATER || NETCOREAPP
        var main = Marshal.GetDelegateForFunctionPointer<component_entry_point_fn>(main_);
#else
        var main = (component_entry_point_fn)Marshal.GetDelegateForFunctionPointer(main_, typeof(component_entry_point_fn));
#endif

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

    delegate int hostfxr_initialize_for_runtime_config_fn(nint runtime_config_path, nint parameters, out nint host_context_handle);

    delegate int hostfxr_get_runtime_delegate_fn(nint host_context_handle, hostfxr_delegate_type type, out nint @delegate);

    delegate int hostfxr_close_fn(nint host_context_handle);

    delegate int load_assembly_and_get_function_pointer_fn(nint assembly_path, nint type_name, nint method_name, nint delegate_type_name, nint reserved, out nint @delegate);

    delegate int component_entry_point_fn(nint arg, int arg_size_in_bytes);

    enum ExitCode
    {
        Failure_load_hostfxr = 5701,
        Failure_get_dotnet_load_assembly,
        Failure_load_assembly_and_get_function_pointer,
        FrameworkMissingFailure,
        EntryPointFileNotFound,
    }

#if NETFRAMEWORK

    /// <summary>
    /// https://www.pinvoke.net/default.aspx/kernel32.LoadLibrary
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.interopservices.nativelibrary.load</para>
    /// </summary>
    /// <param name="lpFileName"></param>
    /// <returns></returns>
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern nint LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

    ///// <summary>
    ///// https://www.pinvoke.net/default.aspx/kernel32/LoadLibraryEx
    ///// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.interopservices.nativelibrary.load</para>
    ///// </summary>
    ///// <param name="lpFileName"></param>
    ///// <param name="hReservedNull"></param>
    ///// <param name="dwFlags"></param>
    ///// <returns></returns>
    //[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    //static extern nint LoadLibraryEx([MarshalAs(UnmanagedType.LPWStr)] string lpFileName, nint hReservedNull = default, uint dwFlags = 0);

    /// <summary>
    /// https://www.pinvoke.net/default.aspx/kernel32.GetProcAddress
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.interopservices.nativelibrary.getexport</para>
    /// </summary>
    /// <param name="hModule"></param>
    /// <param name="procName"></param>
    /// <returns></returns>
    [DllImport("kernel32", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    internal static extern nint GetProcAddress(nint hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

    ///// <summary>
    ///// https://www.pinvoke.net/default.aspx/kernel32/FreeLibrary.html
    ///// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.interopservices.nativelibrary.free</para>
    ///// </summary>
    ///// <param name="hModule"></param>
    ///// <returns></returns>
    //[DllImport("kernel32.dll", SetLastError = true)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //static extern bool FreeLibrary(nint hModule);

#if !NET471_OR_GREATER
    enum Architecture
    {
        X86 = 0,
        X64 = 1,
        Arm64 = 3,
    }
#endif

#endif
}
