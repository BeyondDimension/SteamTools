namespace BD.WTTS.Services.Implementation;

sealed class IPCReverseProxyServiceImpl : IReverseProxyService
{
    public IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IReadOnlyCollection<ScriptDTO>? Scripts { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool IsEnableScript { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool IsOnlyWorkSteamBrowser { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public int ProxyPort { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IPAddress ProxyIp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public ProxyMode ProxyMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool IsProxyGOG { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool OnlyEnableProxyScript { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool EnableHttpProxyToHttps { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool Socks5ProxyEnable { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public int Socks5ProxyPortId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool TwoLevelAgentEnable { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public ExternalProxyType TwoLevelAgentProxyType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string? TwoLevelAgentIp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public int TwoLevelAgentPortId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string? TwoLevelAgentUserName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string? TwoLevelAgentPassword { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IPAddress? ProxyDNS { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool ProxyRunning => throw new NotImplementedException();

    public ReverseProxyEngine ReverseProxyEngine => throw new NotImplementedException();

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> StartProxyAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask StopProxyAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask SyncSettingsAsync()
    {
        throw new NotImplementedException();
    }
}
