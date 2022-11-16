using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Http;

namespace System.Application.Services;

public sealed class DnsDohAnalysisService : IDnsDohAnalysisService
{
    readonly IHttpService httpService;

    public DnsDohAnalysisService(IHttpService httpService)
    {
        this.httpService = httpService;
    }

    /// <summary>
    /// 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="isIPv6"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, bool isIPv6, CancellationToken cancellationToken = default)
        => AnalysisDomainIpAsync(hostNameOrAddress, default, isIPv6, cancellationToken);

    /// <summary>
    /// 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, CancellationToken cancellationToken = default)
        => AnalysisDomainIpAsync(hostNameOrAddress, default, default, cancellationToken);

    /// <summary>
    /// 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="dnsServers">自定义 DNS 服务器，可选的值有 <see cref="DNS_Alis"/>, <see cref="DNS_114s"/>, <see cref="DNS_Cloudflares"/>, <see cref="DNS_Dnspods"/>, <see cref="DNS_Googles"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, string? dnsServers, CancellationToken cancellationToken = default)
        => AnalysisDomainIpAsync(hostNameOrAddress, dnsServers, default, cancellationToken);

    /// <summary>
    /// 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="dnsServers">自定义 DNS 服务器，可选的值有 <see cref="DNS_Alis"/>, <see cref="DNS_114s"/>, <see cref="DNS_Cloudflares"/>, <see cref="DNS_Dnspods"/>, <see cref="DNS_Googles"/></param>
    /// <param name="isIPv6"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, string? dnsServers, bool isIPv6, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield return IPAddress.Any;
    }
}
