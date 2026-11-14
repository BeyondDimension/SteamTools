// https://github.com/dotnetcore/FastGithub/blob/58f79ddc19410c92b18e8d4de1c4b61376e97be7/FastGithub.HttpServer/TcpMiddlewares/TunnelMiddleware.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// 隧道连接创建器
/// </summary>
sealed class TunnelConnector
{
    readonly IReverseProxyConfig reverseProxyConfig;
    readonly IDomainResolver domainResolver;
    readonly TimeSpan connectTimeout = TimeSpan.FromSeconds(10d);

    public TunnelConnector(
        IReverseProxyConfig reverseProxyConfig,
        IDomainResolver domainResolver)
    {
        this.reverseProxyConfig = reverseProxyConfig;
        this.domainResolver = domainResolver;
    }

    /// <summary>
    /// 创建连接
    /// </summary>
    /// <param name="host"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="AggregateException"></exception>
    public async Task<Stream> CreateConnectionAsync(HostString host, CancellationToken cancellationToken)
    {
        var innerExceptions = new List<Exception>();
        await foreach (var endPoint in GetUpstreamEndPointsAsync(host, cancellationToken))
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                using var timeoutTokenSource = new CancellationTokenSource(connectTimeout);
                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);
                await socket.ConnectAsync(endPoint, linkedTokenSource.Token);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (Exception ex)
            {
                socket.Dispose();
                cancellationToken.ThrowIfCancellationRequested();
                innerExceptions.Add(ex);
            }
        }
        throw new AggregateException($"Unable to connect to {host}", innerExceptions);
    }

    /// <summary>
    /// 获取目标终节点
    /// </summary>
    /// <param name="host"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    async IAsyncEnumerable<EndPoint> GetUpstreamEndPointsAsync(HostString host, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const int HTTPS_PORT = 443;
        var targetHost = host.Host;
        var targetPort = host.Port ?? HTTPS_PORT;

        if (IPAddress.TryParse(targetHost, out var address) == true)
        {
            yield return new IPEndPoint(address, targetPort);
        }
        else if (reverseProxyConfig.IsMatch(targetHost) == false)
        {
            yield return new DnsEndPoint(targetHost, targetPort);
        }
        else
        {
            var dnsEndPoint = new DnsEndPoint(targetHost, targetPort);
            await foreach (var item in domainResolver.ResolveAsync(dnsEndPoint, cancellationToken))
            {
                yield return new IPEndPoint(item, targetPort);
            }
        }
    }
}
