using System.Application.Models;
using System.Xml.Serialization;
using static System.Application.SteamApiUrls;

namespace System.Application.Models
{
    [XmlRoot("profile")]
    public class SteamUser : ISteamUserAvatar
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
        public string? PrivacyState { get; set; }

        [XmlElement("avatarIcon")]
        [Obsolete("use GetAvatarIcon()", true)]
        public string? AvatarIcon { get; set; }

        [XmlElement("avatarMedium")]
        [Obsolete("use GetAvatarMedium()", true)]
        public string? AvatarMedium { get; set; } /*= "/Resources/Asstes/avater.jpg";*/

        [XmlElement("avatarFull")]
        [Obsolete("use GetAvatarFull()", true)]
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

    internal interface ISteamUserAvatar
    {
        public const string DefaultAvatar = "/Resources/Asstes/avater.jpg";

        /// <summary>
        /// 头像小图标
        /// </summary>
        string? AvatarIcon { get; set; }

        /// <summary>
        /// 头像中等图片链接
        /// </summary>
        string? AvatarMedium { get; set; }

        /// <summary>
        /// 头像大图链接
        /// </summary>
        string? AvatarFull { get; set; }
    }
}

namespace System
{
    public static class SteamUserAvatarExtensions
    {
        static ISteamUserAvatar GetAvatar(this SteamUser user)
        {
            ISteamUserAvatar user_ = user;
            return user_;
        }

        /// <summary>
        /// 获取头像小图标
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetAvatarIcon(this SteamUser user) => user.GetAvatar().AvatarIcon ?? ISteamUserAvatar.DefaultAvatar;

        /// <summary>
        /// 获取头像中等图片链接
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetAvatarMedium(this SteamUser user) => user.GetAvatar().AvatarMedium ?? ISteamUserAvatar.DefaultAvatar;

        /// <summary>
        /// 获取头像大图链接
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetAvatarFull(this SteamUser user) => user.GetAvatar().AvatarFull ?? ISteamUserAvatar.DefaultAvatar;
    }
}