// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPCPlatformService
{
    [SupportedOSPlatform("windows")]
    void SetBootAutoStart(bool isAutoStart, string name)
    {
#if !LIB_CLIENT_IPC && WINDOWS
        IScheduledTaskService.Instance?.SetBootAutoStart(isAutoStart, name);
#else
#endif
    }
}