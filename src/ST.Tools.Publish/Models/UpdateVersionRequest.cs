using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Models
{
    public class UpdateVersionRequest
    {
        /// <summary>
        /// 指定一个版本号
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// 是否为开发环境，默认值否
        /// </summary>
        public bool Dev { get; set; }

        /// <summary>
        /// 指定码云下载链接，使用分号分割多个下载链接
        /// </summary>
        public string? Gitee { get; set; }

        /// <summary>
        /// 指定测试环境自定义下载链接，使用分号分割多个下载链接
        /// </summary>
        public string? DevCustom { get; set; }

        public IList<PublishDirInfo>? DirNames { get; set; }
    }
}
