// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.DomainResolve/DomainResolver.cs

using System.Application.Models;
using System.Net;

namespace System.Application.Services.Implementation;

sealed class DomainResolver : IDomainResolver
{
    readonly IReverseProxyConfig reverseProxyConfig;

    public DomainResolver(IReverseProxyConfig reverseProxyConfig)
    {
        this.reverseProxyConfig = reverseProxyConfig;
    }

    public IAsyncEnumerable<IPAddress> ResolveAsync(DnsEndPoint endPoint, CancellationToken cancellationToken = default)
    {
        var hostNameOrAddress = endPoint.ToString();
        return reverseProxyConfig.Service.DnsAnalysis.AnalysisDomainIpAsync(hostNameOrAddress, cancellationToken);
    }

    public Task TestSpeedAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
