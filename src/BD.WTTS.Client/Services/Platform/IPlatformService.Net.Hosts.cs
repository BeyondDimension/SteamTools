// ReSharper disable once CheckNamespace

namespace BD.WTTS.Services;

partial interface IPlatformService
{
#if WINDOWS
    static class WINDOWS_HostsFilePath
    {
        internal static readonly string HostsFilePath =
            Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts");
    }
#else
    const string UnixHostsFilePath = $"/etc/hosts";
#endif

    /// <summary>
    /// hosts 文件所在目录
    /// </summary>
    string HostsFilePath =>
#if WINDOWS
        WINDOWS_HostsFilePath.HostsFilePath;
#else
        UnixHostsFilePath;
#endif

    /// <summary>
    /// 默认 hosts 文件内容
    /// </summary>
    string DefaultHostsContent => string.Empty;
}