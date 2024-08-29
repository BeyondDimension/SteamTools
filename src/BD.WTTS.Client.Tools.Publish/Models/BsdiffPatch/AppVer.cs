namespace BD.WTTS.Client.Tools.Publish.Models;

public sealed class AppVer
{
    public Guid Id { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string Ver { get; set; } = "";

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTimeOffset Published { get; set; }

    /// <summary>
    /// 支持的平台
    /// </summary>
    public ClientPlatform Platform { get; set; }

    public IList<AppVerFileList>? FileList { get; set; }
}
