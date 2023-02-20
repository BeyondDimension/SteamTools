// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// 为 <see cref="Process"/> 类型提供扩展方法
/// </summary>
public static class ProcessExtensions
{
    /// <inheritdoc cref="IPlatformService.GetCommandLineArgs(Process)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetCommandLineArgs(this Process process)
    {
        try
        {
            return IPlatformService.Instance.GetCommandLineArgs(process);
        }
        catch
        {
            // 进程已退出。
            return string.Empty;
        }
    }
}