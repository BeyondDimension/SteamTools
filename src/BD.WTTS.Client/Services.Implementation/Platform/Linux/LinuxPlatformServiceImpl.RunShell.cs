#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    ValueTask IPlatformService.RunShellAsync(string script, bool requiredAdministrator)
        => UnixHelper.RunShellAsync(script, requiredAdministrator);
}
#endif