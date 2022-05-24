using MessagePack;
using System.Runtime.InteropServices;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using MPKey = MessagePack.KeyAttribute;
using MPObject = MessagePack.MessagePackObjectAttribute;

namespace System.Application.Models;

/// <summary>
/// 发布文件夹信息
/// </summary>
[MPObject]
public class PublishDirInfo
{
    [SerializationConstructor]
    public PublishDirInfo()
    {
    }

    public PublishDirInfo(string dirName, string dirPath, DeploymentMode deploymentMode)
    {
        Name = dirName;
        Path = dirPath;
        DeploymentMode = deploymentMode;
    }

    string _Name = string.Empty;

    /// <summary>
    /// 发布文件夹名
    /// </summary>
    [MPKey(0)]
    public string Name
    {
        get => _Name;
        set
        {
            var array = value.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (array.Length >= 1)
            {
                switch (array[0]?.ToLower())
                {
                    case "win":
                        Platform = OSPlatform.Windows;
                        break;
                    case "linux":
                        Platform = OSPlatform.Linux;
                        break;
                    case "osx":
                        Platform = OSPlatform.OSX;
                        break;
                }
            }
            if (array.Length >= 2)
            {
                switch (array[1]?.ToLower())
                {
                    case "x86":
                        Architecture = Architecture.X86;
                        break;
                    case "x64":
                        Architecture = Architecture.X64;
                        break;
                    case "arm":
                        Architecture = Architecture.Arm;
                        break;
                    case "arm64":
                        Architecture = Architecture.Arm64;
                        break;
                }
            }
            _Name = value;
        }
    }

    [MPIgnore]
    public OSPlatform Platform { get; private set; }

    [MPIgnore]
    public Architecture Architecture { get; private set; } = (Architecture)int.MinValue;

    /// <summary>
    /// 发布文件夹路径
    /// </summary>
    [MPKey(1)]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// 文件列表
    /// </summary>
    [MPKey(2)]
    public List<PublishFileInfo> Files { get; set; } = new();

    /// <summary>
    /// 单文件打包哈希值
    /// </summary>
    [MPKey(3)]
    public Dictionary<AppDownloadType, PublishFileInfo> BuildDownloads { get; set; } = new();

    [MPKey(4)]
    public DeploymentMode DeploymentMode { get; set; }
}