using System.Diagnostics;
using System.Runtime.Versioning;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System;

public static class ProcessExtensions
{
    public static ProcessModule? TryGetMainModule(this Process process)
    {
        try
        {
            return process.MainModule;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 立即停止关联的进程，并停止其子/后代进程。
    /// </summary>
    /// <param name="process"></param>
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void KillEntireProcessTree(this Process process)
    {
#if NETCOREAPP3_0_OR_GREATER
        process.Kill(entireProcessTree: true);
#else
        process.Kill();
#endif
    }

    /// <summary>
    /// 尝试 Process 组件在指定的毫秒数内等待关联进程退出。
    /// </summary>
    /// <param name="process"></param>
    /// <param name="milliseconds"></param>
    /// <returns></returns>
    public static bool TryWaitForExit(this Process process, int milliseconds = 9000)
    {
        try
        {
            return process.WaitForExit(milliseconds);
        }
        catch
        {

        }
        return false;
    }
}