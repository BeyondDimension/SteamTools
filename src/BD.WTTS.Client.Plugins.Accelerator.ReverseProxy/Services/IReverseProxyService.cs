// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IReverseProxyService
{
    ICertificateManager CertificateManager { get; }

    /// <summary>
    /// 将 PEM 证书公钥写入 GOG GALAXY
    /// </summary>
    /// <returns></returns>
    bool WirtePemCertificateToGoGSteamPlugins();

    /// <summary>
    /// 获取流量统计信息
    /// </summary>
    /// <returns></returns>
    FlowStatistics? GetFlowStatistics();
}