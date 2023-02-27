#pragma warning disable IDE1006 // 命名样式

// https://github.com/dotnet/runtime/blob/main/docs/design/features/host-error-codes.md
// https://github.com/dotnet/samples/blob/91355ef22a10ec614a2e8daefd68785066860d57/core/hosting/src/NativeHost/nativehost.cpp

// 合并32位与64位本机库？检查进程是否是x64，加载不同的运行库与程序集

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class Program
{
    const string dotnet_type = "BD.WTTS.Program, BD.WTTS.Client.Avalonia.App";
    const string dotnet_type_method = "CustomEntryPoint";

    static int Main()
    {
        if (!CompatibilityCheck()) return 0;

        var hostfxr_path = @"C:\Program Files\dotnet\host\fxr\7.0.3\hostfxr.dll";
        var config_path = @"TODO\Steam++.runtimeconfig.json";
        var dotnetlib_path = @"TODO\bin\Debug\Steam++.dll";

        hostfxr_initialize_for_runtime_config_fn init_fptr;
        hostfxr_get_runtime_delegate_fn get_delegate_fptr;
        hostfxr_close_fn close_fptr;

        // STEP 1: Load HostFxr and get exported hosting functions

        #region load_hostfxr Using the nethost library, discover the location of hostfxr and get exports

        // Pre-allocate a large buffer for the path to hostfxr
        // Load hostfxr and get desired exports
        var lib = LoadLibrary(hostfxr_path);
#if NET451_OR_GREATER
        init_fptr = Marshal.GetDelegateForFunctionPointer<hostfxr_initialize_for_runtime_config_fn>(GetProcAddress(lib, "hostfxr_initialize_for_runtime_config"));
        get_delegate_fptr = Marshal.GetDelegateForFunctionPointer<hostfxr_get_runtime_delegate_fn>(GetProcAddress(lib, "hostfxr_get_runtime_delegate"));
        close_fptr = Marshal.GetDelegateForFunctionPointer<hostfxr_close_fn>(GetProcAddress(lib, "hostfxr_close"));
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
            Debug.WriteLine("Init failed: 0x{0}", new object[] { Convert.ToString(rc, 16), });
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
                Debug.WriteLine("Get delegate failed: 0x{0}", new object[] { Convert.ToString(rc, 16), });
            }
            close_fptr(cxt);
#if NET451_OR_GREATER
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

#if NET451_OR_GREATER
        var main = Marshal.GetDelegateForFunctionPointer<component_entry_point_fn>(main_);
#else
        var main = (component_entry_point_fn)Marshal.GetDelegateForFunctionPointer(main_, typeof(component_entry_point_fn));
#endif

        var exitCode = main();
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

    delegate int component_entry_point_fn();

    enum ExitCode
    {
        Failure_load_hostfxr = 5701,
        Failure_get_dotnet_load_assembly,
        Failure_load_assembly_and_get_function_pointer,
    }

#if NETFRAMEWORK

    /// <summary>
    /// https://www.pinvoke.net/default.aspx/kernel32.LoadLibrary
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.interopservices.nativelibrary.load</para>
    /// </summary>
    /// <param name="lpFileName"></param>
    /// <returns></returns>
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern nint LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

    /// <summary>
    /// https://www.pinvoke.net/default.aspx/kernel32.GetProcAddress
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.interopservices.nativelibrary.getexport</para>
    /// </summary>
    /// <param name="hModule"></param>
    /// <param name="procName"></param>
    /// <returns></returns>
    [DllImport("kernel32", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    static extern nint GetProcAddress(nint hModule, string procName);

    ///// <summary>
    ///// https://www.pinvoke.net/default.aspx/kernel32/FreeLibrary.html
    ///// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.interopservices.nativelibrary.free</para>
    ///// </summary>
    ///// <param name="hModule"></param>
    ///// <returns></returns>
    //[DllImport("kernel32.dll", SetLastError = true)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //static extern bool FreeLibrary(nint hModule);

#endif
}
