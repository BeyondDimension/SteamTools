using dotnetCampus.Ipc.CompilerServices.Attributes;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 反向代理服务
/// </summary>
[IpcPublic(Timeout = 55000, IgnoresIpcException = false)]
public partial interface IReverseProxyService : IDisposable, IReverseProxySettings
{
    /// <summary>
    /// 反向代理的常量与配置项，在插件中由 IPC 远程调用给子进程
    /// </summary>
    static class Constants
    {
        public static IReverseProxyService Instance => Ioc.Get<IReverseProxyService>();

        /// <inheritdoc cref="CertificateConstants.CertificateName"/>
        public const string CertificateName = CertificateConstants.CertificateName;

        /// <inheritdoc cref="CertificateConstants.RootCertificateName"/>
        public const string RootCertificateName = CertificateConstants.RootCertificateName;

        public const ExternalProxyType DefaultTwoLevelAgentProxyType = ExternalProxyType.Socks5;

        public const string LocalDomain = "local.steampp.net";

        public static IPAddress DefaultProxyIp => IPAddress.Any;
    }

    /// <summary>
    /// 当前代理服务是否正在运行
    /// </summary>
    bool ProxyRunning { get; }

    /// <summary>
    /// 检查端口号是否被使用
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    bool PortInUse(int port) => SocketHelper.IsUsePort(ProxyIp, port);

    /// <summary>
    /// 启动代理服务
    /// </summary>
    /// <returns></returns>
    ValueTask<bool> StartProxyAsync();

    /// <summary>
    /// 停止代理服务
    /// </summary>
    /// <returns></returns>
    ValueTask StopProxyAsync();
}

/// <summary>
/// 反向代理服务的设置项
/// </summary>
public partial interface IReverseProxySettings
{
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

    bool TwoLevelAgentEnable { get; set; }

    ExternalProxyType TwoLevelAgentProxyType { get; set; }

    string? TwoLevelAgentIp { get; set; }

    int TwoLevelAgentPortId { get; set; }

    string? TwoLevelAgentUserName { get; set; }

    string? TwoLevelAgentPassword { get; set; }

    #endregion

    IPAddress? ProxyDNS { get; set; }
}