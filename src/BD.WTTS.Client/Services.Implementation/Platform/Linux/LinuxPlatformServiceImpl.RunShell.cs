#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    const string AppHost = "Steam++.sh";

    public string GetAppHostPath()
    {
        return Path.Combine(AppContext.BaseDirectory, AppHost);
    }

    public ValueTask RunShellAsync(string script, bool requiredAdministrator)
         => UnixHelper.RunShellAsync(script, requiredAdministrator);
}
#endif