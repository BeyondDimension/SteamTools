// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    #region SystemUserPassword

#if LINUX
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
#endif

    #endregion
}