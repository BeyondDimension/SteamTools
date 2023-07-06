// https://github.com/dotnetcore/FastGithub/blob/ddf63b315fee62a51b3b0fbc6875960dd236f9d0/FastGithub.HttpServer/TcpMiddlewares/IHttpProxyFeature.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

interface IHttpProxyFeature
{
    HostString ProxyHost { get; }

    ProxyProtocol ProxyProtocol { get; }
}