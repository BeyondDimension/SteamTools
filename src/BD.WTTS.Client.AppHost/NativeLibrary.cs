#if NETFRAMEWORK

namespace System.Runtime.InteropServices;

public static class NativeLibrary
{
    /// <summary>
    /// https://www.pinvoke.net/default.aspx/kernel32.LoadLibrary
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.interopservices.nativelibrary.load</para>
    /// </summary>
    /// <param name="lpFileName"></param>
    /// <returns></returns>
    [DllImport("kernel32", EntryPoint = "LoadLibraryW", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern nint Load([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

    /// <summary>
    /// https://www.pinvoke.net/default.aspx/kernel32.GetProcAddress
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.interopservices.nativelibrary.getexport</para>
    /// </summary>
    /// <param name="hModule"></param>
    /// <param name="procName"></param>
    /// <returns></returns>
    [DllImport("kernel32", EntryPoint = "GetProcAddress", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    public static extern nint GetExport(nint hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

    ///// <summary>
    ///// https://www.pinvoke.net/default.aspx/kernel32/FreeLibrary.html
    ///// <para>https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.interopservices.nativelibrary.free</para>
    ///// </summary>
    ///// <param name="hModule"></param>
    ///// <returns></returns>
    //[DllImport("kernel32.dll", SetLastError = true)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //public static extern bool FreeLibrary(nint hModule);
}

#endif