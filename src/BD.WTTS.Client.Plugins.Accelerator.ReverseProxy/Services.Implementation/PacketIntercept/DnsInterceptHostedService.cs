// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/DnsInterceptHostedService.cs

#if WINDOWS && !REMOVE_DNS_INTERCEPT

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// DNS 拦截后台服务
/// </summary>
sealed class DnsInterceptHostedService : InterceptHostedService
{
    public DnsInterceptHostedService(
        IDnsInterceptor dnsInterceptor,
        ILogger<DnsInterceptHostedService> logger,
        IReverseProxyConfig reverseProxyConfig) : base(dnsInterceptor, logger, reverseProxyConfig)
    {
    }
}

#endif