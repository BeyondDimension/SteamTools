using dotnetCampus.Ipc.CompilerServices.Attributes;

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
    [IpcMethod(Timeout = 2000)]
    byte[]? GetFlowStatistics_Bytes();

    /// <summary>Read only new log bytes, or replace the view if the stream restarted or the cursor expired.</summary>
    [IpcMethod(Timeout = 2000)]
    Task<ProxyLogUpdate?> GetLogUpdateAsync(Guid streamId, long offset);
}

/// <summary>A bounded UTF-8 log batch and the next cursor, with null used for an unchanged stream.</summary>
public sealed record ProxyLogUpdate(Guid StreamId, long End, bool Reset, byte[] Utf8);

public static partial class ReverseProxyServiceExtensions
{
    /// <inheritdoc cref="IReverseProxyService.GetFlowStatistics_Bytes"/>
    public static FlowStatistics? GetFlowStatistics(this IReverseProxyService s)
    {
        var bytes = s.GetFlowStatistics_Bytes();
        if (bytes == default) return default;
        var flowStatistics = MemoryPackSerializer.Deserialize<FlowStatistics>(bytes);
        return flowStatistics;
    }
}