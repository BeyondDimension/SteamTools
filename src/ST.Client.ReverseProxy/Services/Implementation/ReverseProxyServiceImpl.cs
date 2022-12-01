using System.Application.Models;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using static System.Application.Services.IReverseProxyService;

namespace System.Application.Services.Implementation;

abstract class ReverseProxyServiceImpl
{
    protected readonly IPlatformService platformService;

    public ReverseProxyServiceImpl(
        IPlatformService platformService,
        IDnsAnalysisService dnsAnalyses)
    {
        this.platformService = platformService;
        DnsAnalysis = dnsAnalyses;
    }

    public IDnsAnalysisService? DnsAnalysis { get; }

    public abstract ICertificateManager CertificateManager { get; }

    /// <summary>
    /// 获取或设置当前根证书
    /// </summary>
    public X509Certificate2? RootCertificate
    {
        get => CertificateManager.RootCertificate;
        set => CertificateManager.RootCertificate = value;
    }

    /// <inheritdoc cref="IReverseProxyService.ProxyRunning"/>
    public abstract bool ProxyRunning { get; }

    /// <inheritdoc cref="IReverseProxyService.ProxyDomains"/>
    public IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }

    /// <inheritdoc cref="IReverseProxyService.Scripts"/>
    public IReadOnlyCollection<ScriptDTO>? Scripts { get; set; }

    /// <inheritdoc cref="IReverseProxyService.IsEnableScript"/>
    public bool IsEnableScript { get; set; }

    /// <inheritdoc cref="IReverseProxyService.IsOnlyWorkSteamBrowser"/>
    public bool IsOnlyWorkSteamBrowser { get; set; }

    /// <inheritdoc cref="IReverseProxyService.ProxyPort"/>
    public int ProxyPort { get; set; } = 26501;

    /// <inheritdoc cref="IReverseProxyService.ProxyIp"/>
    public IPAddress ProxyIp { get; set; } = IPAddress.Any;

    /// <inheritdoc cref="IReverseProxyService.ProxyMode"/>
    public ProxyMode ProxyMode { get; set; }

    /// <inheritdoc cref="IReverseProxyService.IsProxyGOG"/>
    public bool IsProxyGOG { get; set; }

    /// <inheritdoc cref="IReverseProxyService.OnlyEnableProxyScript"/>
    public bool OnlyEnableProxyScript { get; set; }

    /// <inheritdoc cref="IReverseProxyService.EnableHttpProxyToHttps"/>
    public bool EnableHttpProxyToHttps { get; set; }

    #region Socks5

    /// <inheritdoc cref="IReverseProxyService.Socks5ProxyEnable"/>
    public bool Socks5ProxyEnable { get; set; }

    /// <inheritdoc cref="IReverseProxyService.Socks5ProxyPortId"/>
    public int Socks5ProxyPortId { get; set; }

    #endregion

    #region TwoLevelAgent(二级代理)

    public bool TwoLevelAgentEnable { get; set; }

    public EExternalProxyType TwoLevelAgentProxyType { get; set; } = DefaultTwoLevelAgentProxyType;

    public string? TwoLevelAgentIp { get; set; }

    public int TwoLevelAgentPortId { get; set; }

    public string? TwoLevelAgentUserName { get; set; }

    public string? TwoLevelAgentPassword { get; set; }

    #endregion

    public IPAddress? ProxyDNS { get; set; }

    /// <summary>
    /// 获取一个随机的未使用的端口
    /// </summary>
    /// <returns></returns>
    protected int GetRandomUnusedPort() => SocketHelper.GetRandomUnusedPort(ProxyIp);

    /// <inheritdoc cref="IReverseProxyService.PortInUse(int)"/>
    public bool PortInUse(int port) => SocketHelper.IsUsePort(ProxyIp, port);

    /// <inheritdoc cref="IReverseProxyService.WirtePemCertificateToGoGSteamPlugins"/>
    public bool WirtePemCertificateToGoGSteamPlugins()
    {
        /* https://www.gog.com/galaxy
         * GOG GALAXY 2.0 公测需要Windows 8或更新版本。
         * 也同时支持Mac OS X。
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
        Log.Error(TAG, exception, "ProxyServer ExceptionFunc");
    }

    public abstract EReverseProxyEngine ReverseProxyEngine { get; }

    public async ValueTask<bool> StartProxy()
    {
        if (!CertificateManager.IsRootCertificateInstalled)
        {
            //CertificateManager.DeleteRootCertificate();
            var isOk = await CertificateManager.SetupRootCertificate();
            if (!isOk)
            {
                Log.Error("StartProxy", "证书安装失败，或未信任。");
                return false;
            }
        }

        return await StartProxyImpl();
    }

    protected abstract Task<bool> StartProxyImpl();

    #region IDisposable

    protected abstract void DisposeCore();

    bool disposedValue;

    protected void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
                DisposeCore();
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}