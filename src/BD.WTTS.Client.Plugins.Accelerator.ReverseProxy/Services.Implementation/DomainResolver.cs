// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.DomainResolve/DomainResolver.cs

//using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class DomainResolver : IDomainResolver
{
    readonly IReverseProxyConfig reverseProxyConfig;
    readonly IPCToastService toast;

    bool isIpv6 = false;

    public DomainResolver(IPCToastService toast, IReverseProxyConfig reverseProxyConfig)
    {
        this.reverseProxyConfig = reverseProxyConfig;
        this.toast = toast;
    }

    public IAsyncEnumerable<IPAddress> ResolveAsync(DnsEndPoint endPoint, CancellationToken cancellationToken = default)
    {
        try
        {
            // Hosts and DNS interception must not resolve an accelerated domain back to this proxy.
            var servers = reverseProxyConfig.Service.ProxyMode is ProxyMode.DNSIntercept or ProxyMode.Hosts
                ? IDnsAnalysisService.DNS_Dnspods
                : null;
            return reverseProxyConfig.Service.DnsAnalysis.AnalysisDomainIpAsync(endPoint.Host, servers, isIpv6, cancellationToken);
        }
        catch (Exception ex)
        {
            toast.ShowAppend(
                IPCToastService.ToastIcon.Error,
                IPCToastService.ToastText.CommunityFix_DNSErrorNotify,
                Environment.NewLine + "ExceptionMessage: " + ex.Message);
            //INotificationService.Instance.Notify(AppResources.CommunityFix_DNSErrorNotify + Environment.NewLine + "Exception Message :" + ex.Message, NotificationType.Message);
            throw;
        }
    }

    public async void CheckIpv6SupportAsync(CancellationToken cancellationToken = default)
    {
        isIpv6 = await reverseProxyConfig.Service.DnsAnalysis.GetIsIpv6SupportAsync();
    }

    public Task TestSpeedAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}