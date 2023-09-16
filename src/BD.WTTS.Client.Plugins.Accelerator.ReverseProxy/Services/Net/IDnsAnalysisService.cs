using System.Net.NetworkInformation;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IDnsAnalysisService // 服务函数与 protected 定义
{
    protected const string TAG = "DnsAnalysisS";

    const string IPV6_TESTDOMAIN = "ipv6.rmbgame.net";
    const string IPV6_TESTDOMAIN_SUCCESS = PrimaryDNS_IPV6_Ali;

    async Task<long> PingHostnameAsync(string url)
    {
        var pin = new Ping();
        var r = await pin.SendPingAsync(url, 30);
        if (r.Status != IPStatus.Success)
        {
            return 0;
        }
        return r.RoundtripTime;
    }

    Task<int> AnalysisHostnameTimeAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="isIPv6"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, bool isIPv6, CancellationToken cancellationToken = default)
        => AnalysisDomainIpAsync(hostNameOrAddress, default, isIPv6, cancellationToken);

    /// <summary>
    /// 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, CancellationToken cancellationToken = default)
        => AnalysisDomainIpAsync(hostNameOrAddress, default, default, cancellationToken);

    /// <summary>
    /// 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="dnsServers">自定义 DNS 服务器，可选的值有 <see cref="DNS_Alis"/>, <see cref="DNS_114s"/>, <see cref="DNS_Cloudflares"/>, <see cref="DNS_Dnspods"/>, <see cref="DNS_Googles"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, IPAddress[]? dnsServers, CancellationToken cancellationToken = default)
        => AnalysisDomainIpAsync(hostNameOrAddress, dnsServers, default, cancellationToken);

    /// <summary>
    /// 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="dnsServers">自定义 DNS 服务器，可选的值有 <see cref="DNS_Alis"/>, <see cref="DNS_114s"/>, <see cref="DNS_Cloudflares"/>, <see cref="DNS_Dnspods"/>, <see cref="DNS_Googles"/></param>
    /// <param name="isIPv6"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    async IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, IPAddress[]? dnsServers, bool isIPv6, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var items = await
#if NET6_0_OR_GREATER
            Dns.GetHostAddressesAsync(hostNameOrAddress, cancellationToken);
#else
            Dns.GetHostAddressesAsync(hostNameOrAddress);
#endif
        foreach (var item in items)
        {
            yield return item;
        }
    }

    async Task<string?> GetHostByIPAddressAsync(IPAddress ip)
    {
        var hostEntry = await Dns.GetHostEntryAsync(ip);
        return hostEntry.HostName;
    }

    Task<bool> GetIsIpv6SupportAsync() => Task.FromResult(false);

    Task<IPAddress?> GetHostIpv6AddresAsync() => Task.FromResult(default(IPAddress));
}