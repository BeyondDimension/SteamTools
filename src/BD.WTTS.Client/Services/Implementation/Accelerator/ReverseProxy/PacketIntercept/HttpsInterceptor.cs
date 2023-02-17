// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/Tcp/HttpsInterceptor.cs

#if WINDOWS

using static BD.WTTS.Models.IReverseProxyConfig;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

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