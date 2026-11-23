using Ae.Dns.Protocol;
using Ae.Dns.Protocol.Records;
using static BD.WTTS.Services.IDnsAnalysisService;
using DnsQueryType = Ae.Dns.Protocol.Enums.DnsQueryType;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class DnsDohAnalysisService : GeneralHttpClientFactory, IDisposable
{
    const string TAG = "DnsDohAnalysisS";
    const string DnsMessageMediaType = "application/dns-message";

    protected override string? DefaultClientName => TAG;

    readonly ConcurrentDictionary<(Uri Server, string Host, DnsQueryType Type), IPAddressResult> Cache = new();
    readonly ConcurrentDictionary<Uri, Lazy<HttpClient>> dnsClients = new();

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

    HttpClient GetDnsHttpClient(Uri server) => dnsClients.GetOrAdd(server, static _ =>
        new Lazy<HttpClient>(static () =>
        {
            var handler = new HttpClientHandler
            {
                UseCookies = false,
                UseProxy = false,
                Proxy = HttpNoProxy.Instance,
            };
            return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(3) };
        })).Value;

    async Task<DnsMessage> QueryServerAsync(Uri server, string host, DnsQueryType queryType, CancellationToken cancellationToken)
    {
        var query = DnsQueryFactory.CreateQuery(host, queryType);
        // A DNS header and a single question fit within 512 bytes even with the longest valid name.
        var payload = new byte[512];
        var length = 0;
        query.WriteBytes(payload, ref length);

        using var request = new HttpRequestMessage(HttpMethod.Post, server)
        {
            Content = new ReadOnlyMemoryContent(payload.AsMemory(0, length)),
        };
        request.Content.Headers.ContentType = new(DnsMessageMediaType);
        request.Headers.Accept.Add(new(DnsMessageMediaType));

        using var response = await GetDnsHttpClient(server).SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var answer = new DnsMessage();
        var offset = 0;
        answer.ReadBytes(bytes, ref offset);
        if (offset != bytes.Length)
            throw new InvalidDataException("The DoH response contains trailing data.");

        return answer;
    }

    public async Task<int> DohAnalysisHostnameTimeAsync(string? dohAddres, string url, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(url))
        {
            var answer = await QueryServerAsync(GetDohAddres(dohAddres), url, DnsQueryType.A, cancellationToken);
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

        var answer = await QueryServerAsync(server, host, queryType, cancellationToken);
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
