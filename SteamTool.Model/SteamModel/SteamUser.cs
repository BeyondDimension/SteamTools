using System;

namespace SteamTool.Model
{
    public class SteamUser
    {
        public string SteamId3 { get; set; }
        public long SteamId3_Int => ((SteamId64 >> (ushort)0) & 0xFFFFFFFF);

        public string SteamId32 { get; set; }
        public long SteamId32_Int { get; set; }

        public long SteamId64 { get; set; }

        /// <summary>
        /// 个人资料链接
        /// </summary>
        public string ProfileUrl => string.Format("https://steamcommunity.com/profiles/{0}", SteamId64);

        /// <summary>
        /// 在线状态
        /// </summary>
        public string OnlineState { get; set; }

        public string IPCountry { get; set; }

        /// <summary>
        /// 公开状态
        /// friendsonly
        /// public
        /// </summary>
        public string PrivacyState { get; set; }

        /// <summary>
        /// 头像链接
        /// </summary>
        public string AvatarIcon { get; set; }

        /// <summary>
        /// 头像链接
        /// </summary>
        public string AvatarMedium { get; set; }

        /// <summary>
        /// 头像链接
        /// </summary>
        public string AvatarFull { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string SteamID { get; set; }

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
