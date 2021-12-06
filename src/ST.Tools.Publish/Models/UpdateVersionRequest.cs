using System.Collections.Generic;
using MPKey = MessagePack.KeyAttribute;
using MPObject = MessagePack.MessagePackObjectAttribute;

namespace System.Application.Models
{
    [MPObject]
    public class UpdateVersionRequest
    {
        /// <summary>
        /// 指定一个版本号
        /// </summary>
        [MPKey(0)]
        public string? Version { get; set; }

        /// <summary>
        /// 是否为开发环境，默认值否
        /// </summary>
        [MPKey(1)]
        public bool Dev { get; set; }

        /// <summary>
        /// 指定码云下载链接，使用分号分割多个下载链接
        /// </summary>
        [MPKey(2)]
        public string? Gitee { get; set; }

        /// <summary>
        /// 指定测试环境自定义下载链接，使用分号分割多个下载链接
        /// </summary>
        [MPKey(3)]
        public string? DevCustom { get; set; }

        [MPKey(4)]
        public IList<PublishDirInfo>? DirNames { get; set; }
    }
}
