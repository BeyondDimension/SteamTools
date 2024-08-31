// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/Tcp/SshInterceptor.cs

#if WINDOWS && !REMOVE_DNS_INTERCEPT

using static BD.WTTS.Models.Abstractions.IReverseProxyConfig;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// SSH 拦截器
/// </summary>
sealed class SshInterceptor : TcpInterceptor
{
    public SshInterceptor(ILogger<SshInterceptor> logger) : base(SshPortDefault, SshPort, logger)
    {

    }
}
#endif