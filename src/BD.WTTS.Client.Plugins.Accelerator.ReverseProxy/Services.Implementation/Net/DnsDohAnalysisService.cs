using Ae.Dns.Client;
using Ae.Dns.Protocol;
using Ae.Dns.Protocol.Records;
using static BD.WTTS.Services.IDnsAnalysisService;
using DnsQueryType = Ae.Dns.Protocol.Enums.DnsQueryType;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class DnsDohAnalysisService : GeneralHttpClientFactory, IDisposable
{
    const string TAG = "DnsDohAnalysisS";

    protected override string? DefaultClientName => TAG;

    readonly ConcurrentDictionary<(Uri Server, string Host, DnsQueryType Type), IPAddressResult> Cache = new();
    readonly ConcurrentDictionary<Uri, Lazy<DnsHttpClient>> dnsClients = new();

    public DnsDohAnalysisService(
            ILoggerFactory loggerFactory,
            IHttpPlatformHelperService http_helper,
            IHttpClientFactory clientFactory)
            : base(loggerFactory.CreateLogger(TAG), http_helper, clientFactory)
    {
    }

    public void Dispose()
    {
        foreach (var client in dnsClients.Values)
            if (client.IsValueCreated)
                client.Value.Dispose();
        dnsClients.Clear();
        Cache.Clear();
    }

    public const string DefaultDohAddres = "https://doh.pub/dns-query";

    static Uri GetDohAddres(string? dohAddres)
    {
        var uri = new Uri(string.IsNullOrWhiteSpace(dohAddres) ? DefaultDohAddres : dohAddres);
        if (uri.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException("DoH endpoint must use HTTPS.", nameof(dohAddres));
        return uri;
    }

    DnsHttpClient GetDnsHttpClient(string? dohAddres) => GetDnsHttpClient(GetDohAddres(dohAddres));

    DnsHttpClient GetDnsHttpClient(Uri dohAddresUri) => dnsClients.GetOrAdd(dohAddresUri, static uri =>
        new Lazy<DnsHttpClient>(() =>
        {
            var handler = new HttpClientHandler
            {
                UseCookies = false,
                UseProxy = false,
                Proxy = HttpNoProxy.Instance,
            };
            var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(3), BaseAddress = uri };
            return new DnsHttpClient(new DnsHttpClientOptions { Path = uri.PathAndQuery }, client);
        })).Value;

    public async Task<int> DohAnalysisHostnameTimeAsync(string? dohAddres, string url, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(url))
        {
            var dnsClient = GetDnsHttpClient(dohAddres);

            var queryType = DnsQueryType.A;
            var query = DnsQueryFactory.CreateQuery(url, queryType);

            var answer = await dnsClient.Query(query, cancellationToken);
            var value = answer.Answers.FirstOrDefault(x => x.Resource is DnsIpAddressResource)?.TimeToLive;
            return (int)(value ?? 0);
        }
        return 0;
    }

    // Cache successful answers only; empty answers and failures must be retried.

    static readonly TimeSpan TTL = TimeSpan.FromMinutes(9.9D);

    /// <summary>
    /// DNS 解析查询
    /// </summary>
    /// <param name="dohAddresUri">DoH 地址</param>
    /// <param name="hostNameOrAddress">解析的地址</param>
    /// <param name="queryType">查询类型</param>
    /// <param name="cancellationToken">取消标记</param>
    /// <returns></returns>
    async Task<IPAddress[]> Query(Uri server, string host, DnsQueryType queryType, bool useCache, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = (server, host, queryType);
        if (useCache && Cache.TryGetValue(key, out var cached) && DateTimeOffset.UtcNow < cached.ExpiresAt)
            return cached.IPAddresses;

        var query = DnsQueryFactory.CreateQuery(host, queryType);
        var answer = await GetDnsHttpClient(server).Query(query, cancellationToken);
        var addresses = answer.Answers.Select(x => x.Resource).OfType<DnsIpAddressResource>().Select(x => x.IPAddress).ToArray();
        if (useCache && addresses.Length > 0)
        {
            var seconds = Math.Min((double)answer.Answers.Min(static record => record.TimeToLive), TTL.TotalSeconds);
            if (seconds > 0)
            {
                if (Cache.Count >= 4096)
                    Cache.Clear();
                Cache[key] = new() { IPAddresses = addresses, ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(seconds) };
            }
        }
        return addresses;
    }

    readonly record struct IPAddressResult
    {
        public IPAddress[] IPAddresses { get; init; }

        public DateTimeOffset ExpiresAt { get; init; }
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
        var server = GetDohAddres(dohAddres);
        IPAddress[] result = Array.Empty<IPAddress>();
        if (isIPv6)
        {
            try
            {
                result = await Query(server, hostNameOrAddress, DnsQueryType.AAAA, useCache, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception) { }
        }
        if (result.Length == 0)
            result = await Query(server, hostNameOrAddress, DnsQueryType.A, useCache, cancellationToken);
        foreach (var item in result)
            yield return item;
    }
}