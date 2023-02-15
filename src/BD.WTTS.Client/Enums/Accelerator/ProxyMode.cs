namespace BD.WTTS.Enums;

/// <summary>
/// 代理模式
/// </summary>
public enum ProxyMode : byte
{
    /// <summary>
    /// 通过注入驱动实现 DNS 拦截进行代理(Windows Only)
    /// </summary>
    [Description("ProxyMode_DNSIntercept")]
    DNSIntercept,

    /// <summary>
    /// 修改 Hosts 文件进行代理(Desktop Only)
    /// </summary>
    [Description("ProxyMode_Hosts")]
    Hosts,

    /// <summary>
    /// 系统代理模式(Desktop Only)
    /// </summary>
    [Description("ProxyMode_System")]
    System,

    /// <summary>
    /// VPN 代理模式(虚拟网卡)
    /// </summary>
    [Description("ProxyMode_VPN")]
    VPN,

    /// <summary>
    /// 仅代理模式
    /// </summary>
    [Description("ProxyMode_ProxyOnly")]
    ProxyOnly,

    /// <summary>
    /// PAC代理模式(Desktop Only)
    /// </summary>
    [Description("ProxyMode_PAC")]
    PAC,
}
