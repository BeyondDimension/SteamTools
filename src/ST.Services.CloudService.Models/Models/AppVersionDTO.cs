using System.Application.Entities;
using System.Collections.Generic;
using System.Linq;
using static System.Application.Services.CloudService.Constants;
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
        /// 新版本文件增量更新
        /// </summary>
        [MPKey(3)]
        public IEnumerable<IncrementalUpdateDownload>? IncrementalUpdate { get; set; }

        /// <summary>
        /// 下载类型与下载地址
        /// </summary>
        [MPKey(4)]
        public IEnumerable<Download>? Downloads { get; set; }

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

        public static string GetRequestUri(string fileId) => $"{Prefix_HTTPS}steampp.net/uploads/publish/files/{fileId}.{FileEx.BIN}";
    }
}