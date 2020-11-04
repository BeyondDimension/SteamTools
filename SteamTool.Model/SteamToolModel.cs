using System;
using System.Collections.Generic;
using System.Text;

namespace SteamTool.Model
{
    public class SteamToolModel
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
