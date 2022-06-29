// https://github.com/dotnetcore/FastGithub/blob/ddf63b315fee62a51b3b0fbc6875960dd236f9d0/FastGithub.HttpServer/TcpMiddlewares/ProxyProtocol.cs

namespace System.Application.Services.Implementation.HttpServer.Middleware;

enum ProxyProtocol : byte
{
    /// <summary>
    /// 无代理
    /// </summary>
    None,

    /// <summary>
    /// Http 代理
    /// </summary>
    HttpProxy,

    /// <summary>
    /// 隧道代理
    /// </summary>
    TunnelProxy,
}
