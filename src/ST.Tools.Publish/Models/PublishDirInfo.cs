using MessagePack;
using System.Collections.Generic;
using MPKey = MessagePack.KeyAttribute;
using MPObject = MessagePack.MessagePackObjectAttribute;

namespace System.Application.Models
{
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

        /// <summary>
        /// 发布文件夹名
        /// </summary>
        [MPKey(0)]
        public string Name { get; set; } = string.Empty;

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
}