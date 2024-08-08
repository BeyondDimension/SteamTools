#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    const string AppHost = "Steam++.sh";

    [Mobius(
"""
MobiusHost.StartSelfProcess / StartSelfProcessByPkexec
""")]
    public string GetAppHostPath()
    {
        return Path.Combine(AppContext.BaseDirectory, AppHost);
    }

    [Mobius(
"""
ShellHelper
""")]
    public ValueTask RunShellAsync(string script, bool requiredAdministrator)
         => UnixHelper.RunShellAsync(script, requiredAdministrator);
}
#endif