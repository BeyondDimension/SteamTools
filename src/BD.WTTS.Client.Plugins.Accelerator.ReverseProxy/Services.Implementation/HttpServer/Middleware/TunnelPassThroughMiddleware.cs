// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// 隧道透传中间件
/// <para>未启用加速的域名不做 Tls 中间人解密，直接在客户端与目标服务器之间转发字节流</para>
/// <para>本地根证书签发的证书没有吊销信息，Windows 搜索、Microsoft Store、Git 等强制校验吊销状态的
/// 客户端会拒绝握手（CRYPT_E_NO_REVOCATION_CHECK），导致这些应用在开启系统代理后无法联网</para>
/// </summary>
sealed class TunnelPassThroughMiddleware
{
    /// <summary>
    /// 永不进行 Tls 中间人解密的域名
    /// <para>这些域名的客户端做证书固定（pinning），本地根证书签发的证书不可能通过校验，
    /// 无论是否命中加速规则都必须透传</para>
    /// </summary>
    static readonly DomainPattern[] NeverInterceptHosts = new DomainPattern[]
    {
        // Microsoft Store 源（winget 的 msstore）固定到微软根 CA
        new("*.dsx.mp.microsoft.com"),
    };

    readonly IReverseProxyConfig reverseProxyConfig;
    readonly TunnelConnector tunnelConnector;
    readonly ILogger<TunnelPassThroughMiddleware> logger;

    public TunnelPassThroughMiddleware(
        IReverseProxyConfig reverseProxyConfig,
        TunnelConnector tunnelConnector,
        ILogger<TunnelPassThroughMiddleware> logger)
    {
        this.reverseProxyConfig = reverseProxyConfig;
        this.tunnelConnector = tunnelConnector;
        this.logger = logger;
    }

    /// <summary>
    /// 执行中间件
    /// </summary>
    /// <param name="next"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(ConnectionDelegate next, ConnectionContext context)
    {
        var transport = context.Features.Get<IConnectionTransportFeature>()?.Transport;
        var proxyFeature = context.Features.Get<IHttpProxyFeature>();
        if (transport == null ||
            proxyFeature == null ||
            ShouldPassThrough(proxyFeature) == false)
        {
            await next(context);
            return;
        }

        var cancellationToken = context.ConnectionClosed;
        logger.LogDebug("PassThrough {ProxyHost}", proxyFeature.ProxyHost);
        using var connection = await tunnelConnector.CreateConnectionAsync(proxyFeature.ProxyHost, cancellationToken);

        var task1 = connection.CopyToAsync(transport.Output, cancellationToken);
        var task2 = transport.Input.CopyToAsync(connection, cancellationToken);
        await Task.WhenAny(task1, task2);
    }

    /// <summary>
    /// 是否需要透传
    /// </summary>
    /// <param name="proxyFeature"></param>
    /// <returns></returns>
    bool ShouldPassThrough(IHttpProxyFeature proxyFeature)
    {
        if (proxyFeature.ProxyProtocol != ProxyProtocol.TunnelProxy)
        {
            return false;
        }

        // 启用二级代理时保持原有行为，未配置加速的域名也交由二级代理转发
        if (reverseProxyConfig.Service.TwoLevelAgentEnable)
        {
            return false;
        }

        var host = proxyFeature.ProxyHost.Host;
        if (string.IsNullOrEmpty(host))
        {
            return false;
        }

        // 证书固定的域名优先于其它条件判定，否则仍会被解密而导致客户端拒绝握手
        if (IsNeverIntercept(host))
        {
            return true;
        }

        try
        {
            // 加速域名与需要注入脚本的域名仍然解密
            if (reverseProxyConfig.IsMatch(host))
            {
                return false;
            }
            return IsScriptMatched(host) == false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 是否属于永不进行 Tls 中间人解密的域名
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    static bool IsNeverIntercept(string host)
    {
        foreach (var item in NeverInterceptHosts)
        {
            if (item.IsMatch(host))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 是否需要为该域名注入脚本
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    bool IsScriptMatched(string host)
    {
        // 脚本的匹配规则基于完整 Url（如 *://host/*），此处以根路径拼接 Url 进行匹配
        return reverseProxyConfig.TryGetScriptConfig($"https://{host}/", out _) ||
            reverseProxyConfig.TryGetScriptConfig($"http://{host}/", out _);
    }
}
