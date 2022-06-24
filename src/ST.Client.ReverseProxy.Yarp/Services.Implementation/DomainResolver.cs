// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.DomainResolve/DomainResolver.cs

using System.Application.Models;
using System.Net;

namespace System.Application.Services.Implementation;

sealed class DomainResolver : IDomainResolver
{
    readonly IReverseProxyConfig reverseProxyConfig;

    bool isIpv6 = false;

    public DomainResolver(IReverseProxyConfig reverseProxyConfig)
    {
        this.reverseProxyConfig = reverseProxyConfig;
    }

    public IAsyncEnumerable<IPAddress> ResolveAsync(DnsEndPoint endPoint, CancellationToken cancellationToken = default)
    {
        return reverseProxyConfig.Service.DnsAnalysis.AnalysisDomainIpAsync(endPoint.Host, isIpv6, cancellationToken);
    }

    public async void CheckIpv6SupportAsync(CancellationToken cancellationToken = default)
    {
        isIpv6 = await reverseProxyConfig.Service.DnsAnalysis.GetIsIpv6Support();
    }

    public Task TestSpeedAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
