// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/TcpInterceptHostedService.cs

#if WINDOWS && !REMOVE_DNS_INTERCEPT

using static BD.WTTS.Models.Abstractions.IReverseProxyConfig;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// TCP 拦截后台服务
/// </summary>
sealed class TcpInterceptHostedService : InterceptHostedService
{
    public TcpInterceptHostedService(
        IEnumerable<ITcpInterceptor> tcpInterceptors,
        ILogger<TcpInterceptHostedService> logger,
        IReverseProxyConfig reverseProxyConfig) : base(new Interceptors(tcpInterceptors), logger, reverseProxyConfig)
    {
    }
}
#endif