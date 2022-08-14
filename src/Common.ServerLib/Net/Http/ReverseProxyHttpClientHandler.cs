using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

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
        var ipEndPoints = await Dns.GetHostAddressesAsync("steamcommunity.rmbgame.net", cancellationToken);

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

    static readonly HttpRequestOptionsKey<RequestContext> key = new(nameof(RequestContext));

    /// <summary>
    /// 获取 <see cref="RequestContext"/>
    /// </summary>
    /// <param name="httpRequestMessage"></param>
    /// <returns></returns>
    static RequestContext GetRequestContext(HttpRequestMessage httpRequestMessage)
    {
        return httpRequestMessage.Options.TryGetValue(key, out var requestContext)
            ? requestContext
            : throw new InvalidOperationException($"Please call first SetRequestContext.");
    }

    /// <summary>
    /// 请求上下文
    /// </summary>
    sealed class RequestContext
    {
        /// <summary>
        /// 获取或设置是否为 HTTPS 请求
        /// </summary>
        public bool IsHttps { get; }

        /// <summary>
        /// 获取或设置Sni值
        /// </summary>
        public TlsSniPattern TlsSniValue { get; }

        public RequestContext(bool isHttps, TlsSniPattern tlsSniValue)
        {
            IsHttps = isHttps;
            TlsSniValue = tlsSniValue;
        }
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

        var requestContext = GetRequestContext(context.InitialRequestMessage);
        if (requestContext.IsHttps == false)
        {
            return stream;
        }
        var tlsSniValue = requestContext.TlsSniValue.WithIPAddress(ipEndPoint.Address);
        var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
        await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
        {
            TargetHost = tlsSniValue.Value,
            RemoteCertificateValidationCallback = ValidateServerCertificate
        }, cancellationToken);

        return sslStream;

        // 验证证书有效性
        static bool ValidateServerCertificate(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors errors)
        {
            return true;
        }
    }

    /// <summary>
    /// 解析为 <see cref="IPEndPoint"/>
    /// </summary>
    /// <param name="dnsEndPoint"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    async IAsyncEnumerable<IPEndPoint> GetIPEndPointsAsync(DnsEndPoint dnsEndPoint, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (IPAddress.TryParse(dnsEndPoint.Host, out var address))
        {
            yield return new IPEndPoint(address, dnsEndPoint.Port);
        }
        else
        {
            if (domainConfig.IPAddress != null)
            {
                yield return new IPEndPoint(domainConfig.IPAddress, dnsEndPoint.Port);
            }

            if (domainConfig.ForwardDestination != null)
            {
                await foreach (var item in domainResolver.ResolveAsync(new DnsEndPoint(domainConfig.ForwardDestination, dnsEndPoint.Port), cancellationToken))
                {
                    yield return new IPEndPoint(item, dnsEndPoint.Port);
                }
            }

            await foreach (var item in domainResolver.ResolveAsync(dnsEndPoint, cancellationToken))
            {
                yield return new IPEndPoint(item, dnsEndPoint.Port);
            }
        }
    }
}
