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
    protected const string TAG = "ReverseProxyS";

    static IReverseProxyService Instance => DI.Get<IReverseProxyService>();
}
