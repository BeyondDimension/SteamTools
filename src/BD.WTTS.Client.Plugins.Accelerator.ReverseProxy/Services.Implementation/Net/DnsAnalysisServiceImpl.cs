using DnsClient;
using DnsClient.Protocol;
using static BD.WTTS.Services.IDnsAnalysisService;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class DnsAnalysisServiceImpl
{
    readonly LookupClient lookupClient = new();

    public async Task<int> AnalysisHostnameTimeAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(url))
        {
            var client = lookupClient;
            var result = await client.QueryAsync(url, QueryType.A, QueryClass.IN, cancellationToken);
            if (result.Answers.Count > 0)
            {
                var value = result.Answers.ARecords().FirstOrDefault()?.InitialTimeToLive;
                return value ?? 0;
            }
        }
        return 0;
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
        if (!string.IsNullOrEmpty(hostNameOrAddress))
        {
            var client = lookupClient;
            DnsQuestion question;
            DnsQueryAndServerOptions? options = null;
            IDnsQueryResponse response;

            if (dnsServers == null)
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
            else
            {
                options = new DnsQueryAndServerOptions(dnsServers);
            }

            if (isIPv6)
            {
                question = new DnsQuestion(hostNameOrAddress, QueryType.AAAA);

                if (options != null)
                    response = await client.QueryAsync(question, options, cancellationToken);
                else
                    response = await client.QueryAsync(question, cancellationToken);

                if (!response.HasError)
                {
                    var aaaaRecord = response.Answers.AaaaRecords();
                    foreach (var item in aaaaRecord)
                    {
                        yield return item.Address;
                    }
                }
            }

            question = new DnsQuestion(hostNameOrAddress, QueryType.A);

            if (options != null)
                response = await client.QueryAsync(question, options, cancellationToken);
            else
                response = await client.QueryAsync(question, cancellationToken);

            if (!response.HasError)
            {
                var aRecord = response.Answers.ARecords();
                foreach (var item in aRecord)
                {
                    yield return item.Address;
                }
            }
        }
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