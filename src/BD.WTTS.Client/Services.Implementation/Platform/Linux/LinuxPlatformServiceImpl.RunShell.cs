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

    /// <summary>
    /// 运行 Shell 脚本
    /// </summary>
    /// <param name="script">要运行的脚本字符串</param>
    /// <param name="requiredAdministrator">是否以管理员或 Root 权限运行</param>
    public static async void RunShell(string script, bool requiredAdministrator = false)
        => await RunShellAsync(script, requiredAdministrator);

    /// <inheritdoc cref="RunShell(string, bool)"/>
    public static ValueTask RunShellAsync(string script, bool requiredAdministrator = false)
    {
        return UnixHelper.RunShellAsync(script, requiredAdministrator);
    }
}
#endif