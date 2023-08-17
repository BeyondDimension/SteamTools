// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    #region LinuxEtcIssue

#if LINUX

    private const string LinuxFilePath_EtcOSRelease = "/etc/os-release";

    private static IEnumerable<KeyValuePair<string, string>> ReadEtcOSRelease()
    {
        var lines = File.ReadLines(LinuxFilePath_EtcOSRelease);
        foreach (var line in lines)
        {
            var array = line.Split('=', StringSplitOptions.None);
            if (array.Length >= 2)
            {
                var value = array[1].Trim().TrimStart('"').TrimEnd('"');
                if (!string.IsNullOrWhiteSpace(value))
                    yield return new KeyValuePair<string, string>(array[0], value);
            }
        }
    }

    protected static readonly Lazy<IEnumerable<KeyValuePair<string, string>>> EtcOSRelease = new(() =>
    {
        try
        {
            var r = ReadEtcOSRelease();
            return r;
        }
        catch
        {
        }
        return Array.Empty<KeyValuePair<string, string>>();
    });

#pragma warning disable IDE1006 // 命名样式
#pragma warning disable SA1302 // Interface names should begin with I
    interface LinuxConstants
#pragma warning restore SA1302 // Interface names should begin with I
#pragma warning restore IDE1006 // 命名样式
    {
        const string Deepin = "Deepin";
        const string Ubuntu = "Ubuntu";

        const string ReleaseKey_PRETTY_NAME = "PRETTY_NAME";
        const string ReleaseKey_NAME = "NAME";
        const string ReleaseKey_VERSION_ID = "VERSION_ID";
        const string ReleaseKey_VERSION = "VERSION";
        const string ReleaseKey_VERSION_CODENAME = "VERSION_CODENAME";
        const string ReleaseKey_ID = "ID";
        const string ReleaseKey_ID_LIKE = "ID_LIKE";
        const string ReleaseKey_HOME_URL = "HOME_URL";
        const string ReleaseKey_BUG_REPORT_URL = "BUG_REPORT_URL";
    }

    string? GetLinuxReleaseValue(string key)
    {
        foreach (var item in EtcOSRelease.Value)
        {
            if (string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))
                return item.Value;
        }
        return default;
    }

#endif

    #endregion

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