// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    #region LinuxEtcIssue

#if LINUX

    private const string LinuxFilePath_EtcIssue = "/etc/issue";
    private const string Deepin = "Deepin";
    private const string Ubuntu = "Ubuntu";

    protected static readonly Lazy<string?> _LinuxEtcIssue = new(() =>
    {
        if (File.Exists(LinuxFilePath_EtcIssue))
            return File.ReadAllText(LinuxFilePath_EtcIssue).TrimEnd(" \\n \\l\n\n").Trim();
        return null;
    });

#endif

    /// <summary>
    /// 获取当前 Linux 系统发行版名称
    /// </summary>
    string? GetLinuxEtcIssue() =>
#if LINUX
        _LinuxEtcIssue.Value;
#else
        null;
#endif

    /// <summary>
    /// 获取当前 Linux 系统发行版是否为 深度操作系统（deepin）
    /// </summary>
    bool IsDeepin() =>
#if LINUX
        GetLinuxEtcIssue()?.Contains(Deepin, StringComparison.OrdinalIgnoreCase) ?? false;
#else
        false;
#endif

    /// <summary>
    /// 获取当前 Linux 系统发行版是否为 Ubuntu
    /// </summary>
    bool IsUbuntu() =>
#if LINUX
        GetLinuxEtcIssue()?.Contains(Ubuntu, StringComparison.OrdinalIgnoreCase) ?? false;
#else
        false;
#endif

    #endregion

    #region SystemUserPassword

    private const sbyte GetSystemUserPasswordRetry = 3;

    /// <summary>
    /// 获取或设置系统用户密码
    /// </summary>
    string? SystemUserPassword { get; set; }

    /// <summary>
    /// 获取系统用户密码（密码将缓存在 <see cref="SystemUserPassword"/> 中）
    /// </summary>
    /// <param name="retry"></param>
    /// <exception cref="PlatformNotSupportedException"></exception>
    async ValueTask<string?> GetSystemUserPasswordAsync(sbyte retry = GetSystemUserPasswordRetry)
    {
        if (SystemUserPassword != null)
            SystemUserPassword = await GetSystemUserPasswordIgnoreCacheAsync(retry) ?? "";
        return SystemUserPassword;
    }

    /// <inheritdoc cref="GetSystemUserPasswordAsync(sbyte)"/>
    async void GetSystemUserPassword(sbyte retry = GetSystemUserPasswordRetry) => await GetSystemUserPasswordAsync(retry);

    /// <summary>
    /// 获取系统用户密码（不进行缓存）
    /// </summary>
    /// <param name="retry"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    protected ValueTask<string?> GetSystemUserPasswordIgnoreCacheAsync(sbyte retry = GetSystemUserPasswordRetry)
        => throw new PlatformNotSupportedException();

#if DEBUG
    [Obsolete("use GetSystemUserPasswordAsync", true)]
    async void TryGetSystemUserPassword(sbyte retry = GetSystemUserPasswordRetry) => await GetSystemUserPasswordAsync(retry);
#endif

    #endregion
}