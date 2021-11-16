using System.Collections.Generic;

namespace System.Application.Models
{
    /// <summary>
    /// 发布文件夹信息
    /// </summary>
    internal class PublishDirInfo
    {
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
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 发布文件夹路径
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 文件列表
        /// </summary>
        public List<PublishFileInfo> Files { get; set; } = new();

        /// <summary>
        /// 单文件打包哈希值
        /// </summary>
        public Dictionary<AppDownloadType, PublishFileInfo> BuildDownloads { get; set; } = new();

        public DeploymentMode DeploymentMode { get; set; }
    }
}