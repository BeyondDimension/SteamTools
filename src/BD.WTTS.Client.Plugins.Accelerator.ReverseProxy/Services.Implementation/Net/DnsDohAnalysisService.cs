#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using Ae.Dns.Client;
using Ae.Dns.Protocol;
using Ae.Dns.Protocol.Records;
using static BD.WTTS.Services.IDnsAnalysisService;
using DnsQueryType = Ae.Dns.Protocol.Enums.DnsQueryType;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

public sealed class DnsDohAnalysisService : GeneralHttpClientFactory, IDnsAnalysisService
{
    const string TAG = "DnsDohAnalysisS";

    protected override string? DefaultClientName => TAG;

    public DnsDohAnalysisService(
            ILoggerFactory loggerFactory,
            IHttpPlatformHelperService http_helper,
            IHttpClientFactory clientFactory)
            : base(loggerFactory.CreateLogger(TAG), http_helper, clientFactory)
    {
    }

    public async Task<int> AnalysisHostnameTimeAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(url))
        {
            var client = CreateClient();
            client.BaseAddress = new Uri(Dnspod_DohAddres);
            IDnsClient dnsClient = new DnsHttpClient(client);

            var queryType = DnsQueryType.A;
            var query = DnsQueryFactory.CreateQuery(url, queryType);

            var answer = await dnsClient.Query(query, cancellationToken);
            var value = answer.Answers.FirstOrDefault()?.TimeToLive;
            return (int)(value ?? 0);
        }
        return 0;
    }

    /// <summary>
    /// 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="isIPv6"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, bool isIPv6, CancellationToken cancellationToken = default)
        => DohAnalysisDomainIpAsync(hostNameOrAddress, default, isIPv6, cancellationToken);

    /// <summary>
    /// 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, CancellationToken cancellationToken = default)
        => DohAnalysisDomainIpAsync(hostNameOrAddress, default, default, cancellationToken);

    /// <summary>
    /// 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="dnsServers">自定义 DNS 服务器，可选的值有 <see cref="DNS_Alis"/>, <see cref="DNS_114s"/>, <see cref="DNS_Cloudflares"/>, <see cref="DNS_Dnspods"/>, <see cref="DNS_Googles"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, string? dnsServers, CancellationToken cancellationToken = default)
        => DohAnalysisDomainIpAsync(hostNameOrAddress, dnsServers, default, cancellationToken);

    /// <summary>
    /// DOH 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="dnsServers">自定义 DNS 服务器，可选的值有 <see cref="DNS_Alis"/>, <see cref="DNS_114s"/>, <see cref="DNS_Cloudflares"/>, <see cref="DNS_Dnspods"/>, <see cref="DNS_Googles"/></param>
    /// <param name="isIPv6"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<IPAddress> DohAnalysisDomainIpAsync(string hostNameOrAddress, string? dnsServers, bool isIPv6, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        client.BaseAddress = new Uri(string.IsNullOrEmpty(dnsServers) ? Dnspod_DohAddres : dnsServers);
        IDnsClient dnsClient = new DnsHttpClient(client);

        var queryType = isIPv6 ? DnsQueryType.AAAA : DnsQueryType.A;
        var query = DnsQueryFactory.CreateQuery(hostNameOrAddress, queryType);

        var answer = await dnsClient.Query(query, cancellationToken);

        foreach (var x in answer.Answers)
        {
            if (x.Resource is DnsIpAddressResource ipAddressResource)
            {
                yield return ipAddressResource.IPAddress;
            }
        }
    }
}
#endif