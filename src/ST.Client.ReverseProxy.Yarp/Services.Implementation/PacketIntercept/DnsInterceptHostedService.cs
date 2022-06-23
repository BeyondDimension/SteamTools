// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/DnsInterceptHostedService.cs

#if WINDOWS

using Microsoft.Extensions.Logging;
using System.Application.Models;

namespace System.Application.Services.Implementation.PacketIntercept;

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