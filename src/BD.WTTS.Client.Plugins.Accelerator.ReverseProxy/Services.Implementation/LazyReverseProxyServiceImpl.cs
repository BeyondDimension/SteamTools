namespace BD.WTTS.Services.Implementation;

sealed class LazyReverseProxyServiceImpl : IReverseProxyService
{
    public static IReverseProxyService Instance = new LazyReverseProxyServiceImpl();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable IDE1006 // 命名样式
    static IReverseProxyService instance() => Ioc.Get<IReverseProxyService>();
#pragma warning restore IDE1006 // 命名样式

    public ICertificateManager CertificateManager => instance().CertificateManager;

    public bool ProxyRunning => instance().ProxyRunning;

    public IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get => instance().ProxyDomains; set => instance().ProxyDomains = value; }

    public IReadOnlyCollection<ScriptDTO>? Scripts { get => instance().Scripts; set => instance().Scripts = value; }

    public bool IsEnableScript { get => instance().IsEnableScript; set => instance().IsEnableScript = value; }

    public bool IsOnlyWorkSteamBrowser { get => instance().IsOnlyWorkSteamBrowser; set => instance().IsOnlyWorkSteamBrowser = value; }

    public int ProxyPort { get => instance().ProxyPort; set => instance().ProxyPort = value; }

    public IPAddress ProxyIp { get => instance().ProxyIp; set => instance().ProxyIp = value; }

    public ProxyMode ProxyMode { get => instance().ProxyMode; set => instance().ProxyMode = value; }

    public bool IsProxyGOG { get => instance().IsProxyGOG; set => instance().IsProxyGOG = value; }

    public bool OnlyEnableProxyScript { get => instance().OnlyEnableProxyScript; set => instance().OnlyEnableProxyScript = value; }

    public bool EnableHttpProxyToHttps { get => instance().EnableHttpProxyToHttps; set => instance().EnableHttpProxyToHttps = value; }

    public bool Socks5ProxyEnable { get => instance().Socks5ProxyEnable; set => instance().Socks5ProxyEnable = value; }

    public int Socks5ProxyPortId { get => instance().Socks5ProxyPortId; set => instance().Socks5ProxyPortId = value; }

    public bool TwoLevelAgentEnable { get => instance().TwoLevelAgentEnable; set => instance().TwoLevelAgentEnable = value; }

    public ExternalProxyType TwoLevelAgentProxyType { get => instance().TwoLevelAgentProxyType; set => instance().TwoLevelAgentProxyType = value; }

    public string? TwoLevelAgentIp { get => instance().TwoLevelAgentIp; set => instance().TwoLevelAgentIp = value; }

    public int TwoLevelAgentPortId { get => instance().TwoLevelAgentPortId; set => instance().TwoLevelAgentPortId = value; }

    public string? TwoLevelAgentUserName { get => instance().TwoLevelAgentUserName; set => instance().TwoLevelAgentUserName = value; }

    public string? TwoLevelAgentPassword { get => instance().TwoLevelAgentPassword; set => instance().TwoLevelAgentPassword = value; }

    public IPAddress? ProxyDNS { get => instance().ProxyDNS; set => instance().ProxyDNS = value; }

    public void Dispose() => instance().Dispose();

    public FlowStatistics? GetFlowStatistics() => instance().GetFlowStatistics();

    public ValueTask<bool> StartProxyAsync() => instance().StartProxyAsync();

    public ValueTask StopProxyAsync() => instance().StopProxyAsync();

    public bool WirtePemCertificateToGoGSteamPlugins() => instance().WirtePemCertificateToGoGSteamPlugins();
}