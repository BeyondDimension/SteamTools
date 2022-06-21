// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/KestrelServerOptionsExtensions.cs

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Application.Internals.HttpServer;
using System.Application.Models;
using System.Application.Services;

namespace Microsoft.AspNetCore.Hosting;

public static class KestrelServerOptionsExtensions
{
    /// <summary>
    /// 无限制
    /// </summary>
    /// <param name="options"></param>
    public static void NoLimit(this KestrelServerOptions options)
    {
        options.Limits.MaxRequestBodySize = null;
        options.Limits.MinResponseDataRate = null;
        options.Limits.MinRequestBodyDataRate = null;
    }

    /// <summary>
    /// 监听 Http 代理
    /// </summary>
    /// <param name="options"></param>
    public static void ListenHttpProxy(this KestrelServerOptions options)
    {
        var reverseProxyConfig = options.ApplicationServices.GetRequiredService<IReverseProxyConfig>();
        var httpProxyPort = reverseProxyConfig.HttpProxyPort;

        if (!IReverseProxyConfig.IsAvailableTcp(httpProxyPort))
        {
            throw new ApplicationException(
                $"TCP port {httpProxyPort} is already occupied by other processes.");
        }

        options.ListenLocalhost(httpProxyPort);
        var logger = options.GetLogger();
        logger.LogInformation(
            $"Listened http://localhost:{httpProxyPort}, HTTP proxy service startup completed.");
    }

    /// <summary>
    /// 监听 SSH 反向代理
    /// </summary>
    /// <param name="options"></param>
    public static void ListenSshReverseProxy(this KestrelServerOptions options)
    {
        var sshPort = IReverseProxyConfig.SshPort;
        options.ListenLocalhost(sshPort, listen =>
        {
            listen.UseFlowAnalyze();
            listen.UseConnectionHandler<GithubSshReverseProxyHandler>();
        });

        var logger = options.GetLogger();
        logger.LogInformation(
            $"Listened ssh://localhost:{sshPort}, the SSH reverse proxy service of GitHub is started.");
    }

    /// <summary>
    /// 监听 Git 反向代理
    /// </summary>
    /// <param name="options"></param>
    public static void ListenGitReverseProxy(this KestrelServerOptions options)
    {
        var gitPort = IReverseProxyConfig.GitPort;
        options.ListenLocalhost(gitPort, listen =>
        {
            listen.UseFlowAnalyze();
            listen.UseConnectionHandler<GithubGitReverseProxyHandler>();
        });

        var logger = options.GetLogger();
        logger.LogInformation(
            $"Listened git://localhost:{gitPort}, the Git reverse proxy service of GitHub has been started.");
    }

    /// <summary>
    /// 监听 HTTP 反向代理
    /// </summary>
    /// <param name="options"></param>
    public static void ListenHttpReverseProxy(this KestrelServerOptions options)
    {
        var httpPort = IReverseProxyConfig.HttpPort;
        options.ListenLocalhost(httpPort);

        var logger = options.GetLogger();
        logger.LogInformation(
            $"Listened http://localhost:{httpPort}, HTTP reverse proxy service startup completed.");
    }

    /// <summary>
    /// 监听 HTTPS 反向代理
    /// </summary>
    /// <param name="options"></param>
    public static void ListenHttpsReverseProxy(this KestrelServerOptions options)
    {
        var reverseProxyConfig = options.ApplicationServices.GetRequiredService<IReverseProxyConfig>();

        var httpsPort = IReverseProxyConfig.HttpsPort;
        options.ListenLocalhost(httpsPort, listen =>
        {
            listen.UseFlowAnalyze();
            listen.UseHttps(https =>
            {
                https.ServerCertificateSelector = (connectionContext, name) => reverseProxyConfig.Service.RootCertificate;
            });
        });

        var logger = options.GetLogger();
        logger.LogInformation(
            $"Listened https://localhost:{httpsPort}, HTTPS reverse proxy service startup completed.");
    }

    static ILogger GetLogger(this KestrelServerOptions kestrel)
    {
        var loggerFactory = kestrel.ApplicationServices.GetRequiredService<ILoggerFactory>();
        return loggerFactory.CreateLogger(IReverseProxyService.TAG);
    }
}
