#if NET35
// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Program
{
    //// https://www.codeproject.com/questions/641314/check-64-or-32-bit-in-net-3-5

    //delegate bool IsWow64ProcessDelegate([In] IntPtr handle, [Out] out bool isWow64Process);

    //public static bool Is64BitOperatingSystem
    //{
    //    get
    //    {
    //        if (IntPtr.Size == 8 || (IntPtr.Size == 4 && Is32BitProcessOn64BitProcessor()))
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //}

    //static IsWow64ProcessDelegate? GetIsWow64ProcessDelegate()
    //{
    //    IntPtr handle = Program.LoadLibrary("kernel32");

    //    if (handle != IntPtr.Zero)
    //    {
    //        IntPtr fnPtr = Program.GetProcAddress(handle, "IsWow64Process");

    //        if (fnPtr != IntPtr.Zero)
    //        {
    //            return (IsWow64ProcessDelegate)Marshal.GetDelegateForFunctionPointer(fnPtr, typeof(IsWow64ProcessDelegate));
    //        }
    //    }

    //    return null;
    //}

    //static bool Is32BitProcessOn64BitProcessor()
    //{
    //    var fnDelegate = GetIsWow64ProcessDelegate();

    //    if (fnDelegate == null)
    //    {
    //        return false;
    //    }

    //    bool retVal = fnDelegate.Invoke(Process.GetCurrentProcess().Handle, out var isWow64);

    //    if (retVal == false)
    //    {
    //        return false;
    //    }

    //    return isWow64;
    //}

#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsNullOrWhiteSpace(string value)
    {
        if (value == null)
        {
            return true;
        }

        for (int i = 0; i < value.Length; i++)
        {
            if (!char.IsWhiteSpace(value[i]))
            {
                return false;
            }
        }

        return true;
    }

#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    [Conditional("DEBUG")]
    public static void DebugWriteLine(string format, params object[] args)
    {
        Debug.WriteLine(string.Format(format, args));
    }

    public const char DirectorySeparatorChar = '\\';

#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string PathCombine(string path1, string path2, string path3)
    {
        return $"{path1}{(path1[path1.Length - 1] == DirectorySeparatorChar ? "" : DirectorySeparatorChar)}{path2}{DirectorySeparatorChar}{path3}";
    }

#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string PathCombine(string path1, string path2, string path3, string path4)
    {
        return $"{path1}{(path1[path1.Length - 1] == DirectorySeparatorChar ? "" : DirectorySeparatorChar)}{path2}{DirectorySeparatorChar}{path3}{DirectorySeparatorChar}{path4}";
    }

#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static string PathCombine(string path1, string path2, string path3, string path4, string path5)
    {
        return $"{path1}{(path1[path1.Length - 1] == DirectorySeparatorChar ? "" : DirectorySeparatorChar)}{path2}{DirectorySeparatorChar}{path3}{DirectorySeparatorChar}{path4}{DirectorySeparatorChar}{path5}";
    }

}
#endif