using System.Net;
using System.Application.Models;

namespace System.Application.Services;

/// <summary>
/// 反向代理服务
/// </summary>
public interface IReverseProxyService : IDisposable
{
    /// <summary>
    /// 证书名称，硬编码不可改动，确保兼容性
    /// </summary>
    const string CertificateName = "SteamTools";

    const string RootCertificateName = $"{CertificateName} Certificate";
    const string LocalDomain = "local.steampp.net";
    const string TAG = "ReverseProxyS";

    static IReverseProxyService Instance => DI.Get<IReverseProxyService>();

    ICertificateManager CertificateManager { get; }

    /// <summary>
    /// 当前勾选的加速项目组
    /// </summary>
    IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }

    /// <summary>
    /// 当前勾选的脚本集
    /// </summary>
    IReadOnlyCollection<ScriptDTO>? Scripts { get; set; }

    /// <summary>
    /// 是否启用脚本
    /// </summary>
    bool IsEnableScript { get; set; }

    /// <summary>
    /// 是否只针对 Steam 内置浏览器启用脚本
    /// </summary>
    bool IsOnlyWorkSteamBrowser { get; set; }

    /// <summary>
    /// 代理服务器端口号
    /// </summary>
    int ProxyPort { get; set; }

    /// <summary>
    /// 代理服务器 IP 地址
    /// </summary>
    IPAddress ProxyIp { get; set; }

    /// <summary>
    /// 代理模式
    /// </summary>
    ProxyMode ProxyMode { get; set; }

    /// <summary>
    /// 启用 GOG 插件代理
    /// </summary>
    bool IsProxyGOG { get; set; }

    /// <summary>
    /// 开启加速后仅代理脚本而不加速
    /// </summary>
    bool OnlyEnableProxyScript { get; set; }

    /// <summary>
    /// 启用 Http 链接转发到 Https
    /// </summary>
    bool EnableHttpProxyToHttps { get; set; }

    #region Socks5

    /// <summary>
    /// Socks5 Enable
    /// </summary>
    bool Socks5ProxyEnable { get; set; }

    /// <summary>
    /// Socks5 监听端口
    /// </summary>
    int Socks5ProxyPortId { get; set; }

    #endregion

    #region TwoLevelAgent(二级代理)

    const EExternalProxyType DefaultTwoLevelAgentProxyType = EExternalProxyType.Socks5;

    bool TwoLevelAgentEnable { get; set; }

    EExternalProxyType TwoLevelAgentProxyType { get; set; }

    string? TwoLevelAgentIp { get; set; }

    int TwoLevelAgentPortId { get; set; }

    string? TwoLevelAgentUserName { get; set; }

    string? TwoLevelAgentPassword { get; set; }

    #endregion

    IPAddress? ProxyDNS { get; set; }

    /// <summary>
    /// 当前代理服务是否正在运行
    /// </summary>
    bool ProxyRunning { get; }

    /// <summary>
    /// 检查端口号是否被使用
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    bool PortInUse(int port);

    Task<bool> StartProxy();

    Task StopProxy();

    /// <summary>
    /// 将 PEM 证书公钥写入 GOG GALAXY
    /// </summary>
    /// <returns></returns>
    bool WirtePemCertificateToGoGSteamPlugins();

    /// <summary>
    /// 获取当前反向代理实现引擎
    /// </summary>
    EReverseProxyEngine ReverseProxyEngine { get; }

}
