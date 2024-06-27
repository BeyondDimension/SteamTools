// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

abstract class ReverseProxyServiceImpl : IReverseProxySettings
{
    protected const string TAG = "ReverseProxyS";

    public ReverseProxyServiceImpl(
        DnsAnalysisServiceImpl dnsAnalysisServiceImpl,
        DnsDohAnalysisService dnsDohAnalysisService)
    {
        DnsAnalysis = new DnsAnalysisServiceSwitchImpl(this, dnsAnalysisServiceImpl, dnsDohAnalysisService);
    }

    public IDnsAnalysisService DnsAnalysis { get; }

    public abstract CertificateManagerImpl CertificateManager { get; }

    /// <summary>
    /// 获取或设置当前根证书
    /// </summary>
    public X509Certificate2? RootCertificate
    {
        get => CertificateManager.RootCertificatePackable;
    }

    /// <inheritdoc cref="IReverseProxyService.ProxyRunning"/>
    public abstract bool ProxyRunning { get; }

    /// <inheritdoc cref="IReverseProxyService.ProxyDomains"/>
    public IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }

    /// <inheritdoc cref="IReverseProxyService.Scripts"/>
    public IReadOnlyCollection<ScriptIPCDTO>? Scripts { get; set; }

    /// <inheritdoc cref="IReverseProxyService.IsEnableScript"/>
    public bool IsEnableScript { get; set; }

    /// <inheritdoc cref="IReverseProxyService.IsOnlyWorkSteamBrowser"/>
    public bool IsOnlyWorkSteamBrowser { get; set; }

    public const ushort DefaultProxyPort = 26501;

    /// <inheritdoc cref="IReverseProxyService.ProxyPort"/>
    public ushort ProxyPort { get; set; } = DefaultProxyPort;

    /// <inheritdoc cref="IReverseProxyService.ProxyIp"/>
    public IPAddress ProxyIp { get; set; } = IReverseProxyService.Constants.DefaultProxyIp;

    /// <inheritdoc cref="IReverseProxyService.ProxyMode"/>
    public ProxyMode ProxyMode { get; set; }

    /// <inheritdoc cref="IReverseProxyService.IsProxyGOG"/>
    public bool IsProxyGOG { get; set; }

    /// <inheritdoc cref="IReverseProxyService.OnlyEnableProxyScript"/>
    public bool OnlyEnableProxyScript { get; set; }

    /// <inheritdoc cref="IReverseProxyService.EnableHttpProxyToHttps"/>
    public bool EnableHttpProxyToHttps { get; set; }

    // Socks5

    /// <inheritdoc cref="IReverseProxyService.Socks5ProxyEnable"/>
    public bool Socks5ProxyEnable { get; set; }

    /// <inheritdoc cref="IReverseProxyService.Socks5ProxyPortId"/>
    public ushort Socks5ProxyPortId { get; set; }

    // TwoLevelAgent(二级代理)

    public bool TwoLevelAgentEnable { get; set; }

    public ExternalProxyType TwoLevelAgentProxyType { get; set; } = IReverseProxyService.Constants.DefaultTwoLevelAgentProxyType;

    public string? TwoLevelAgentIp { get; set; }

    public ushort TwoLevelAgentPortId { get; set; }

    public string? TwoLevelAgentUserName { get; set; }

    public string? TwoLevelAgentPassword { get; set; }

    public IPAddress? ProxyDNS { get; set; }

    public bool IsSupportIpv6 { get; set; }

    public bool UseDoh { get; set; }

    public string? CustomDohAddres { get; set; }

    public string? ServerSideProxyToken { get; set; }

    /// <summary>
    /// 获取一个随机的未使用的端口
    /// </summary>
    /// <returns></returns>
    protected int GetRandomUnusedPort() => SocketHelper.GetRandomUnusedPort(ProxyIp);

    /// <inheritdoc cref="IReverseProxyService.WirtePemCertificateToGoGSteamPlugins"/>
    public bool WirtePemCertificateToGoGSteamPlugins()
    {
        /* https://www.gog.com/galaxy
         * GOG GALAXY 2.0 公测需要 Windows 8 或更新版本。
         * 也同时支持 Mac OS X。
         * OSX 也是这个路径？？？？
         * https://snapcraft.io/gog-galaxy-wine
         * 作废
         */
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var gogPlugins = Path.Combine(local, "GOG.com", "Galaxy", "plugins", "installed");
        if (Directory.Exists(gogPlugins))
        {
            foreach (var dir in Directory.GetDirectories(gogPlugins))
            {
                if (dir.Contains("steam"))
                {
                    var pem = RootCertificate!.GetPublicPemCertificateString();
                    var certifi = Path.Combine(local, dir, "certifi", "cacert.pem");
                    if (File.Exists(certifi))
                    {
                        var file = File.ReadAllText(certifi);
                        var s = file.Substring(Constants.CERTIFICATE_TAG, Constants.CERTIFICATE_TAG, true);
                        if (string.IsNullOrEmpty(s))
                        {
                            File.AppendAllText(certifi, Environment.NewLine + pem);
                        }
                        else if (s.Trim() != pem.Trim())
                        {
                            var index = file.IndexOf(Constants.CERTIFICATE_TAG);
                            File.WriteAllText(certifi, file.Remove(index, s.Length) + pem);
                        }
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 自定义异常纪录处理
    /// </summary>
    /// <param name="exception"></param>
    protected virtual void OnException(Exception exception)
    {
        Log.Error(TAG, exception, "OnException");
    }

    public async Task<StartProxyResult> StartProxyAsync(byte[] reverseProxySettings_)
    {
        ReverseProxySettings reverseProxySettings;
        try
        {
            reverseProxySettings = Serializable.DMP2<ReverseProxySettings>(reverseProxySettings_);
        }
        catch
        {
            return StartProxyResultCode.DeserializeReverseProxySettingsFail;
        }

        // Linux 如果是443 需要验证 是否允许使用
        if (OperatingSystem.IsLinux())
        {
            if (string.IsNullOrWhiteSpace(reverseProxySettings.ProxyIp))
                return StartProxyResultCode.Exception;
            if (reverseProxySettings.ProxyMode == ProxyMode.Hosts)
            {
                var inUsePort = SocketHelper.IsUsePort(IPAddress.Parse(reverseProxySettings.ProxyIp!), 443);
                if (inUsePort)
                {
                    return StartProxyResultCode.BindPortError;
                }
            }
        }

        CheckRootCertificate();

        reverseProxySettings.SetValue(this);

        return await StartProxyImpl();
    }

    protected virtual void CheckRootCertificate()
    {
    }

    protected abstract Task<StartProxyResult> StartProxyImpl();

    // IDisposable

    protected abstract void DisposeCore();

    bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                DisposeCore();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}