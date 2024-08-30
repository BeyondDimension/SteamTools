// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IReverseProxyService
{
    /// <summary>
    /// 将 PEM 证书公钥写入 GOG GALAXY
    /// </summary>
    /// <returns></returns>
    [Obsolete("not supported")]
    bool WirtePemCertificateToGoGSteamPlugins();

    /// <summary>
    /// 获取流量统计信息
    /// </summary>
    /// <returns></returns>
    byte[]? GetFlowStatistics_Bytes();

    /// <summary>
    /// 获取所有日志信息
    /// </summary>
    /// <returns></returns>
    string? GetLogAllMessage();
}

public static partial class ReverseProxyServiceExtensions
{
    /// <inheritdoc cref="IReverseProxyService.GetFlowStatistics_Bytes"/>
    public static FlowStatistics? GetFlowStatistics(this IReverseProxyService s)
    {
        var bytes = s.GetFlowStatistics_Bytes();
        if (bytes == default) return default;
        var flowStatistics = Serializable.DMP2<FlowStatistics>(bytes);
        return flowStatistics;
    }
}