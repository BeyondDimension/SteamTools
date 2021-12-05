using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Models
{
    public class CreateVersionRequest
    {
        public string? Version { get; set; }

        /// <summary>
        /// 新版本的描述
        /// </summary>
        public string? Desc { get; set; }

        /// <summary>
        /// 是否使用上一个版本的密钥
        /// </summary>
        public bool UseLastSKey { get; set; }
    }
}
