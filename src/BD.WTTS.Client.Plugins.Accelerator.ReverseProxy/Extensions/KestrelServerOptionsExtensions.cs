// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/KestrelServerOptionsExtensions.cs

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Hosting;

public static class KestrelServerOptionsExtensions
{
    /// <summary>
    /// 无限制
    /// </summary>
    /// <param name="options"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ListenHttpProxy(this KestrelServerOptions options)
    {
        var reverseProxyConfig = options.ApplicationServices.GetRequiredService<IReverseProxyConfig>();
        var httpProxyPort = reverseProxyConfig.HttpProxyPort;

        if (!IReverseProxyConfig.IsAvailableTcp(httpProxyPort))
        {
            throw new ApplicationException(
                $"TCP port {httpProxyPort} is already occupied by other processes.");
        }

        options.Listen(IReverseProxyService.Constants.Instance.ProxyIp, httpProxyPort, listen =>
        {
            var proxyMiddleware = options.ApplicationServices.GetRequiredService<HttpProxyMiddleware>();
            var tunnelMiddleware = options.ApplicationServices.GetRequiredService<TunnelMiddleware>();

            listen.UseFlowAnalyze();
            listen.Use(next => context => proxyMiddleware.InvokeAsync(next, context));
            listen.UseTls();
            listen.Use(next => context => tunnelMiddleware.InvokeAsync(next, context));
        });

        options.GetLogger().LogInformation(
            "Listened http://{ProxyIp}:{httpProxyPort}, HTTP proxy service startup completed.",
            IReverseProxyService.Constants.Instance.ProxyIp, httpProxyPort);
    }

#if WINDOWS
    /// <summary>
    /// 监听 SSH 反向代理
    /// </summary>
    /// <param name="options"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            "Listened ssh://localhost:{sshPort}, the SSH reverse proxy service of GitHub is started.", sshPort);
    }
#endif

#if WINDOWS
    /// <summary>
    /// 监听 Git 反向代理
    /// </summary>
    /// <param name="options"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            "Listened git://localhost:{gitPort}, the Git reverse proxy service of GitHub has been started.", gitPort);
    }
#endif

    /// <summary>
    /// 监听 HTTP 反向代理
    /// </summary>
    /// <param name="options"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ListenHttpReverseProxy(this KestrelServerOptions options)
    {
        var httpPort = IReverseProxyConfig.HttpPort;
        options.Listen(IReverseProxyService.Constants.Instance.ProxyIp, httpPort);

        var logger = options.GetLogger();
        logger.LogInformation(
            "Listened http://{ProxyIp}:{httpPort}, HTTP reverse proxy service startup completed.",
            IReverseProxyService.Constants.Instance.ProxyIp, httpPort);
    }

    /// <summary>
    /// 监听 HTTPS 反向代理
    /// </summary>
    /// <param name="options"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ListenHttpsReverseProxy(this KestrelServerOptions options)
    {
        var certService = options.ApplicationServices.GetRequiredService<CertService>();
        //var reverseProxyConfig = options.ApplicationServices.GetRequiredService<IReverseProxyConfig>();

        var domainResolver = options.ApplicationServices.GetRequiredService<IDomainResolver>();
        domainResolver.CheckIpv6SupportAsync();

        var httpsPort = IReverseProxyConfig.HttpsPort;
        options.Listen(IReverseProxyService.Constants.Instance.ProxyIp, httpsPort, listen =>
        {
            listen.Protocols = HttpProtocols.Http1AndHttp2;
            listen.UseFlowAnalyze();
            listen.UseTls();
        });

        var logger = options.GetLogger();
        logger.LogInformation(
            "Listened https://{ProxyIp}:{httpsPort}, HTTPS reverse proxy service startup completed.",
            IReverseProxyService.Constants.Instance.ProxyIp, httpsPort);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ILogger GetLogger(this KestrelServerOptions kestrel)
    {
        var loggerFactory = kestrel.ApplicationServices.GetRequiredService<ILoggerFactory>();
        return loggerFactory.CreateLogger(TAG);
    }

    const string TAG = "KestrelServerOptEx";

}