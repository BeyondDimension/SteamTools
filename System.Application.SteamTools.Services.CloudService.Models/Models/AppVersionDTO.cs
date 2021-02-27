using System.Application.Entities;
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
        /// 哈希SHA256验证安装包，可选
        /// </summary>
        [MPKey(2)]
        public string? SHA256 { get; set; }

        /// <summary>
        /// 本次更新描述
        /// </summary>
        [MPKey(3)]
        public string? Description { get; set; }

        /// <summary>
        /// 下载链接
        /// </summary>
        [MPKey(4)]
        public string? DownloadLink { get; set; }

        bool IExplicitHasValue.ExplicitHasValue()
        {
            return !string.IsNullOrWhiteSpace(Version) &&
                !string.IsNullOrWhiteSpace(SHA256) &&
                !string.IsNullOrWhiteSpace(Description) &&
                !string.IsNullOrWhiteSpace(DownloadLink);
        }
    }
}