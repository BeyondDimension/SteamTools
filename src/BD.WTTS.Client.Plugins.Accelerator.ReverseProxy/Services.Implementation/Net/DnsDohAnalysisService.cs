using Ae.Dns.Client;
using Ae.Dns.Protocol;
using Ae.Dns.Protocol.Records;
using static BD.WTTS.Services.IDnsAnalysisService;
using DnsQueryType = Ae.Dns.Protocol.Enums.DnsQueryType;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class DnsDohAnalysisService : GeneralHttpClientFactory, IDnsAnalysisService
{
    const string TAG = "DnsDohAnalysisS";

    protected override string? DefaultClientName => TAG;

    private readonly ConcurrentDictionary<string, List<DnsResourceRecord>> Cache = new();

    public DnsDohAnalysisService(
            ILoggerFactory loggerFactory,
            IHttpPlatformHelperService http_helper,
            IHttpClientFactory clientFactory)
            : base(loggerFactory.CreateLogger(TAG), http_helper, clientFactory)
    {
    }

    public async Task<int> AnalysisHostnameTimeAsync(string url, CancellationToken cancellationToken = default)
    {
        const string dohAddres = Dnspod_DohAddres;
        var r = await AnalysisHostnameTimeByDohAddresAsync(dohAddres, url, cancellationToken);
        return r;
    }

    public async Task<int> AnalysisHostnameTimeByDohAddresAsync(string dohAddres, string url, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(url))
        {
            const string clientName = TAG + "_" + nameof(AnalysisHostnameTimeAsync);
            var client = CreateClient(clientName, HttpHandlerCategory.Default);
            client.BaseAddress = new Uri(dohAddres);
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
    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, bool isIPv6, bool useCache = true, CancellationToken cancellationToken = default)
        => DohAnalysisDomainIpAsync(hostNameOrAddress, default, isIPv6, useCache, cancellationToken);

    /// <summary>
    /// 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, bool useCache = true, CancellationToken cancellationToken = default)
        => DohAnalysisDomainIpAsync(hostNameOrAddress, default, default, useCache, cancellationToken);

    /// <summary>
    /// 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="dnsServers">自定义 DNS 服务器，可选的值有 <see cref="DNS_Alis"/>, <see cref="DNS_114s"/>, <see cref="DNS_Cloudflares"/>, <see cref="DNS_Dnspods"/>, <see cref="DNS_Googles"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, Uri[]? dnsServers, bool useCache = true, CancellationToken cancellationToken = default)
        => DohAnalysisDomainIpAsync(hostNameOrAddress, dnsServers, default, useCache, cancellationToken);

    /// <summary>
    /// DOH 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="dnsServers">自定义 DNS 服务器，可选的值有 <see cref="DNS_Alis"/>, <see cref="DNS_114s"/>, <see cref="DNS_Cloudflares"/>, <see cref="DNS_Dnspods"/>, <see cref="DNS_Googles"/></param>
    /// <param name="isIPv6"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<IPAddress> DohAnalysisDomainIpAsync(string hostNameOrAddress, Uri[]? dotServers, bool isIPv6, bool useCache = true, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryType = isIPv6 ? DnsQueryType.AAAA : DnsQueryType.A;

        var current_dns_list = Enumerable.Empty<DnsResourceRecord>();
        if (useCache)
        {
            var cacheKey = string.Concat(hostNameOrAddress, ":", (short)queryType);
            if (Cache.TryGetValue(cacheKey, out var dns_list))
            {
                current_dns_list = dns_list.Where(x => x.TimeToLive > DateTime.Now.ToUnixTimeSeconds()).ToList();
                Cache[cacheKey] = current_dns_list.ToList();
                foreach (var x in current_dns_list)
                {
                    if (x.Resource is DnsIpAddressResource ipAddressResource)
                    {
                        yield return ipAddressResource.IPAddress;
                    }
                }
            }
        }

        if (!current_dns_list.Any())
        {
            if (dotServers == null || dotServers.Length <= 0)
                dotServers = new Uri[] { new Uri(DNS_Ali_DohAddres) };
            try
            {
                var query = DnsQueryFactory.CreateQuery(hostNameOrAddress, queryType);

                IDnsClient dnsClient;
                HttpClient httpClient;
                foreach (var dot_server in dotServers)
                {
                    const string clientName = TAG + "_" + nameof(DohAnalysisDomainIpAsync);
                    httpClient = CreateClient(clientName, HttpHandlerCategory.Default);
                    httpClient.BaseAddress = dot_server;
                    dnsClient = new DnsHttpClient(httpClient);

                    var answer = await dnsClient.Query(query, cancellationToken);
                    if (useCache)
                        current_dns_list = current_dns_list.Concat(answer.Answers);
                    foreach (var x in answer.Answers)
                    {
                        if (x.Resource is DnsIpAddressResource ipAddressResource)
                        {
                            yield return ipAddressResource.IPAddress;
                        }
                    }
                    dnsClient.Dispose();
                }
            }
            finally
            {
                if (useCache)
                {
                    var cacheKey = string.Concat(hostNameOrAddress, ":", (short)queryType);
                    Cache[cacheKey] = current_dns_list
                        .Select(item => { item.TimeToLive += (uint)DateTime.Now.ToUnixTimeSeconds(); return item; })
                        .ToList();
                }
            }
        }
    }
}