// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/Tcp/GitInterceptor.cs

#if WINDOWS

using Microsoft.Extensions.Logging;
using static System.Application.Models.IReverseProxyConfig;

namespace System.Application.Services.Implementation.PacketIntercept;

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