// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    #region LinuxEtc

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
        const string SteamOS = "SteamOS";

        const string ReleaseKey_PRETTY_NAME = "PRETTY_NAME";
        const string ReleaseKey_NAME = "NAME";
        const string ReleaseKey_VERSION_ID = "VERSION_ID";
        const string ReleaseKey_VERSION = "VERSION";
        const string ReleaseKey_VERSION_CODENAME = "VERSION_CODENAME";
        const string ReleaseKey_ID = "ID";
        const string ReleaseKey_ID_LIKE = "ID_LIKE";
        const string ReleaseKey_HOME_URL = "HOME_URL";
        const string ReleaseKey_BUG_REPORT_URL = "BUG_REPORT_URL";

        // NAME="SteamOS"
        // PRETTY_NAME="SteamOS"
        // VERSION_CODENAME=holo
        // ID=steamos
        // ID_LIKE=arch
        // ANSI_COLOR="1;35"
        // HOME_URL="https://www.steampowered.com/"
        // DOCUMENTATION_URL="https://support.steampowered.com/"
        // SUPPORT_URL="https://support.steampowered.com/"
        // BUG_REPORT_URL="https://support.steampowered.com/"
        // LOGO=steamos
        // VARIANT_ID=steamdeck
        // BUILD_ID=20230508.1
        // VERSION_ID=3.4.8

        // PRETTY_NAME="Deepin 20.9"
        // NAME="Deepin"
        // VERSION_ID="20.9"
        // VERSION="20.9"
        // VERSION_CODENAME="apricot"
        // ID=Deepin
        // HOME_URL="https://www.deepin.org/"
        // BUG_REPORT_URL="https://bbs.deepin.org/"

        enum Distribution : ushort
        {
            SteamOS = 1,

            Ubuntu,

            Deepin,
        }
    }

    static string? GetLinuxReleaseValue(string key)
    {
        foreach (var item in EtcOSRelease.Value)
        {
            if (string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))
                return item.Value;
        }
        return default;
    }

    private static readonly Lazy<string?> _LinuxReleaseId = new(() =>
    {
        var value = GetLinuxReleaseValue(LinuxConstants.ReleaseKey_ID);
        return value;
    });

    static string? LinuxReleaseId => _LinuxReleaseId.Value;

    private static readonly Lazy<LinuxConstants.Distribution> _LinuxDistribution = new(() =>
    {
        var value = LinuxReleaseId;
        if (!string.IsNullOrWhiteSpace(value) &&
            value.Length < sbyte.MaxValue)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            Utf8StringComparerOrdinalIgnoreCase comparer = new();
            if ("steamos"u8.SequenceEqual(bytes, comparer))
                return LinuxConstants.Distribution.SteamOS;
            else if ("ubuntu"u8.SequenceEqual(bytes, comparer))
                return LinuxConstants.Distribution.Ubuntu;
            else if ("Deepin"u8.SequenceEqual(bytes, comparer))
                return LinuxConstants.Distribution.Deepin;
        }
        return default;
    });

    /// <summary>
    /// 获取当前 Linux 发行版
    /// </summary>
    LinuxConstants.Distribution LinuxDistribution => _LinuxDistribution.Value;

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