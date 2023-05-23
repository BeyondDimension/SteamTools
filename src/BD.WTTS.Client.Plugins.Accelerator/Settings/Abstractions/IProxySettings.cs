using AppResources = BD.WTTS.Client.Resources.Strings;
using EProxyMode = BD.WTTS.Enums.ProxyMode;

namespace BD.WTTS.Settings.Abstractions;

public interface IProxySettings
{

    static IProxySettings? Instance => Ioc.Get_Nullable<IOptionsMonitor<IProxySettings>>()?.CurrentValue;

    /// <summary>
    /// 启用脚本自动检查更新
    /// </summary>
    bool? IsAutoCheckScriptUpdate { get; set; }

    const bool DefaultIsAutoCheckScriptUpdate = true;

    /// <summary>
    /// 启用代理脚本
    /// </summary>
    bool? IsEnableScript { get; set; }

    const bool DefaultIsEnableScript = false;

    /// <summary>
    /// 代理服务启用状态
    /// </summary>
    IReadOnlyCollection<string>? SupportProxyServicesStatus { get; set; }

    static readonly IReadOnlyCollection<string> DefaultSupportProxyServicesStatus = Array.Empty<string>();

    /// <summary>
    /// 脚本启用状态
    /// </summary>
    IReadOnlyCollection<int>? ScriptsStatus { get; set; }

    static readonly IReadOnlyCollection<int> DefaultScriptsStatus = Array.Empty<int>();

    #region 代理设置

    /// <summary>
    /// 程序启动时自动启动代理
    /// </summary>
    bool? ProgramStartupRunProxy { get; set; }

    const bool DefaultProgramStartupRunProxy = false;

    /// <summary>
    /// 系统代理模式端口
    /// </summary>
    int? SystemProxyPortId { get; set; }

    const int DefaultSystemProxyPortId = 26561;

    /// <summary>
    /// 系统代理模式IP
    /// </summary>
    string? SystemProxyIp { get; set; }

    static readonly string DefaultSystemProxyIp = IPAddress.Any.ToString();

    /// <summary>
    /// 开启加速后仅代理脚本而不加速
    /// </summary>
    bool? OnlyEnableProxyScript { get; set; }

    const bool DefaultOnlyEnableProxyScript = false;

    /// <summary>
    /// 代理时使用的解析主DNS
    /// </summary>
    string? ProxyMasterDns { get; set; }

    const string DefaultProxyMasterDns = "223.5.5.5";

    /// <summary>
    /// 启用 Http 链接转发到 Https
    /// </summary>
    bool? EnableHttpProxyToHttps { get; set; }

    const bool DefaultEnableHttpProxyToHttps = true;

    #endregion

    #region 本地代理设置

    /// <summary>
    /// Socks5 Enable
    /// </summary>
    bool? Socks5ProxyEnable { get; set; }

    const bool DefaultSocks5ProxyEnable = false;

    /// <summary>
    /// Socks5 监听端口
    /// </summary>
    int? Socks5ProxyPortId { get; set; }

    const int DefaultSocks5ProxyPortId = 8868;

    #endregion

    #region 二级代理设置

    /// <summary>
    /// TwoLevelAgent Enable
    /// </summary>
    bool? TwoLevelAgentEnable { get; set; }

    const bool DefaultTwoLevelAgentEnable = false;

    /// <summary>
    /// TwoLevelAgent ProxyType
    /// </summary>
    short? TwoLevelAgentProxyType { get; set; }

    const short DefaultTwoLevelAgentProxyType =
        (short)IReverseProxyService.Constants.DefaultTwoLevelAgentProxyType;

    /// <summary>
    /// 二级代理 IP
    /// </summary>
    string? TwoLevelAgentIp { get; set; }

    static readonly string DefaultTwoLevelAgentIp = IPAddress.Loopback.ToString();

    /// <summary>
    /// 二级代理 监听端口
    /// </summary>
    int? TwoLevelAgentPortId { get; set; }

    const int DefaultTwoLevelAgentPortId = 7890;

    /// <summary>
    /// TwoLevelAgent UserName
    /// </summary>
    string? TwoLevelAgentUserName { get; set; }

    /// <summary>
    /// TwoLevelAgent Password
    /// </summary>
    string? TwoLevelAgentPassword { get; set; }

    #endregion

    #region 代理模式设置

    static EProxyMode DefaultProxyMode => ProxyModes[0];

    static IEnumerable<EProxyMode> GetProxyModes()
    {
#if WINDOWS
        yield return EProxyMode.Hosts;
        yield return EProxyMode.DNSIntercept;
        yield return EProxyMode.PAC;
        yield return EProxyMode.System;
#elif ANDROID
        yield return EProxyMode.VPN;
        yield return EProxyMode.ProxyOnly;
#elif LINUX || MACOS || MACCATALYST
#if MACCATALYST
        if (OperatingSystem.IsMacOS())
#endif
        {
            yield return EProxyMode.Hosts;
            yield return EProxyMode.System;
        }
#else
        return Array.Empty<EProxyMode>();
#endif
    }

    public static IReadOnlyList<EProxyMode> ProxyModes => mProxyModes.Value;

    static readonly Lazy<IReadOnlyList<EProxyMode>> mProxyModes = new(() => GetProxyModes().ToArray());

    /// <summary>
    /// 当前代理模式
    /// </summary>
    EProxyMode ProxyMode { get; set; }

    /// <inheritdoc cref="ProxyMode"/>
    EProxyMode ProxyModeValue
    {
        get
        {
            var value = ProxyMode;
            if (ProxyModes.Contains(value)) return value;
            return DefaultProxyMode;
        }
        set => ProxyMode = value;
    }

    public static string ToStringByProxyMode(EProxyMode mode) => mode switch
    {
        EProxyMode.DNSIntercept => AppResources.ProxyMode_DNSIntercept,
        EProxyMode.Hosts => AppResources.ProxyMode_Hosts,
        EProxyMode.System => AppResources.ProxyMode_System,
        EProxyMode.VPN => AppResources.ProxyMode_VPN,
        EProxyMode.ProxyOnly => AppResources.ProxyMode_ProxyOnly,
        _ => string.Empty,
    };

    string ProxyModeValueString => ToStringByProxyMode(ProxyModeValue);

    #endregion

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    /// <summary>
    /// 启用 GOG 插件代理
    /// </summary>
    bool? IsProxyGOG { get; set; }

    const bool DefaultIsProxyGOG = false;

    /// <summary>
    /// 是否只针对 Steam 内置浏览器启用脚本
    /// </summary>
    bool? IsOnlyWorkSteamBrowser { get; set; }

    const bool DefaultIsOnlyWorkSteamBrowser = false;

#endif
}
