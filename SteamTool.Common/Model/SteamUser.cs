using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.Model
{
    public class SteamUser
    {

        public string SteamId3 { get; set; }
        public int SteamId3_Int { get; }

        public string SteamId32 { get; set; }
        public int SteamId32_Int { get; }

        public long SteamId64 { get; set; }

        /// <summary>
        /// 个人资料链接
        /// </summary>
        public string ProfileUrl { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string PersonaName { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// 是否记住密码
        /// </summary>
        public bool RememberPassword { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord { get; set; }

        /// <summary>
        /// 最后登录时间戳
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime LastLoginTime { get; set; }

        /// <summary>
        /// 最近登录
        /// </summary>
        public bool MostRecent { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
