// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/Tcp/HttpInterceptor.cs

#if WINDOWS

using static BD.WTTS.Models.IReverseProxyConfig;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

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