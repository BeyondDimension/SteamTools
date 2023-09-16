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

    readonly ConcurrentDictionary<(Uri dohAddresUri, string hostNameOrAddressWithQueryType), List<DnsResourceRecord>> Cache = new();
    readonly ConcurrentDictionary<Uri, DnsHttpClient> dnsClients = new();

    public DnsDohAnalysisService(
            ILoggerFactory loggerFactory,
            IHttpPlatformHelperService http_helper,
            IHttpClientFactory clientFactory)
            : base(loggerFactory.CreateLogger(TAG), http_helper, clientFactory)
    {
    }

    public const string DefaultDohAddres = Dnspod_DohAddres;

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
        var client = CreateClient($"{TAG}_{dohAddresUri}", HttpHandlerCategory.Default);
        //var handler = new HttpClientHandler
        //{
        //    UseCookies = false,
        //    UseProxy = false,
        //    Proxy = HttpNoProxy.Instance,
        //};
        //var client = new HttpClient(handler);
        client.BaseAddress = dohAddresUri;
        var dnsClient = new DnsHttpClient(client);
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

        var current_dns_list = new List<DnsResourceRecord>();
        if (useCache)
        {
            if (Cache.TryGetValue(cacheKey, out var dns_list))
            {
                current_dns_list = dns_list.Where(x => x.TimeToLive > DateTime.Now.ToUnixTimeSeconds()).ToList();
                Cache[cacheKey] = current_dns_list;
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
            try
            {
                var query = DnsQueryFactory.CreateQuery(hostNameOrAddress, queryType);
                var dnsClient = GetDnsHttpClient(dohAddresUri);

                var answer = await dnsClient.Query(query, cancellationToken);
                if (useCache)
                {
                    current_dns_list.AddRange(answer.Answers);
                }
                foreach (var x in answer.Answers)
                {
                    if (x.Resource is DnsIpAddressResource ipAddressResource)
                    {
                        yield return ipAddressResource.IPAddress;
                    }
                }
            }
            finally
            {
                if (useCache)
                {
                    current_dns_list.ForEach(item =>
                    {
                        item.TimeToLive += (uint)DateTime.Now.ToUnixTimeSeconds();
                    });
                    Cache[cacheKey] = current_dns_list;
                }
            }
        }
    }
}