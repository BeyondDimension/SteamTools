// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPCPlatformService
{
    /// <inheritdoc cref="IScheduledTaskService.SetBootAutoStart(bool, string, bool?)"/>
    [SupportedOSPlatform("windows")]
    void SetBootAutoStart(bool isAutoStart, string name, bool? isPrivilegedProcess = null)
    {
#if !LIB_CLIENT_IPC && WINDOWS
        IScheduledTaskService.Instance?.SetBootAutoStart(isAutoStart, name, isPrivilegedProcess);
#else
#endif
    }
}