using System.Application.Entities;
using System.Collections.Generic;
using System.Linq;
using MPKey = MessagePack.KeyAttribute;
using MPObject = MessagePack.MessagePackObjectAttribute;

namespace System.Application.Models
{
    [MPObject]
    public class AppVersionDTO : IEntity<Guid>, IExplicitHasValue
    {
        [MPKey(0)]
        public Guid Id { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        [MPKey(1)]
        public string? Version { get; set; }

        /// <summary>
        /// 本次更新描述
        /// </summary>
        [MPKey(2)]
        public string? Description { get; set; }

        /// <summary>
        /// (增量更新V1)新版本文件增量更新，由服务端比较版本差异返回差集，当文件哈希值相同时或路径不同时无法正确更新
        /// </summary>
        [MPKey(3)]
        [Obsolete("use AllFiles")]
        public IEnumerable<IncrementalUpdateDownload>? IncrementalUpdate { get; set; }

        /// <summary>
        /// 下载类型与下载地址
        /// </summary>
        [MPKey(4)]
        public IEnumerable<Download>? Downloads { get; set; }

        /// <summary>
        /// (单选)支持的平台
        /// </summary>
        [MPKey(5)]
        public Platform Platform { get; set; }

        /// <summary>
        /// (多或单选)支持的CPU构架
        /// </summary>
        [MPKey(6)]
        public ArchitectureFlags SupportedAbis { get; set; }

        /// <summary>
        /// (增量更新V2)新版本的完整文件列表清单，由客户端计算当前文件哈希比对清单，出现不一致时下载文件，注意文件哈希值相同但路径不同的情况下不要重复下载
        /// </summary>
        [MPKey(7)]
        public IEnumerable<IncrementalUpdateDownload>? AllFiles { get; set; }

        /// <summary>
        /// 是否禁用自动化更新，当此值为 <see langword="true"/> 时，仅提供跳转官网的手动更新方式
        /// </summary>
        [MPKey(8)]
        public bool DisableAutomateUpdate { get; set; }

        /// <summary>
        /// (增量更新V2)当前版本的完整文件列表清单
        /// </summary>
        [MPKey(9)]
        public IEnumerable<IncrementalUpdateDownload>? CurrentAllFiles { get; set; }

        bool IExplicitHasValue.ExplicitHasValue()
        {
            return !string.IsNullOrWhiteSpace(Version) &&
                !string.IsNullOrWhiteSpace(Description) &&
                Downloads != null && Downloads.All(x => x.HasValue());
        }

        [MPObject]
        public class Download : IExplicitHasValue
        {
            /// <inheritdoc cref="AppDownloadType"/>
            [MPKey(0)]
            public AppDownloadType DownloadType { get; set; }

            [MPKey(1)]
            public string? SHA256 { get; set; }

            [MPKey(2)]
            public long Length { get; set; }

            [MPKey(3)]
            public string? FileId { get; set; }

            bool IExplicitHasValue.ExplicitHasValue()
            {
                return !string.IsNullOrWhiteSpace(SHA256) &&
                    !string.IsNullOrWhiteSpace(FileId) &&
                    Length > 0;
            }
        }

        [MPObject]
        public class IncrementalUpdateDownload : IExplicitHasValue
        {
            [MPKey(0)]
            public string? SHA256 { get; set; }

            [MPKey(1)]
            public long Length { get; set; }

            [MPKey(2)]
            public string? FileId { get; set; }

            [MPKey(3)]
            public string? FileRelativePath { get; set; }

            bool IExplicitHasValue.ExplicitHasValue()
            {
                return !string.IsNullOrWhiteSpace(SHA256) &&
                    !string.IsNullOrWhiteSpace(FileId) &&
                    Length > 0;
            }
        }
    }
}