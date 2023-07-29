// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.DomainResolve/IDomainResolver.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 域名解析器
/// </summary>
public interface IDomainResolver
{
    /// <summary>
    /// 解析域名
    /// </summary>
    /// <param name="endPoint"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<IPAddress> ResolveAsync(DnsEndPoint endPoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查 IPV6 网络支持
    /// </summary>
    /// <param name="cancellationToken"></param>
    void CheckIpv6SupportAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 对所有节点进行测速
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task TestSpeedAsync(CancellationToken cancellationToken = default);
}