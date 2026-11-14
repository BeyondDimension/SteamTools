// https://github.com/dotnetcore/FastGithub/blob/58f79ddc19410c92b18e8d4de1c4b61376e97be7/FastGithub.HttpServer/TcpMiddlewares/TunnelMiddleware.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// 隧道中间件
/// </summary>
sealed class TunnelMiddleware
{
    readonly TunnelConnector tunnelConnector;

    /// <summary>
    /// 隧道中间件
    /// </summary>
    /// <param name="tunnelConnector"></param>
    public TunnelMiddleware(TunnelConnector tunnelConnector)
    {
        this.tunnelConnector = tunnelConnector;
    }

    /// <summary>
    /// 执行中间件
    /// </summary>
    /// <param name="next"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(ConnectionDelegate next, ConnectionContext context)
    {
        var proxyFeature = context.Features.Get<IHttpProxyFeature>();
        if (proxyFeature == null || // 非代理
            proxyFeature.ProxyProtocol != ProxyProtocol.TunnelProxy || // 非隧道代理
            context.Features.Get<ITlsConnectionFeature>() != null) // 经过隧道的https
        {
            await next(context);
        }
        else
        {
            var transport = context.Features.Get<IConnectionTransportFeature>()?.Transport;
            if (transport != null)
            {
                var cancellationToken = context.ConnectionClosed;
                using var connection = await tunnelConnector.CreateConnectionAsync(proxyFeature.ProxyHost, cancellationToken);

                var task1 = connection.CopyToAsync(transport.Output, cancellationToken);
                var task2 = transport.Input.CopyToAsync(connection, cancellationToken);
                await Task.WhenAny(task1, task2);
            }
        }
    }
}