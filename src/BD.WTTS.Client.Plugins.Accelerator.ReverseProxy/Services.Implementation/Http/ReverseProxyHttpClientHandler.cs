// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Http/HttpClientHandler.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class ReverseProxyHttpClientHandler : DelegatingHandler
{
    readonly IDomainConfig domainConfig;
    readonly IDomainResolver domainResolver;
    readonly IWebProxy webProxy;
    readonly TimeSpan connectTimeout = TimeSpan.FromSeconds(10d);

    public ReverseProxyHttpClientHandler(IDomainConfig domainConfig, IDomainResolver domainResolver, IWebProxy webProxy)
    {
        this.domainConfig = domainConfig;
        this.domainResolver = domainResolver;
        this.webProxy = webProxy;
        InnerHandler = CreateSocketsHttpHandler();
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri ??
            throw new ApplicationException("The requested URI must be specified.");

        // 请求上下文信息
        var isHttps = uri.Scheme == Uri.UriSchemeHttps;
        var tlsSniValue = domainConfig.GetTlsSniPattern().WithDomain(uri.Host).WithRandom();
        request.SetRequestContext(new RequestContext(isHttps, tlsSniValue));

        // 设置请求头 host，因为连接中已带端口号故 修改协议为 http
        request.Headers.Host = uri.Host;
        request.RequestUri = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttp }.Uri;

        if (domainConfig.Timeout != null)
        {
            using var timeoutTokenSource = new CancellationTokenSource(domainConfig.Timeout.Value);
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);
            return await base.SendAsync(request, linkedTokenSource.Token);
        }
        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// 创建转发代理的 <see cref="HttpMessageHandler"/>
    /// </summary>
    /// <returns></returns>
    HttpHandlerType CreateSocketsHttpHandler() => new()
    {
        Proxy = HttpNoProxy.Instance,
        UseProxy = false,
        PreAuthenticate = false,
        UseCookies = false,
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.None,
        ConnectCallback = ConnectCallback,
        EnableMultipleHttp2Connections = true,
        RequestHeaderEncodingSelector = (_, _) => Encoding.UTF8,
        ResponseHeaderEncodingSelector = (_, _) => Encoding.UTF8,
    };

    private async ValueTask<Stream> ConnectThroughProxyAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        if (context.InitialRequestMessage.RequestUri == null)
        {
            throw new InvalidOperationException("Request URI is null");
        }
        var proxyUri = webProxy.GetProxy(context.InitialRequestMessage.RequestUri);
        if (proxyUri == null)
        {
            throw new InvalidOperationException("Proxy URI is null");
        }

        var proxyEndPoint = new DnsEndPoint(proxyUri.Host, proxyUri.Port);
        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(proxyEndPoint, cancellationToken);

        var stream = new NetworkStream(socket, ownsSocket: true);

        // 确定代理类型
        var proxyType = DetermineProxyType(proxyUri);

        switch (proxyType)
        {
            case ExternalProxyType.Http:
                await ConnectHttpProxy(stream, context, cancellationToken);
                break;
            case ExternalProxyType.Socks4:
                await ConnectSocks4Proxy(stream, context, cancellationToken);
                break;
            case ExternalProxyType.Socks5:
                await ConnectSocks5Proxy(stream, context, cancellationToken);
                break;
        }

        if (context.InitialRequestMessage.RequestUri.Scheme == "https" || context.InitialRequestMessage.RequestUri.Port == 443)
        {
            var requestContext = context.InitialRequestMessage.GetRequestContext();
            var tlsSniValue = requestContext.TlsSniValue;
            var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
            await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
            {
                TargetHost = tlsSniValue.Value,
                RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true,
            }, cancellationToken);

            return sslStream;
        }

        return stream;
    }

    private ExternalProxyType DetermineProxyType(Uri proxyUri)
    {
        return proxyUri.Scheme.ToLower() switch
        {
            "socks4" => ExternalProxyType.Socks4,
            "socks5" => ExternalProxyType.Socks5,
            _ => ExternalProxyType.Http
        };

        throw new NotImplementedException("Proxy type determination needs to be implemented");
    }

    private async Task ConnectHttpProxy(NetworkStream stream, SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var credentials = webProxy.Credentials as NetworkCredential;
        var authHeader = credentials != null
            ? $"Proxy-Authorization: Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{credentials.UserName}:{credentials.Password}"))}\r\n"
            : string.Empty;

        var connectRequest = $"CONNECT {context.DnsEndPoint.Host}:{context.DnsEndPoint.Port} HTTP/1.1\r\n" +
                             $"Host: {context.DnsEndPoint.Host}:{context.DnsEndPoint.Port}\r\n" +
                             $"{authHeader}\r\n";

        var connectBytes = Encoding.ASCII.GetBytes(connectRequest);
        await stream.WriteAsync(connectBytes, cancellationToken);

        var buffer = new byte[1024];
        var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
        var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

        if (!response.StartsWith("HTTP/1.1 200"))
        {
            throw new Exception($"Proxy connection failed: {response}");
        }
    }

    private async Task ConnectSocks4Proxy(NetworkStream stream, SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var credentials = webProxy.Credentials as NetworkCredential;
        var userId = credentials?.UserName ?? string.Empty;

        var destinationAddress = await GetIPEndPointsAsync(context.DnsEndPoint, cancellationToken).FirstAsync();
        var portBytes = BitConverter.GetBytes((ushort)context.DnsEndPoint.Port);
        if (BitConverter.IsLittleEndian) Array.Reverse(portBytes);

        var request = new byte[] { 0x04, 0x01 }
            .Concat(portBytes)
            .Concat(destinationAddress.Address.GetAddressBytes())
            .Concat(Encoding.ASCII.GetBytes(userId))
            .Concat(new byte[] { 0x00 })
            .ToArray();

        await stream.WriteAsync(request, cancellationToken);

        var response = new byte[8];
        await stream.ReadAsync(response, cancellationToken);

        if (response[1] != 0x5A)
        {
            throw new Exception($"SOCKS4 proxy connection failed: {response[1]}");
        }
    }

    private async Task ConnectSocks5Proxy(NetworkStream stream, SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var credentials = webProxy.Credentials as NetworkCredential;

        // SOCKS5 握手
        byte[] authMethod = credentials != null ? new byte[] { 0x02 } : new byte[] { 0x00 };
        await stream.WriteAsync(new byte[] { 0x05, 0x01 }.Concat(authMethod).ToArray(), cancellationToken);

        var response = new byte[2];
        await stream.ReadAsync(response, cancellationToken);

        if (response[0] != 0x05)
        {
            throw new Exception("SOCKS5 handshake failed");
        }

        // 如果需要身份验证
        if (response[1] == 0x02 && credentials != null)
        {
            var userBytes = Encoding.ASCII.GetBytes(credentials.UserName);
            var passBytes = Encoding.ASCII.GetBytes(credentials.Password);
            var authRequest = new byte[] { 0x01, (byte)userBytes.Length }
                .Concat(userBytes)
                .Concat(new byte[] { (byte)passBytes.Length })
                .Concat(passBytes)
                .ToArray();

            await stream.WriteAsync(authRequest, cancellationToken);

            var authResponse = new byte[2];
            await stream.ReadAsync(authResponse, cancellationToken);

            if (authResponse[1] != 0x00)
            {
                throw new Exception("SOCKS5 authentication failed");
            }
        }

        // 发送连接请求
        byte[] addressBytes;
        byte addressType;
        if (IPAddress.TryParse(context.DnsEndPoint.Host, out IPAddress? ipAddress))
        {
            addressBytes = ipAddress.GetAddressBytes();
            addressType = (byte)(ipAddress.AddressFamily == AddressFamily.InterNetwork ? 1 : 4);
        }
        else
        {
            addressBytes = Encoding.ASCII.GetBytes(context.DnsEndPoint.Host);
            addressType = 3; // 域名
        }

        var request = new byte[] { 0x05, 0x01, 0x00, addressType }
            .Concat(addressType == 3 ? new[] { (byte)addressBytes.Length } : Array.Empty<byte>())
            .Concat(addressBytes)
            .Concat(BitConverter.GetBytes((ushort)context.DnsEndPoint.Port).Reverse())
            .ToArray();

        await stream.WriteAsync(request, cancellationToken);

        var connectResponse = new byte[10];
        await stream.ReadAsync(connectResponse, cancellationToken);

        if (connectResponse[1] != 0x00)
        {
            throw new Exception($"SOCKS5 connection failed: {connectResponse[1]}");
        }
    }

    /// <summary>
    /// 连接回调
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    async ValueTask<Stream> ConnectCallback(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        if (webProxy != null && !HttpNoProxy.IsNoProxy(webProxy))
        {
            return await ConnectThroughProxyAsync(context, cancellationToken);
        }

        var innerExceptions = new List<Exception>();
        var ipEndPoints = GetIPEndPointsAsync(context.DnsEndPoint, cancellationToken);

        await foreach (var ipEndPoint in ipEndPoints)
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
    async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, IPEndPoint ipEndPoint, CancellationToken cancellationToken)
    {
        var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(ipEndPoint, cancellationToken);
        var stream = new NetworkStream(socket, ownsSocket: true);

        var requestContext = context.InitialRequestMessage.GetRequestContext();
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
        bool ValidateServerCertificate(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors errors)
        {
            if (errors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
            {
                if (domainConfig.TlsIgnoreNameMismatch == true)
                {
                    return true;
                }

                var domain = context.DnsEndPoint.Host;
                var dnsNames = ReadDnsNames(cert);
                return dnsNames.Any(dns => IsMatch(dns, domain));
            }

            return errors == SslPolicyErrors.None;
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

    /// <summary>
    /// 读取使用的 DNS 名称
    /// </summary>
    /// <param name="cert"></param>
    /// <returns></returns>
    static IEnumerable<string> ReadDnsNames(X509Certificate? cert)
    {
        if (cert is X509Certificate2 x509)
        {
            var extension = x509.Extensions
                .OfType<X509SubjectAlternativeNameExtension>().FirstOrDefault();
            if (extension != null)
            {
                return extension.EnumerateDnsNames();
            }
        }
        return Array.Empty<string>();

        //if (cert == null)
        //{
        //    yield break;
        //}

        //var parser = new Org.BouncyCastle.X509.X509CertificateParser();
        //var x509Cert = parser.ReadCertificate(cert.GetRawCertData());
        //var subjects = x509Cert.GetSubjectAlternativeNames();
        //if (subjects == null)
        //{
        //    yield break;
        //}

        //foreach (var subject in subjects)
        //{
        //    if (subject is IList list)
        //    {
        //        if (list.Count >= 2 && list[0] is int nameType && nameType == 2)
        //        {
        //            var dnsName = list[1]?.ToString();
        //            if (dnsName != null)
        //            {
        //                yield return dnsName;
        //            }
        //        }
        //    }
        //}
    }

    /// <summary>
    /// 比较域名
    /// </summary>
    /// <param name="dnsName"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    static bool IsMatch(string dnsName, string? domain)
    {
        if (domain == null)
        {
            return false;
        }
        if (dnsName == domain)
        {
            return true;
        }
        if (dnsName[0] == '*')
        {
            return domain.EndsWith(dnsName[1..]);
        }
        return false;
    }
}