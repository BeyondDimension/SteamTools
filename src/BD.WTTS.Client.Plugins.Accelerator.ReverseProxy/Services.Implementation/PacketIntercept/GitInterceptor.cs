// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/Tcp/GitInterceptor.cs

#if WINDOWS && !REMOVE_DNS_INTERCEPT

using static BD.WTTS.Models.Abstractions.IReverseProxyConfig;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// Git 拦截器
/// </summary>
sealed class GitInterceptor : TcpInterceptor
{
    public GitInterceptor(ILogger<GitInterceptor> logger) : base(GitHubDesktopPort, GitPort, logger)
    {

    }
}

#endif