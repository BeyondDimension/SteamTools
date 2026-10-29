// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class DnsAnalysisServiceSwitchImpl : IDnsAnalysisService
{
    readonly DnsAnalysisServiceImpl dnsAnalysisServiceImpl;
    readonly DnsDohAnalysisService dnsDohAnalysisService;
    readonly IReverseProxySettings settings;
    readonly object serverLock = new();
    string? cachedConfiguration;
    Server[]? cachedServers;

    sealed record Server(string Address, IPAddress[]? DnsServers, bool IsSystemDefault, bool IsDoh);

    Server[] GetServers()
    {
        var configuration = settings.ProxyDNSAddresses;
        if (string.IsNullOrWhiteSpace(configuration))
            configuration = IDnsAnalysisService.DefaultResolverServerList;
        lock (serverLock)
        {
            if (cachedServers != null && cachedConfiguration == configuration)
                return cachedServers;
            var servers = new List<Server>();
            foreach (var item in configuration.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var address = item.Trim();
                if (string.Equals(address, "System Default", StringComparison.OrdinalIgnoreCase))
                    servers.Add(new Server(address, null, true, false));
                else if (Uri.TryCreate(address, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps && !string.IsNullOrEmpty(uri.Host))
                    servers.Add(new Server(address, null, false, true));
                else if (IPAddress.TryParse(address, out var ip))
                    servers.Add(new Server(address, new[] { ip }, false, false));
            }
            cachedConfiguration = configuration;
            return cachedServers = servers.ToArray();
        }
    }

    bool RequiresSafeDns => settings.ProxyMode is ProxyMode.DNSIntercept or ProxyMode.Hosts;

    public DnsAnalysisServiceSwitchImpl(
        IReverseProxySettings settings,
        DnsAnalysisServiceImpl dnsAnalysisServiceImpl,
        DnsDohAnalysisService dnsDohAnalysisService)
    {
        this.settings = settings;
        this.dnsAnalysisServiceImpl = dnsAnalysisServiceImpl;
        this.dnsDohAnalysisService = dnsDohAnalysisService;
    }

    public async Task<int> AnalysisHostnameTimeAsync(string url, CancellationToken cancellationToken = default)
    {
        foreach (var server in GetServers())
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var ttl = server.IsDoh
                    ? await dnsDohAnalysisService.DohAnalysisHostnameTimeAsync(server.Address, url, cancellationToken)
                    : await dnsAnalysisServiceImpl.AnalysisHostnameTimeAsync(url,
                        server.IsSystemDefault && RequiresSafeDns ? IDnsAnalysisService.DNS_Dnspods : server.DnsServers, cancellationToken);
                if (ttl > 0)
                    return ttl;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception) { }
        }
        return 0;
    }

    public async IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, IPAddress[]? dnsServers, bool isIPv6, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (IPAddress.TryParse(hostNameOrAddress, out var literal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return literal;
            yield break;
        }
        foreach (var server in GetServers())
        {
            cancellationToken.ThrowIfCancellationRequested();
            IPAddress[] results;
            try
            {
                if (server.IsDoh)
                    results = await dnsDohAnalysisService.DohAnalysisDomainIpAsync(server.Address, hostNameOrAddress, isIPv6, true, cancellationToken).ToArrayAsync(cancellationToken);
                else
                {
                    // System resolution would resolve accelerated hosts back to this proxy.
                    var servers = server.IsSystemDefault && RequiresSafeDns
                        ? dnsServers ?? IDnsAnalysisService.DNS_Dnspods
                        : server.DnsServers;
                    results = await dnsAnalysisServiceImpl.AnalysisDomainIpAsync(hostNameOrAddress, servers, isIPv6, cancellationToken).ToArrayAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                continue;
            }
            if (results.Length == 0)
                continue;
            foreach (var result in results)
                yield return result;
            yield break;
        }
    }

    public async Task<string?> GetHostByIPAddressAsync(IPAddress ip)
    {
        var r = await dnsAnalysisServiceImpl.GetHostByIPAddressAsync(ip);
        return r;
    }

    public async Task<bool> GetIsIpv6SupportAsync()
    {
        var r = await dnsAnalysisServiceImpl.GetIsIpv6SupportAsync();
        return r;
    }
}
