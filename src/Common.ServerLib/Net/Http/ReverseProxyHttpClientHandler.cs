using System.Net.Security;
using System.Net.Sockets;

namespace System.Net.Http;

public sealed class ReverseProxyHttpClientHandler : DelegatingHandler
{
    readonly TimeSpan connectTimeout = TimeSpan.FromSeconds(10d);

    public ReverseProxyHttpClientHandler()
    {
        InnerHandler = new SocketsHttpHandler()
        {
            Proxy = GeneralHttpClientFactory.HttpNoProxy.Instance,
            UseProxy = false,
            UseCookies = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            ConnectCallback = ConnectCallback,
        };
    }

    /// <summary>
    /// 连接回调
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    async ValueTask<Stream> ConnectCallback(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var innerExceptions = new List<Exception>();
        var ipEndPoints = (await Dns.GetHostAddressesAsync("steamcommunity.rmbgame.net", cancellationToken)).Select(s => new IPEndPoint(s, context.DnsEndPoint.Port));

        foreach (var ipEndPoint in ipEndPoints)
        {
            try
            {
                using var timeoutTokenSource = new CancellationTokenSource(connectTimeout);
                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, cancellationToken);
                return await ConnectAsync(context, ipEndPoint, linkedTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                cancellationToken.ThrowIfCancellationRequested();
                innerExceptions.Add(new TimeoutException(
                    $"HTTP connection to {ipEndPoint.Address} timed out."));
            }
            catch (Exception ex)
            {
                innerExceptions.Add(ex);
            }
        }

        throw new AggregateException("Could not find any IP that can be successfully connected.", innerExceptions);
    }

    /// <summary>
    /// 建立连接
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ipEndPoint"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    static async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, IPEndPoint ipEndPoint, CancellationToken cancellationToken)
    {
        var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(ipEndPoint, cancellationToken);
        var stream = new NetworkStream(socket, ownsSocket: true);

        if (context.DnsEndPoint.Port != 443)
        {
            return stream;
        }

        var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
        await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
        {
            TargetHost = string.Empty,
            RemoteCertificateValidationCallback = (_, _, _, _) => true,
        }, cancellationToken);

        return sslStream;
    }
}
