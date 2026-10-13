// ReSharper disable once CheckNamespace
namespace BD.WTTS.Enums;

/// <summary>
/// 代理模式
/// </summary>
public enum ProxyMode : byte
{
    /// <summary>
    /// 通过注入驱动实现 DNS 拦截进行代理(Windows Only)
    /// </summary>
    DNSIntercept,

    /// <summary>
    /// 修改 Hosts 文件进行代理(Desktop Only)
    /// </summary>
    Hosts,

    /// <summary>
    /// 系统代理模式(Desktop Only)
    /// </summary>
    System,

    /// <summary>
    /// NixOS DNS 动态劫持模式（由系统配置 programs.watt-toolkit.proxyMode 统一控制，UI 不可选）
    /// </summary>
    DNS,

    /// <summary>
    /// VPN 代理模式(虚拟网卡)
    /// </summary>
    VPN,

    /// <summary>
    /// 仅代理模式
    /// </summary>
    ProxyOnly,

    /// <summary>
    /// PAC代理模式(Desktop Only)
    /// </summary>
    PAC,
}
