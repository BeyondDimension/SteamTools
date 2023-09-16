// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class DnsAnalysisServiceSwitchImpl : IDnsAnalysisService
{
    readonly DnsAnalysisServiceImpl dnsAnalysisServiceImpl;
    readonly DnsDohAnalysisService dnsDohAnalysisService;
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

    public async Task<int> AnalysisHostnameTimeAsync(string url, CancellationToken cancellationToken = default)
    {
        int r;
        if (settings.UseDoh)
        {
            r = await dnsDohAnalysisService.DohAnalysisHostnameTimeAsync(
                settings.CustomDohAddres, url, cancellationToken);
        }
        else
        {
            r = await dnsAnalysisServiceImpl.AnalysisHostnameTimeAsync(url, cancellationToken);
        }
        return r;
    }

    public IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, IPAddress[]? dnsServers, bool isIPv6, CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<IPAddress> r;
        if (settings.UseDoh)
        {
            r = dnsDohAnalysisService.DohAnalysisDomainIpAsync(settings.CustomDohAddres, hostNameOrAddress, isIPv6, true, cancellationToken);
        }
        else
        {

            r = dnsAnalysisServiceImpl.AnalysisDomainIpAsync(hostNameOrAddress, dnsServers, isIPv6, cancellationToken);
        }
        return r;
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
