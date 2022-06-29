// https://github.com/dotnetcore/FastGithub/blob/ddf63b315fee62a51b3b0fbc6875960dd236f9d0/FastGithub.HttpServer/TcpMiddlewares/IHttpProxyFeature.cs

using Microsoft.AspNetCore.Http;

namespace System.Application.Services.Implementation.HttpServer.Middleware;

interface IHttpProxyFeature
{
    HostString ProxyHost { get; }

    ProxyProtocol ProxyProtocol { get; }
}
