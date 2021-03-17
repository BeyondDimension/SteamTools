using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application
{
    /// <summary>
    /// 迁移
    /// </summary>
    public static class Migrations
    {
        /// <summary>
        /// 从 V1 版本 迁移配置文件等信息
        /// </summary>
        public static void FromV1()
        {
            // 将以前的配置读取，存储到新的格式
            // 删除以前的配置文件

            // 执行迁移成功后，在AppData下写入文件或写入键值对中的一个标记
            // 下次启动检查标记，跳过迁移
        }
    }
}
