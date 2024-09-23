using Ae.Dns.Client;
using Ae.Dns.Protocol;
using Ae.Dns.Protocol.Records;
using static BD.WTTS.Services.IDnsAnalysisService;
using DnsQueryType = Ae.Dns.Protocol.Enums.DnsQueryType;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class DnsDohAnalysisService : GeneralHttpClientFactory
{
    const string TAG = "DnsDohAnalysisS";

    protected override string? DefaultClientName => TAG;

    readonly ConcurrentDictionary<(Uri dohAddresUri, string hostNameOrAddressWithQueryType), IPAddressResult> Cache = new();
    readonly ConcurrentDictionary<Uri, DnsHttpClient> dnsClients = new();

    public DnsDohAnalysisService(
            ILoggerFactory loggerFactory,
            IHttpPlatformHelperService http_helper,
            IHttpClientFactory clientFactory)
            : base(loggerFactory.CreateLogger(TAG), http_helper, clientFactory)
    {
    }

    public const string DefaultDohAddres = Dnspod_DohAddres2;

    static Uri GetDohAddres(string? dohAddres)
    {
        if (string.IsNullOrWhiteSpace(dohAddres))
            return new Uri(DefaultDohAddres);

        Uri dohAddresUri;
        try
        {
            dohAddresUri = new Uri(dohAddres);
        }
        catch
        {
            dohAddresUri = new Uri(DefaultDohAddres);
        }
        return dohAddresUri;
    }

    DnsHttpClient GetDnsHttpClient(string? dohAddres)
    {
        var dohAddresUri = GetDohAddres(dohAddres);
        return GetDnsHttpClient(dohAddresUri);
    }

    DnsHttpClient GetDnsHttpClient(Uri dohAddresUri)
    {
        if (dnsClients.TryGetValue(dohAddresUri, out var value))
            return value;
        //var client = CreateClient($"{TAG}_{dohAddresUri}", HttpHandlerCategory.Default);
        var handler = new HttpClientHandler
        {
            UseCookies = false,
            UseProxy = false,
            Proxy = HttpNoProxy.Instance,
        };
        var client = new HttpClient(handler);
        client.BaseAddress = dohAddresUri;
        var dnsClient = new DnsHttpClient(client);
        //暂未考虑释放问题 注意！
        //foreach (var dc in dnsClients.Values)
        //{
        //    dc.Dispose();
        //}
        //dnsClients.Clear();
        dnsClients.TryAdd(dohAddresUri, dnsClient);
        return dnsClient;
    }

    public async Task<int> DohAnalysisHostnameTimeAsync(string? dohAddres, string url, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(url))
        {
            var dnsClient = GetDnsHttpClient(dohAddres);

            var queryType = DnsQueryType.A;
            var query = DnsQueryFactory.CreateQuery(url, queryType);

            var answer = await dnsClient.Query(query, cancellationToken);
            var value = answer.Answers.FirstOrDefault()?.TimeToLive;
            return (int)(value ?? 0);
        }
        return 0;
    }

    ///// <summary>
    ///// DNS 解析查询最大递归查询次数
    ///// </summary>
    //const byte max_recursion_dns_query = 20;

    static readonly TimeSpan TTL = TimeSpan.FromMinutes(9.9D);

    /// <summary>
    /// DNS 解析查询
    /// </summary>
    /// <param name="dohAddresUri">DoH 地址</param>
    /// <param name="hostNameOrAddress">解析的地址</param>
    /// <param name="queryType">查询类型</param>
    /// <param name="count">当前递归的次数</param>
    /// <param name="domainHistory">返回的历史域名，避免死循环</param>
    /// <param name="cancellationToken">取消标记</param>
    /// <returns></returns>
    async Task<IPAddress[]> Query(
        Uri dohAddresUri,
        string hostNameOrAddress,
        DnsQueryType queryType = DnsQueryType.A,
        //byte count = 1,
        //HashSet<string>? domainHistory = null,
        CancellationToken cancellationToken = default)
    {
        //if (count <= max_recursion_dns_query)
        //{
        //    domainHistory ??= new();
        var query = DnsQueryFactory.CreateQuery(hostNameOrAddress, queryType);
        var dnsClient = GetDnsHttpClient(dohAddresUri);
        var answer = await dnsClient.Query(query, cancellationToken);

        var dnsResources = answer.Answers.Select(x => x.Resource);

        var iPAddresses = dnsResources.OfType<DnsIpAddressResource>().Select(x => x.IPAddress).ToArray();
        if (iPAddresses.Any())
            return iPAddresses;

        //var domainRes = dnsResources.OfType<DnsDomainResource>().FirstOrDefault();
        //if (domainRes != null)
        //{
        //    var domain = domainRes.Domain;
        //    if (!string.IsNullOrWhiteSpace(domain))
        //    {
        //        //if (domainHistory.Add(domain))
        //        //{
        //        // CNAME 递归解析
        //        iPAddresses = await Query(dohAddresUri, domain, queryType,/* ++count, domainHistory,*/ cancellationToken);
        //        //}
        //    }
        //}
        ////}

        return Array.Empty<IPAddress>();
    }

    readonly record struct IPAddressResult
    {
        public IPAddress[] IPAddresses { get; init; }

        public DateTimeOffset CreationTime { get; init; }
    }

    /// <summary>
    /// DOH 解析域名 IP 地址
    /// </summary>
    /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
    /// <param name="isIPv6"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<IPAddress> DohAnalysisDomainIpAsync(string? dohAddres, string hostNameOrAddress, bool isIPv6, bool useCache = true, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryType = isIPv6 ? DnsQueryType.AAAA : DnsQueryType.A;
        var dohAddresUri = GetDohAddres(dohAddres);

        (Uri dohAddresUri, string hostNameOrAddressWithQueryType) cacheKey = useCache ? GetCacheKey() : default;
        (Uri dohAddresUri, string hostNameOrAddressWithQueryType) GetCacheKey()
        {
            var hostNameOrAddressWithQueryType = string.Concat(hostNameOrAddress, ":", (short)queryType);
            var cacheKey = (dohAddresUri, hostNameOrAddressWithQueryType);
            return cacheKey;
        }

        if (useCache)
        {
            if (Cache.TryGetValue(cacheKey, out var dns_list))
            {
                if ((DateTimeOffset.Now - dns_list.CreationTime) <= TTL)
                {
                    foreach (var item in dns_list.IPAddresses)
                    {
                        yield return item;
                    }
                }
            }
        }

        var result = await Query(dohAddresUri, hostNameOrAddress, queryType, cancellationToken: cancellationToken);

        if (isIPv6 && !result.Any())
        {
            // IPv6 返回空时使用 IPv4 解析
            result = await Query(dohAddresUri, hostNameOrAddress, DnsQueryType.A, cancellationToken: cancellationToken);
        }

        if (useCache)
        {
            Cache[cacheKey] = new()
            {
                IPAddresses = result,
                CreationTime = DateTimeOffset.Now,
            };
        }

        foreach (var item in result)
        {
            yield return item;
        }
    }
}