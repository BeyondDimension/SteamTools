namespace BD.WTTS.Client.Tools.Publish.Models;

/// <summary>
/// 程序发布【修补包】信息
/// </summary>
[MP2Obj(SerializeLayout.Explicit)]
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public sealed partial class AppPublishPatchBundleInfo
{
    string DebuggerDisplay() => $"{OldAppVer}-{NewAppVer} {SHA384} {IOPath.GetDisplayFileSizeString(Length)}";

    /// <summary>
    /// 当前包适用程序版本
    /// </summary>
    [MP2Key(0)]
    public string OldAppVer { get; set; } = string.Empty;

    /// <summary>
    /// 当前包适用程序版本
    /// </summary>
    [MP2Key(1)]
    public string NewAppVer { get; set; } = string.Empty;

    /// <summary>
    /// 修补包 SHA384 值
    /// </summary>
    [MP2Key(2)]
    public string SHA384 { get; set; } = string.Empty;

    /// <summary>
    /// 修补包文件大小
    /// </summary>
    [MP2Key(3)]
    public long Length { get; set; }

    /// <summary>
    /// 修补包文件路径
    /// </summary>
    [MP2Key(4)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 修补包数据 (文件相对路径，包信息 为 null 删除文件) 不存在其中的文件全量复制
    /// </summary>
    [MP2Key(5)]
    public Dictionary<string, PatchBundleInfo?> PatchData { get; set; } = new();
}
