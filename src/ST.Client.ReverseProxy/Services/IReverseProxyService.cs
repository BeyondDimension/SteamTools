using System.Net;
using System.Properties;
using System.Security.Cryptography.X509Certificates;
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
    const string RootCertificateIssuerName = $"{CertificateName} Certificate Authority";
    const string LocalDomain = "local.steampp.net";
    const string TAG = "ReverseProxyS";

    static IReverseProxyService Instance => DI.Get<IReverseProxyService>();

    bool IsCertificate { get; }

    void TrustCer();

    IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }

    IReadOnlyCollection<ScriptDTO>? Scripts { get; set; }

    bool IsEnableScript { get; set; }

    bool IsOnlyWorkSteamBrowser { get; set; }

    ECertificateEngine CertificateEngine { get; set; }

    int ProxyPort { get; set; }

    IPAddress ProxyIp { get; set; }

    bool IsSystemProxy { get; set; }

    bool IsProxyGOG { get; set; }

    bool OnlyEnableProxyScript { get; set; }

    bool Socks5ProxyEnable { get; set; }

    bool EnableHttpProxyToHttps { get; set; }

    int Socks5ProxyPortId { get; set; }

    bool TwoLevelAgentEnable { get; set; }

    EExternalProxyType TwoLevelAgentProxyType { get; set; }

    const EExternalProxyType DefaultTwoLevelAgentProxyType = EExternalProxyType.Socks5;

    string? TwoLevelAgentIp { get; set; }

    int TwoLevelAgentPortId { get; set; }

    string? TwoLevelAgentUserName { get; set; }

    string? TwoLevelAgentPassword { get; set; }

    IPAddress? ProxyDNS { get; set; }

    bool ProxyRunning { get; }

    bool SetupCertificate();

    bool DeleteCertificate();

    bool PortInUse(int port);

    Task<bool> StartProxy();

    void StopProxy();

    bool WirtePemCertificateToGoGSteamPlugins();

    bool IsCertificateInstalled(X509Certificate2? certificate2);

    const string PfxFileName = $"{CertificateName}.Certificate{FileEx.PFX}";

    const string CerFileName = $"{CertificateName}.Certificate{FileEx.CER}";

    static string CerExportFileName
    {
        get
        {
            var now = DateTime.Now;
            const string f = $"{ThisAssembly.AssemblyTrademark}  Certificate {{0}}{FileEx.CER}";
            return string.Format(f, now.ToString(DateTimeFormat.File));
        }
    }

    static string DefaultPfxFilePath => Path.Combine(IOPath.AppDataDirectory, PfxFileName);

    static string DefaultCerFilePath => Path.Combine(IOPath.AppDataDirectory, CerFileName);

    string PfxFilePath => DefaultPfxFilePath;

    string CerFilePath => DefaultCerFilePath;

    /// <summary>
    /// 获取 Cer 证书路径，当不存在时生成文件后返回路径
    /// </summary>
    /// <returns></returns>
    string? GetCerFilePathGeneratedWhenNoFileExists();

    bool IsCurrentCertificateInstalled { get; }

    /// <summary>
    /// 获取当前 Root 证书
    /// </summary>
    X509Certificate2? RootCertificate { get; }
}
