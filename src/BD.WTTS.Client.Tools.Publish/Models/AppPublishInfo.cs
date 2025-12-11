using BD.WTTS.Client.Tools.Publish.Enums;

namespace BD.WTTS.Client.Tools.Publish.Models;

/// <summary>
/// 应用程序发布信息
/// </summary>
[MP2Obj(SerializeLayout.Explicit)]
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public sealed partial class AppPublishInfo
{
    string DebuggerDisplay() => $"{Platform} {Architecture} {DeviceIdiom} {DeploymentMode} {Path.GetDirectoryName(DirectoryPath)}";

    /// <summary>
    /// CPU Arch
    /// </summary>
    [MP2Key(0)]
    public Architecture Architecture { get; set; }

    /// <summary>
    /// 平台，单选
    /// </summary>
    [MP2Key(1)]
    public Platform Platform { get; set; }

    /// <summary>
    /// 设备类型，单选
    /// </summary>
    [MP2Key(2)]
    public DeviceIdiom DeviceIdiom { get; set; }

    /// <summary>
    /// 部署模式
    /// </summary>
    [MP2Key(3)]
    public DeploymentMode DeploymentMode { get; set; }

    /// <summary>
    /// 文件夹路径
    /// </summary>
    [MP2Key(4)]
    public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// 单文件包
    /// </summary>
    [MP2Key(5)]
    public Dictionary<CloudFileType, AppPublishFileInfo> SingleFile { get; set; } = new();

    /// <summary>
    /// 所有文件
    /// </summary>
    [MP2Key(6)]
    public List<AppPublishFileInfo> Files { get; set; } = new();

    [MP2Key(7)]
    public string RuntimeIdentifier { get; set; } = "";
}
