using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTool.Core.Model
{
    public class SteamToolConfig
    {
        /// <summary>
        /// 启动参数
        /// </summary>
        public string SteamRunParam { get; set; }

        /// <summary>
        /// 保存用户
        /// </summary>
        public List<SteamUser> SteamUsers { get; set; } 


        public bool EnableTextLog { get; set; }
    }
}
