namespace BD.WTTS.Client.Tools.Publish.Models;

public sealed class AppVerFile
{
    public Guid Id { get; set; }

    /// <summary>
    /// 文件 Hash
    /// </summary>
    public string SHA384 { get; set; } = "";

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTimeOffset Published { get; set; }

    /// <summary>
    /// 文件原始 Byte 数据
    /// </summary>
    public byte[] DataRaw { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 文件大小
    /// </summary>
    public long Length { get; set; }
}
