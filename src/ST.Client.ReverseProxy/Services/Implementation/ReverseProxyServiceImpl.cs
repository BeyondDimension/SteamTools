using System.Application.Models;
using System.Net;
using System.Net.Sockets;
using static System.Application.Services.IReverseProxyService;

namespace System.Application.Services.Implementation;

public abstract class ReverseProxyServiceImpl
{
    public IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }

    public IReadOnlyCollection<ScriptDTO>? Scripts { get; set; }

    public bool IsEnableScript { get; set; }

    public bool IsOnlyWorkSteamBrowser { get; set; }

    public ECertificateEngine CertificateEngine { get; set; } = ECertificateEngine.BouncyCastle;

    public int ProxyPort { get; set; } = 26501;

    public IPAddress ProxyIp { get; set; } = IPAddress.Any;

    public bool IsSystemProxy { get; set; }

    public bool IsProxyGOG { get; set; }

    public bool OnlyEnableProxyScript { get; set; }

    public bool Socks5ProxyEnable { get; set; }

    public bool EnableHttpProxyToHttps { get; set; }

    public int Socks5ProxyPortId { get; set; }

    public bool TwoLevelAgentEnable { get; set; }

    public EExternalProxyType TwoLevelAgentProxyType { get; set; } = DefaultTwoLevelAgentProxyType;

    public string? TwoLevelAgentIp { get; set; }

    public int TwoLevelAgentPortId { get; set; }

    public string? TwoLevelAgentUserName { get; set; }

    public string? TwoLevelAgentPassword { get; set; }

    public IPAddress? ProxyDNS { get; set; }

    public int GetRandomUnusedPort() => SocketHelper.GetRandomUnusedPort(ProxyIp);

    public bool PortInUse(int port) => SocketHelper.IsUsePort(ProxyIp, port);

    protected bool WirtePemCertificateToGoGSteamPlugins(Func<string> getPemCertificateString)
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var gogPlugins = Path.Combine(local, "GOG.com", "Galaxy", "plugins", "installed");
        if (Directory.Exists(gogPlugins))
        {
            foreach (var dir in Directory.GetDirectories(gogPlugins))
            {
                if (dir.Contains("steam"))
                {
                    var pem = getPemCertificateString();
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

    #region IDisposable

    protected abstract void DisposeCore();

    bool disposedValue;

    void Dispose(bool disposing)
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
