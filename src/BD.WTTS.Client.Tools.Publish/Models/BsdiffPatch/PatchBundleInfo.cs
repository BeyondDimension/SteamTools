namespace BD.WTTS.Client.Tools.Publish.Models;

/// <summary>
/// 修补包信息
/// </summary>
[MP2Obj(SerializeLayout.Explicit)]
public sealed partial class PatchBundleInfo
{
    /// <summary>
    /// 文件修补完后SHA384
    /// </summary>
    [MP2Key(0)]
    public string? SHA384 { get; set; }

    /// <summary>
    /// 补丁包数据
    /// </summary>
    [MP2Key(1)]
    public byte[]? PatchData { get; set; }

    /// <summary>
    /// 是否全量或补丁
    /// </summary>
    [MP2Key(2)]
    public bool IsFullOrPatch { get; set; }

}
