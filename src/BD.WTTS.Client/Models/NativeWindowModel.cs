#if WINDOWS7_0_OR_GREATER
namespace BD.WTTS.Models;

/// <summary>
/// 原生窗口模型
/// </summary>
/// <param name="Handle">窗口句柄</param>
/// <param name="Title">窗口标题</param>
/// <param name="ClassName"></param>
/// <param name="Process">进程</param>
/// <param name="Path">路径</param>
/// <param name="Name">名称</param>
[SupportedOSPlatform("Windows")]
public record class NativeWindowModel(
    IntPtr Handle,
    string? Title,
    string? ClassName,
    Process? Process,
    string? Path,
    string? Name);

[SupportedOSPlatform("Windows")]
public static class HandleWindowExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHasProcessExits(this NativeWindowModel window)
    {
        if (window?.Process?.HasExited == false && window.Name != Process.GetCurrentProcess().ProcessName)
        {
            return false;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Kill(this NativeWindowModel window)
    {
        if (!window.IsHasProcessExits())
        {
            window.Process?.Kill();
        }
    }
}
#endif