// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPCPlatformService
{
    /// <summary>
    /// 运行 Shell 脚本
    /// </summary>
    /// <param name="script">要运行的脚本字符串</param>
    /// <param name="requiredAdministrator">是否以管理员或 Root 权限运行</param>
    async void RunShell(string script, bool requiredAdministrator = false)
        => await RunShellAsync(script, requiredAdministrator);

    /// <inheritdoc cref="RunShell(string, bool)"/>
    ValueTask RunShellAsync(string script, bool requiredAdministrator = false) => default;

    ValueTask<bool?> RunShellReturnAsync(string script, bool requiredAdministrator = false) => default;
}