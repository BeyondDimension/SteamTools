// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/Tcp/HttpInterceptor.cs

#if WINDOWS

using Microsoft.Extensions.Logging;
using static System.Application.Models.IReverseProxyConfig;

namespace System.Application.Services.Implementation.PacketIntercept;

/// <summary>
/// Http 拦截器
/// </summary>
sealed class HttpInterceptor : TcpInterceptor
{
    public HttpInterceptor(ILogger<HttpInterceptor> logger) : base(HttpPortDefault, HttpPort, logger)
    {

    }
}

#endif