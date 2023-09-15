// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class DnsAnalysisServiceSwitchImpl : IDnsAnalysisService
{
    readonly IDnsAnalysisService dnsAnalysisServiceImpl;
    readonly IDnsAnalysisService dnsDohAnalysisService;
    readonly IReverseProxySettings settings;

    public DnsAnalysisServiceSwitchImpl(
        IReverseProxySettings settings,
        DnsAnalysisServiceImpl dnsAnalysisServiceImpl,
        DnsDohAnalysisService dnsDohAnalysisService)
    {
        this.settings = settings;
        this.dnsAnalysisServiceImpl = dnsAnalysisServiceImpl;
        this.dnsDohAnalysisService = dnsDohAnalysisService;
    }

    public async Task<long> PingHostnameAsync(string url)
    {
        long r;
        if (settings.UseDoh)
        {
            r = await dnsAnalysisServiceImpl.PingHostnameAsync(url);
        }
        else
        {
            r = await dnsDohAnalysisService.PingHostnameAsync(url);
        }
        return r;
    }

    public async Task<int> AnalysisHostnameTimeAsync(string url, CancellationToken cancellationToken = default)
    {
        int r;
        if (settings.UseDoh)
        {
            var dohAddres = settings.CustomDohAddres;
            if (!string.IsNullOrWhiteSpace(dohAddres))
            {
                r = await dnsAnalysisServiceImpl.AnalysisHostnameTimeByDohAddresAsync(dohAddres, url, cancellationToken);
            }
            else
            {
                r = await dnsAnalysisServiceImpl.AnalysisHostnameTimeAsync(url, cancellationToken);
            }
        }
        else
        {
            r = await dnsDohAnalysisService.AnalysisHostnameTimeAsync(url, cancellationToken);
        }
        return r;
    }

    public async Task<int> AnalysisHostnameTimeByDohAddresAsync(string dohAddres, string url, CancellationToken cancellationToken = default)
    {
        int r;
        if (settings.UseDoh)
        {
            r = await dnsAnalysisServiceImpl.AnalysisHostnameTimeByDohAddresAsync(dohAddres, url, cancellationToken);
        }
        else
        {
            r = await dnsDohAnalysisService.AnalysisHostnameTimeByDohAddresAsync(dohAddres, url, cancellationToken);
        }
        return r;
    }

    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, bool isIPv6, CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<IPAddress> r;
        if (settings.UseDoh)
        {
            r = dnsAnalysisServiceImpl.AnalysisDomainIpAsync(hostNameOrAddress, isIPv6, cancellationToken);
        }
        else
        {
            r = dnsDohAnalysisService.AnalysisDomainIpAsync(hostNameOrAddress, isIPv6, cancellationToken);
        }
        return r;
    }

    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<IPAddress> r;
        if (settings.UseDoh)
        {
            r = dnsAnalysisServiceImpl.AnalysisDomainIpAsync(hostNameOrAddress, cancellationToken);
        }
        else
        {
            r = dnsDohAnalysisService.AnalysisDomainIpAsync(hostNameOrAddress, cancellationToken);
        }
        return r;
    }

    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, IPAddress[]? dnsServers, CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<IPAddress> r;
        if (settings.UseDoh)
        {
            r = dnsAnalysisServiceImpl.AnalysisDomainIpAsync(hostNameOrAddress, dnsServers, cancellationToken);
        }
        else
        {
            r = dnsDohAnalysisService.AnalysisDomainIpAsync(hostNameOrAddress, dnsServers, cancellationToken);
        }
        return r;
    }

    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, IPAddress[]? dnsServers, bool isIPv6, CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<IPAddress> r;
        if (settings.UseDoh)
        {
            r = dnsAnalysisServiceImpl.AnalysisDomainIpAsync(hostNameOrAddress, dnsServers, isIPv6, cancellationToken);
        }
        else
        {
            r = dnsDohAnalysisService.AnalysisDomainIpAsync(hostNameOrAddress, dnsServers, isIPv6, cancellationToken);
        }
        return r;
    }

    public IAsyncEnumerable<IPAddress>? DohAnalysisDomainIpAsync(string hostNameOrAddress, string? dnsServers, bool isIPv6, CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<IPAddress>? r;
        if (settings.UseDoh)
        {
            r = dnsAnalysisServiceImpl.DohAnalysisDomainIpAsync(hostNameOrAddress, dnsServers, isIPv6, cancellationToken);
        }
        else
        {
            r = dnsDohAnalysisService.DohAnalysisDomainIpAsync(hostNameOrAddress, dnsServers, isIPv6, cancellationToken);
        }
        return r;
    }

    public async Task<string?> GetHostByIPAddressAsync(IPAddress ip)
    {
        string? r;
        if (settings.UseDoh)
        {
            r = await dnsAnalysisServiceImpl.GetHostByIPAddressAsync(ip);
        }
        else
        {
            r = await dnsDohAnalysisService.GetHostByIPAddressAsync(ip);
        }
        return r;
    }

    public async Task<bool> GetIsIpv6SupportAsync()
    {
        bool r;
        if (settings.UseDoh)
        {
            r = await dnsAnalysisServiceImpl.GetIsIpv6SupportAsync();
        }
        else
        {
            r = await dnsDohAnalysisService.GetIsIpv6SupportAsync();
        }
        return r;
    }

    public async Task<IPAddress?> GetHostIpv6AddresAsync()
    {
        IPAddress? r;
        if (settings.UseDoh)
        {
            r = await dnsAnalysisServiceImpl.GetHostIpv6AddresAsync();
        }
        else
        {
            r = await dnsDohAnalysisService.GetHostIpv6AddresAsync();
        }
        return r;
    }
}
