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
#endif

    /// <summary>
    /// hosts 文件所在目录
    /// </summary>
    string HostsFilePath =>
#if WINDOWS
        WINDOWS_HostsFilePath.HostsFilePath;
#else
        "/etc/hosts";
#endif

    /// <summary>
    /// 默认 hosts 文件内容
    /// </summary>
    [Obsolete("use WriteDefaultHostsContent(Stream", true)]
    string DefaultHostsContent => string.Empty;

    /// <summary>
    /// 写入默认 hosts 文件内容
    /// </summary>
    void WriteDefaultHostsContent()
    {
        using var fileStream = new FileStream(HostsFilePath,
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.ReadWrite | FileShare.Delete);
        WriteDefaultHostsContent(fileStream);
    }

    /// <summary>
    /// 写入默认 hosts 文件内容
    /// </summary>
    /// <param name="stream"></param>
    void WriteDefaultHostsContent(Stream stream)
    {
        stream.SetLength(0);
    }
}