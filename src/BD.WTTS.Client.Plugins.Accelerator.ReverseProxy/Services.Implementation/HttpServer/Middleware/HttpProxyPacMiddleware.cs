// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// Http 代理策略中间件
/// </summary>
sealed class HttpProxyPacMiddleware
{
    readonly IReverseProxyConfig reverseProxyConfig;

    public HttpProxyPacMiddleware(IReverseProxyConfig reverseProxyConfig)
    {
        this.reverseProxyConfig = reverseProxyConfig;
    }

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Http 请求经过了 HttpProxy 中间件
        var proxyFeature = context.Features.Get<IHttpProxyFeature>();
        if (proxyFeature != null && proxyFeature.ProxyProtocol == ProxyProtocol.None)
        {
            var proxyPac = CreateProxyPac(context.Request.Host);
            context.Response.ContentType = "application/x-ns-proxy-autoconfig";
            context.Response.Headers.Add("Content-Disposition", $"attachment;filename=proxy.pac");
            await context.Response.WriteAsync(proxyPac);
        }
        else
        {
            await next(context);
        }
    }

    /// <summary>
    /// 创建 proxypac 脚本
    /// </summary>
    /// <param name="proxyHost"></param>
    /// <returns></returns>
    string CreateProxyPac(HostString proxyHost)
    {
        var buidler = new StringBuilder();
        buidler.AppendLine("function FindProxyForURL(url, host){");
        buidler.AppendLine($"    var pac = 'PROXY {proxyHost}';");

        foreach (var domains in reverseProxyConfig.GetDomainPatterns())
            foreach (var domain in domains.ToString().Split(DomainPattern.GeneralSeparator))
                if (!string.IsNullOrWhiteSpace(domain))
                    buidler.AppendLine($"    if (shExpMatch(host, '{domain}')) return pac;");

        buidler.AppendLine("    return 'DIRECT';");
        buidler.AppendLine("}");
        return buidler.ToString();
    }
}