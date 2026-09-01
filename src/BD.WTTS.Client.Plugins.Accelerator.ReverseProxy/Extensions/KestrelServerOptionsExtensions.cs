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

        var proxyMiddleware = options.ApplicationServices.GetRequiredService<HttpProxyMiddleware>();
        var tunnelMiddleware = options.ApplicationServices.GetRequiredService<TunnelMiddleware>();

        options.ListenAndLog(
            IReverseProxyService.Constants.Instance.ProxyIp,
            httpProxyPort,
            listen =>
            {
                listen.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                listen.UseFlowAnalyze();
                listen.Use(next => context => proxyMiddleware.InvokeAsync(next, context));
                listen.UseTls();
                listen.Use(next => context => tunnelMiddleware.InvokeAsync(next, context));
            },
            "Listened http://{ProxyIp}:{httpProxyPort}, HTTP proxy service startup completed.",
            IReverseProxyService.Constants.Instance.ProxyIp,
            httpProxyPort);
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
        options.ListenLocalReverseProxy<GithubSshReverseProxyHandler>(
            sshPort,
            "Listened ssh://localhost:{sshPort}, the SSH reverse proxy service of GitHub is started.",
            sshPort);
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
        options.ListenLocalReverseProxy<GithubGitReverseProxyHandler>(
            gitPort,
            "Listened git://localhost:{gitPort}, the Git reverse proxy service of GitHub has been started.",
            gitPort);
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
        options.ListenAndLog(
            IReverseProxyService.Constants.Instance.ProxyIp,
            httpPort,
            static _ => { },
            "Listened http://{ProxyIp}:{httpPort}, HTTP reverse proxy service startup completed.",
            IReverseProxyService.Constants.Instance.ProxyIp,
            httpPort);
    }

    /// <summary>
    /// 监听 CRL（证书吊销列表）服务，供 Schannel 等客户端完成本地 MITM 证书的吊销检查
    /// </summary>
    /// <param name="options"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ListenCrlReverseProxy(this KestrelServerOptions options)
    {
        var crlPort = IReverseProxyConfig.CrlPort;
        options.ListenAndLog(
            IPAddress.Loopback,
            crlPort,
            static listen => listen.Protocols = HttpProtocols.Http1,
            "Listened http://127.0.0.1:{crlPort}, CRL service startup completed.",
            crlPort);
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
        options.ListenAndLog(
            IReverseProxyService.Constants.Instance.ProxyIp,
            httpsPort,
            static listen =>
            {
                listen.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                listen.UseFlowAnalyze();
                listen.UseTls();
            },
            "Listened https://{ProxyIp}:{httpsPort}, HTTPS reverse proxy service startup completed.",
            IReverseProxyService.Constants.Instance.ProxyIp,
            httpsPort);
    }

    /// <summary>
    /// 监听本地反向代理（SSH / Git 通用）
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void ListenLocalReverseProxy<TConnectionHandler>(this KestrelServerOptions options, int port, string message, params object[] args)
        where TConnectionHandler : ConnectionHandler
    {
        options.ListenLocalhost(port, listen =>
        {
            listen.UseFlowAnalyze();
            listen.UseConnectionHandler<TConnectionHandler>();
        });

        options.GetLogger().LogInformation(message, args);
    }

    /// <summary>
    /// 监听指定地址并记录启动日志
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void ListenAndLog(this KestrelServerOptions options, IPAddress ip, int port, Action<ListenOptions> configure, string message, params object[] args)
    {
        options.Listen(ip, port, configure);
        options.GetLogger().LogInformation(message, args);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ILogger GetLogger(this KestrelServerOptions kestrel)
    {
        var loggerFactory = kestrel.ApplicationServices.GetRequiredService<ILoggerFactory>();
        return loggerFactory.CreateLogger(TAG);
    }

    const string TAG = "KestrelServerOptEx";

}