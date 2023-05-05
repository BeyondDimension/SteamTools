namespace BD.WTTS.Services;

/// <summary>
/// 反向代理服务
/// </summary>
public partial interface IReverseProxyService : IDisposable, IReverseProxySettings
{
    // 反向代理的常量与配置项，在插件中由 IPC 远程调用给子进程

    const string TAG = "ReverseProxyS";

    static IReverseProxyService Instance => Ioc.Get<IReverseProxyService>();

    /// <inheritdoc cref="CertificateConstants.CertificateName"/>
    const string CertificateName = CertificateConstants.CertificateName;

    /// <inheritdoc cref="CertificateConstants.RootCertificateName"/>
    const string RootCertificateName = CertificateConstants.RootCertificateName;

    const string LocalDomain = "local.steampp.net";

    const ExternalProxyType DefaultTwoLevelAgentProxyType = ExternalProxyType.Socks5;

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

    /// <summary>
    /// 获取当前反向代理实现引擎
    /// </summary>
    ReverseProxyEngine ReverseProxyEngine { get; }
}

/// <summary>
/// 反向代理服务的设置项
/// </summary>
public partial interface IReverseProxySettings
{
    void SetValue(IReverseProxySettings settings)
    {
        ProxyDomains = settings.ProxyDomains;
        Scripts = settings.Scripts;
        IsEnableScript = settings.IsEnableScript;
        IsOnlyWorkSteamBrowser = settings.IsOnlyWorkSteamBrowser;
        ProxyPort = settings.ProxyPort;
        ProxyIp = settings.ProxyIp;
        ProxyMode = settings.ProxyMode;
        IsProxyGOG = settings.IsProxyGOG;
        OnlyEnableProxyScript = settings.OnlyEnableProxyScript;
        EnableHttpProxyToHttps = settings.EnableHttpProxyToHttps;
        Socks5ProxyEnable = settings.Socks5ProxyEnable;
        Socks5ProxyPortId = settings.Socks5ProxyPortId;
        TwoLevelAgentEnable = settings.TwoLevelAgentEnable;
        TwoLevelAgentProxyType = settings.TwoLevelAgentProxyType;
        TwoLevelAgentIp = settings.TwoLevelAgentIp;
        TwoLevelAgentPortId = settings.TwoLevelAgentPortId;
        TwoLevelAgentUserName = settings.TwoLevelAgentUserName;
        TwoLevelAgentPassword = settings.TwoLevelAgentPassword;
        ProxyDNS = settings.ProxyDNS;
    }

    static ReverseProxySettings Create(IReverseProxySettings settings)
    {
        ReverseProxySettings s = new();
        ((IReverseProxySettings)s).SetValue(settings);
        return s;
    }

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

    static IPAddress DefaultProxyIp => IPAddress.Any;

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

/// <summary>
/// 反向代理服务的设置项可序列化传输类型
/// </summary>
[MP2Obj(SerializeLayout.Explicit)]
public sealed partial class ReverseProxySettings : IReverseProxySettings
{
    [MP2Key(0)]
    public IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }

    [MP2Key(1)]
    public IReadOnlyCollection<ScriptDTO>? Scripts { get; set; }

    [MP2Key(2)]
    public bool IsEnableScript { get; set; }

    [MP2Key(3)]
    public bool IsOnlyWorkSteamBrowser { get; set; }

    [MP2Key(4)]
    public int ProxyPort { get; set; }

    [MP2Key(5)]
    [MemoryPackAllowSerialize]
    [MessagePackFormatter(typeof(IPAddressFormatter))]
    public IPAddress ProxyIp { get; set; } = IReverseProxySettings.DefaultProxyIp;

    [MP2Key(6)]
    public ProxyMode ProxyMode { get; set; }

    [MP2Key(7)]
    public bool IsProxyGOG { get; set; }

    [MP2Key(8)]
    public bool OnlyEnableProxyScript { get; set; }

    [MP2Key(9)]
    public bool EnableHttpProxyToHttps { get; set; }

    [MP2Key(10)]
    public bool Socks5ProxyEnable { get; set; }

    [MP2Key(11)]
    public int Socks5ProxyPortId { get; set; }

    [MP2Key(12)]
    public bool TwoLevelAgentEnable { get; set; }

    [MP2Key(13)]
    public ExternalProxyType TwoLevelAgentProxyType { get; set; }

    [MP2Key(14)]
    public string? TwoLevelAgentIp { get; set; }

    [MP2Key(15)]
    public int TwoLevelAgentPortId { get; set; }

    [MP2Key(16)]
    public string? TwoLevelAgentUserName { get; set; }

    [MP2Key(17)]
    public string? TwoLevelAgentPassword { get; set; }

    [MP2Key(18)]
    [MemoryPackAllowSerialize]
    [MessagePackFormatter(typeof(IPAddressFormatter))]
    public IPAddress? ProxyDNS { get; set; }
}