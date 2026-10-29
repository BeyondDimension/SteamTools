using DnsClient;
using DnsClient.Protocol;
using static BD.WTTS.Services.IDnsAnalysisService;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class DnsAnalysisServiceImpl
{
    readonly LookupClient lookupClient = new(new LookupClientOptions
    {
        Retries = 0,
        Timeout = TimeSpan.FromSeconds(2),
        UseCache = false,
    });

    // DnsClient caches by question, so sharing a client would let a fallback answer mask the preferred server.
    readonly ConcurrentDictionary<IPAddress, LookupClient> serverClients = new();

    LookupClient GetClient(IPAddress[]? servers)
        => servers is { Length: 1 }
            ? serverClients.GetOrAdd(servers[0], static server => new LookupClient(new LookupClientOptions(server)
            {
                Retries = 0,
                Timeout = TimeSpan.FromSeconds(2),
            }))
            : lookupClient;

    public async Task<int> AnalysisHostnameTimeAsync(string url, IPAddress[]? dnsServers = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(url))
            return 0;
        var question = new DnsQuestion(url, QueryType.A);
        var client = GetClient(dnsServers);
        var result = dnsServers is { Length: > 1 }
            ? await client.QueryAsync(question, new DnsQueryAndServerOptions(dnsServers) { UseCache = false }, cancellationToken)
            : await client.QueryAsync(question, cancellationToken);
        return result.HasError ? 0 : result.Answers.ARecords().FirstOrDefault()?.InitialTimeToLive ?? 0;
    }

    public IEnumerable<ARecord>? GetPingHostNameData(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            var client = lookupClient;
            var result = client.Query(url, QueryType.A);
            if (result.Answers.Count > 0)
            {
                return result.Answers.ARecords();
            }
        }
        return null;
    }

    public async IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, IPAddress[]? dnsServers, bool isIPv6, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(hostNameOrAddress))
            yield break;
        if (dnsServers == null)
        {
#if NET6_0_OR_GREATER
            var items = await Dns.GetHostAddressesAsync(hostNameOrAddress, cancellationToken)
                .WaitAsync(TimeSpan.FromSeconds(2), cancellationToken);
#else
            var items = await Dns.GetHostAddressesAsync(hostNameOrAddress);
#endif
            foreach (var item in items)
                yield return item;
            yield break;
        }

        var options = dnsServers.Length > 1 ? new DnsQueryAndServerOptions(dnsServers) { UseCache = false } : null;
        var client = GetClient(dnsServers);
        var hasIpv6 = false;
        if (isIPv6)
        {
            IDnsQueryResponse? response = null;
            try
            {
                var question = new DnsQuestion(hostNameOrAddress, QueryType.AAAA);
                response = options == null
                    ? await client.QueryAsync(question, cancellationToken)
                    : await client.QueryAsync(question, options, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception) { }
            if (response != null && !response.HasError)
                foreach (var item in response.Answers.AaaaRecords())
                {
                    hasIpv6 = true;
                    yield return item.Address;
                }
        }

        IDnsQueryResponse? ipv4 = null;
        try
        {
            var question = new DnsQuestion(hostNameOrAddress, QueryType.A);
            ipv4 = options == null
                ? await client.QueryAsync(question, cancellationToken)
                : await client.QueryAsync(question, options, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception) when (hasIpv6) { }
        if (ipv4 != null && !ipv4.HasError)
            foreach (var item in ipv4.Answers.ARecords())
                yield return item.Address;
    }

    public async Task<string?> GetHostByIPAddressAsync(IPAddress ip)
    {
        //var hostName = Dns.GetHostEntry(IPAddress2.Parse(ip)).HostName;
        var client = lookupClient;
        var result = await client.QueryReverseAsync(ip);
        var hostName = result.Answers.PtrRecords().FirstOrDefault()?.PtrDomainName.Value;
        return hostName;
    }

    public async Task<bool> GetIsIpv6SupportAsync()
    {
        try
        {
            var options = new LookupClientOptions
            {
                Retries = 1,
                Timeout = TimeSpan.FromSeconds(1),
                UseCache = false,
            };
            var client = new LookupClient(options);
            var response = await client.QueryServerAsync(
                new[] {
                    IPAddress.Parse(PrimaryDNS_IPV6_Ali),
                },
                IPV6_TESTDOMAIN,
                QueryType.AAAA,
                QueryClass.IN);
            if (response.Answers.AaaaRecords().Any(s =>
                s.Address.Equals(IPAddress.Parse(IPV6_TESTDOMAIN_SUCCESS))))
            {
                return true;
            }
        }
        catch
        {
        }
        return false;
    }
}