using static BD.WTTS.SteamApiUrls;

namespace BD.WTTS.Models;

[XmlRoot("profile")]
public class SteamUser : ReactiveObject
{
    public const long UndefinedId = 76561197960265728;

    public SteamUser()
    {
    }

    [XmlIgnore]
    public string? SteamId3 => $"[U:1:{SteamId32}]";

    [XmlIgnore]
    public int SteamId32 => Convert.ToInt32((SteamId64 >> 0) & 0xFFFFFFFF);

    [XmlElement("steamID64")]
    public long SteamId64 { get; set; }

    /// <summary>
    /// 个人资料链接
    /// </summary>
    [XmlIgnore]
    public string ProfileUrl => string.Format(STEAM_PROFILES_URL, SteamId64);

    /// <summary>
    /// Userdata文件夹相对路径
    /// </summary>
    [XmlIgnore]
    public string UserdataPath => Path.Combine("userdata", SteamId32.ToString());

    private string? _OnlineState;

    /// <summary>
    /// 在线状态
    /// </summary>
    [XmlElement("onlineState")]
    public string? OnlineState
    {
        get => _OnlineState;
        set => this.RaiseAndSetIfChanged(ref _OnlineState, value);
    }

    int? _Level;

    /// <summary>
    /// Steam等级
    /// </summary>
    [XmlIgnore]
    public int? Level
    {
        get => _Level;
        set => this.RaiseAndSetIfChanged(ref _Level, value);
    }

    [XmlIgnore]
    public string? IPCountry { get; set; }

    /// <summary>
    /// 公开状态
    /// friendsonly
    /// public
    /// </summary>
    [XmlElement("privacyState")]
    public string? PrivacyState { get; set; }

    [XmlElement("avatarIcon")]
    public string? AvatarIcon { get; set; }

    [XmlElement("avatarMedium")]
    public string? AvatarMedium { get; set; }

    Task<string?>? _AvatarStream;

    [XmlIgnore]
    public Task<string?>? AvatarStream
    {
        get => _AvatarStream;
        set => this.RaiseAndSetIfChanged(ref _AvatarStream, value);
    }

    [XmlElement("avatarFull")]
    public string? AvatarFull { get; set; }

    /// <summary>
    /// 注册日期
    /// </summary>
    [XmlElement("memberSince")]
    public string? MemberSince { get; set; }

    /// <summary>
    /// VAC
    /// </summary>
    [XmlElement("vacBanned")]
    public bool VacBanned { get; set; }

    /// <summary>
    /// 自我介绍HTML
    /// </summary>
    [XmlElement("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [XmlElement("steamID")]
    public string? SteamID { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [XmlIgnore]
    public string? SteamNickName => string.IsNullOrEmpty(SteamID) ? PersonaName : SteamID;

    /// <summary>
    /// 从 Valve Data File 读取到的用户名
    /// </summary>
    [XmlIgnore]
    public string? PersonaName { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    /// 是否记住密码
    /// </summary>
    [XmlIgnore]
    public bool RememberPassword { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [XmlIgnore]
    public string? PassWord { get; set; }

    /// <summary>
    /// 最后登录时间戳
    /// </summary>
    [XmlIgnore]
    public long Timestamp { get; set; }

    /// <summary>
    /// 最后登录时间
    /// </summary>
    [XmlIgnore]
    public DateTime LastLoginTime { get; set; }

    bool _MostRecent;

    /// <summary>
    /// 最近登录
    /// </summary>
    [XmlIgnore]
    public bool MostRecent
    {
        get => _MostRecent;
        set => this.RaiseAndSetIfChanged(ref _MostRecent, value);
    }

    bool _WantsOfflineMode;

    /// <summary>
    /// 离线模式
    /// </summary>
    [XmlIgnore]
    public bool WantsOfflineMode
    {
        get => _WantsOfflineMode;
        set => this.RaiseAndSetIfChanged(ref _WantsOfflineMode, value);
    }

    /// <summary>
    /// 忽略离线模式警告弹窗
    /// </summary>
    [XmlIgnore]
    public bool SkipOfflineModeWarning { get; set; }

    string? _Remark;

    /// <summary>
    /// 备注
    /// </summary>
    [XmlIgnore]
    public string? Remark
    {
        get => _Remark;
        set => this.RaiseAndSetIfChanged(ref _Remark, value);
    }

    SteamMiniProfile? _MiniProfile;

    public SteamMiniProfile? MiniProfile
    {
        get => _MiniProfile;
        set => this.RaiseAndSetIfChanged(ref _MiniProfile, value);
    }

    ///// <summary>
    ///// 来源 Valve Data File 字符串
    ///// </summary>
    //[XmlIgnore]
    //public string? OriginVdfString { get; set; }

    ///// <summary>
    ///// 导出 Valve Data File 配置字符串
    ///// </summary>
    //[XmlIgnore]
    //public string CurrentVdfString =>
    //    "\"" + SteamId64 + "\"\n{\n" +
    //    "\t\t\"AccountName\"\t\t\"" + AccountName + "\"\n" +
    //    "\t\t\"PersonaName\"\t\t\"" + PersonaName + "\"\n" +
    //    "\t\t\"RememberPassword\"\t\t\"" + Convert.ToByte(RememberPassword) + "\"\n" +
    //    "\t\t\"MostRecent\"\t\t\"" + Convert.ToByte(MostRecent) + "\"\n" +
    //    "\t\t\"WantsOfflineMode\"\t\t\"" + Convert.ToByte(WantsOfflineMode) + "\"\n" +
    //    "\t\t\"SkipOfflineModeWarning\"\t\t\"" + Convert.ToByte(SkipOfflineModeWarning) + "\"\n" +
    //    "\t\t\"Timestamp\"\t\t\"" + Timestamp + "\"\n}";
}