// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/Tcp/SshInterceptor.cs

#if WINDOWS

using Microsoft.Extensions.Logging;
using static System.Application.Models.IReverseProxyConfig;

namespace System.Application.Services.Implementation.PacketIntercept;

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