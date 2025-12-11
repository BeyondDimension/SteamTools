namespace BD.WTTS.Client.Tools.Publish.Models;

public sealed class AppVerFileList
{
    public Guid Id { get; set; }

    /// <summary>
    /// 版本号ID
    /// </summary>
    public Guid AppVerId { get; set; }

    /// <summary>
    /// 平台
    /// </summary>
    public ClientPlatform Platform { get; set; }

    /// <summary>
    /// 文件清单列表
    /// </summary>
    public byte[] FileListRaw { get; set; } = Array.Empty<byte>();

    public AppVer? AppVerInfo { get; set; }
}
