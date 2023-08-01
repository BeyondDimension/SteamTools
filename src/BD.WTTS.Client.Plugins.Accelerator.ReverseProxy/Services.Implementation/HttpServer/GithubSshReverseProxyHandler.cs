#if WINDOWS
// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/GithubSshReverseProxyHandler.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// Github SSH 代理处理者
/// </summary>
sealed class GithubSshReverseProxyHandler : TcpReverseProxyHandler
{
    public GithubSshReverseProxyHandler(IDomainResolver domainResolver)
        : base(domainResolver, new("github.com", IReverseProxyConfig.SshPort))
    {

    }
}
#endif