// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/GithubGitReverseProxyHandler.cs

using System.Application.Models;

namespace System.Application.Services.Implementation.HttpServer;

/// <summary>
/// Github Git 代理处理者
/// </summary>
sealed class GithubGitReverseProxyHandler : TcpReverseProxyHandler
{
    public GithubGitReverseProxyHandler(IDomainResolver domainResolver)
        : base(domainResolver, new("github.com", IReverseProxyConfig.GitPort))
    {

    }
}
