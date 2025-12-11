namespace BD.WTTS.Client.Tools.Publish.Models;

/// <summary>
/// 应用程序发布【文件】信息
/// </summary>
[MP2Obj(SerializeLayout.Explicit)]
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public sealed partial class AppPublishFileInfo
{
    string DebuggerDisplay() => $"{IOPath.GetDisplayFileSizeString(Length)} {Path.GetDirectoryName(FilePath)}";

    [MP2Constructor]
    public AppPublishFileInfo()
    {

    }

    /// <summary>
    /// 文件路径
    /// </summary>
    [MP2Key(0)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 相对路径
    /// </summary>
    [MP2Key(1)]
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// 文件的 SHA256 哈希值
    /// </summary>
    [MP2Key(2)]
    public string SHA256 { get; set; } = string.Empty;

    /// <summary>
    /// 文件的 SHA384 哈希值
    /// </summary>
    [MP2Key(3)]
    public string SHA384 { get; set; } = string.Empty;

    /// <summary>
    /// 文件扩展名
    /// </summary>
    [MP2Key(4)]
    public string FileEx { get; set; } = string.Empty;

    /// <summary>
    /// 数字签名后的文件的 SHA256 哈希值
    /// </summary>
    [MP2Key(5)]
    public string SignatureSHA256 { get; set; } = string.Empty;

    /// <summary>
    /// 数字签名后的文件的 SHA384 哈希值
    /// </summary>
    [MP2Key(6)]
    public string SignatureSHA384 { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小
    /// </summary>
    [MP2Key(7)]
    public long Length { get; set; }
}
