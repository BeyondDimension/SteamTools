using System.Application.Models;
using System.IO;
using System.Xml.Serialization;
using static System.Application.SteamApiUrls;

namespace System.Application.Models
{
    [XmlRoot("profile")]
    public class SteamUser
    {
        public SteamUser()
        {
        }

        public SteamUser(string vdfstring)
        {
            OriginVdfString = vdfstring;
        }

        public string? SteamId3 { get; set; }

        public long SteamId3_Int => (SteamId64 >> 0) & 0xFFFFFFFF;

        public string? SteamId32 { get; set; }

        public long SteamId32_Int { get; set; }

        [XmlElement("steamID64")]
        public long SteamId64 { get; set; }

        /// <summary>
        /// 个人资料链接
        /// </summary>
        public string ProfileUrl => string.Format(STEAM_PROFILES_URL, SteamId64);

        /// <summary>
        /// 在线状态
        /// </summary>
        [XmlElement("onlineState")]
        public string? OnlineState { get; set; }

        public string? IPCountry { get; set; }

        /// <summary>
        /// 公开状态
        /// friendsonly
        /// public
        /// </summary>
        [XmlElement("privacyState")]
        public string PrivacyState { get; set; }

        [XmlElement("avatarIcon")]
        public string AvatarIcon { get; set; }

        [XmlElement("avatarMedium")]
        public string AvatarMedium { get; set; }

        public Stream? AvatarStream { get; set; }

        [XmlElement("avatarFull")]
        public string? AvatarFull { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [XmlElement("steamID")]
        public string? SteamID { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string? SteamNickName => string.IsNullOrEmpty(SteamID) ? PersonaName : SteamID;

        /// <summary>
        /// 从 Valve Data File 读取到的用户名
        /// </summary>
        public string? PersonaName { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? AccountName { get; set; }

        /// <summary>
        /// 是否记住密码
        /// </summary>
        public bool RememberPassword { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string? PassWord { get; set; }

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
        /// 离线模式
        /// </summary>
        public bool WantsOfflineMode { get; set; }

        /// <summary>
        /// 忽略离线模式警告弹窗
        /// </summary>
        public bool SkipOfflineModeWarning { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 来源 Valve Data File 字符串
        /// </summary>
        public string? OriginVdfString { get; set; }

        /// <summary>
        /// 导出 Valve Data File 配置字符串
        /// </summary>
        public string CurrentVdfString =>
            "\"" + SteamId64 + "\"\n{\n" +
            "\t\t\"AccountName\"\t\t\"" + AccountName + "\"\n" +
            "\t\t\"PersonaName\"\t\t\"" + PersonaName + "\"\n" +
            "\t\t\"RememberPassword\"\t\t\"" + Convert.ToByte(RememberPassword) + "\"\n" +
            "\t\t\"MostRecent\"\t\t\"" + Convert.ToByte(MostRecent) + "\"\n" +
            "\t\t\"WantsOfflineMode\"\t\t\"" + Convert.ToByte(WantsOfflineMode) + "\"\n" +
            "\t\t\"SkipOfflineModeWarning\"\t\t\"" + Convert.ToByte(SkipOfflineModeWarning) + "\"\n" +
            "\t\t\"Timestamp\"\t\t\"" + Timestamp + "\"\n}";
    }
}