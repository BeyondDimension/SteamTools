// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/Tcp/HttpsInterceptor.cs

#if WINDOWS

using Microsoft.Extensions.Logging;
using static System.Application.Models.IReverseProxyConfig;

namespace System.Application.Services.Implementation.PacketIntercept;

/// <summary>
/// Https 拦截器
/// </summary>
sealed class HttpsInterceptor : TcpInterceptor
{
    public HttpsInterceptor(ILogger<HttpsInterceptor> logger) : base(HttpsPortDefault, HttpsPort, logger)
    {

    }
}

#endif