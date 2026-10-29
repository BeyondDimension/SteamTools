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

        // Preserve a non-default port in the forwarded Host header.
        request.Headers.Host = uri.Authority;
        //request.RequestUri = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttp }.Uri;

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
        EnableMultipleHttp3Connections = true,
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
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(connectTimeout);
            await socket.ConnectAsync(proxyEndPoint, timeout.Token);

            var stream = new NetworkStream(socket, ownsSocket: true);
            switch (DetermineProxyType(proxyUri))
            {
                case ExternalProxyType.Http:
                    await ConnectHttpProxy(stream, context, timeout.Token);
                    break;
                case ExternalProxyType.Socks4:
                    await ConnectSocks4Proxy(stream, context, timeout.Token);
                    break;
                case ExternalProxyType.Socks5:
                    await ConnectSocks5Proxy(stream, context, timeout.Token);
                    break;
            }

            if (context.InitialRequestMessage.RequestUri.Scheme == "https" || context.InitialRequestMessage.RequestUri.Port == 443)
            {
                var requestContext = context.InitialRequestMessage.GetRequestContext();
                var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
                try
                {
                    await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                    {
                        TargetHost = requestContext.TlsSniValue.Value,
                        RemoteCertificateValidationCallback = (_, cert, _, errors) => ValidateServerCertificate(context, cert, errors),
                    }, timeout.Token);
                    return sslStream;
                }
                catch
                {
                    await sslStream.DisposeAsync();
                    throw;
                }
            }

            return stream;
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    private static ExternalProxyType DetermineProxyType(Uri proxyUri) => proxyUri.Scheme switch
    {
        "socks4" => ExternalProxyType.Socks4,
        "socks5" => ExternalProxyType.Socks5,
        _ => ExternalProxyType.Http,
    };

    private async Task ConnectHttpProxy(NetworkStream stream, SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var credentials = webProxy.Credentials as NetworkCredential;
        var authHeader = credentials != null
            ? $"Proxy-Authorization: Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{credentials.UserName}:{credentials.Password}"))}\r\n"
            : string.Empty;

        var host = context.DnsEndPoint.Host;
        if (IPAddress.TryParse(host, out var ip) && ip.AddressFamily == AddressFamily.InterNetworkV6)
            host = $"[{host}]";
        var connectRequest = $"CONNECT {host}:{context.DnsEndPoint.Port} HTTP/1.1\r\n" +
                             $"Host: {host}:{context.DnsEndPoint.Port}\r\n" +
                             $"{authHeader}\r\n";

        var connectBytes = Encoding.ASCII.GetBytes(connectRequest);
        await stream.WriteAsync(connectBytes, cancellationToken);

        // Do not consume tunnel bytes past the CONNECT response headers.
        var buffer = new byte[8192];
        var length = 0;
        var headersComplete = false;
        while (length < buffer.Length)
        {
            await stream.ReadExactlyAsync(buffer.AsMemory(length, 1), cancellationToken);
            length++;
            if (length >= 4 && buffer[length - 4] == '\r' && buffer[length - 3] == '\n' &&
                buffer[length - 2] == '\r' && buffer[length - 1] == '\n')
            {
                headersComplete = true;
                break;
            }
        }
        var response = Encoding.ASCII.GetString(buffer, 0, length);
        var statusLine = response.Split('\n', 2)[0].TrimEnd('\r');
        var status = statusLine.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (!headersComplete || status.Length < 2 || !status[0].StartsWith("HTTP/1.", StringComparison.Ordinal) || status[1] != "200")
            throw new IOException($"Proxy connection failed: {statusLine}");
    }

    private async Task ConnectSocks4Proxy(NetworkStream stream, SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var credentials = webProxy.Credentials as NetworkCredential;
        var userId = credentials?.UserName ?? string.Empty;

        var destinationAddress = await GetIPEndPointsAsync(context.DnsEndPoint, cancellationToken)
            .Where(x => x.AddressFamily == AddressFamily.InterNetwork).FirstAsync(cancellationToken);
        var userBytes = Encoding.ASCII.GetBytes(userId);
        var request = new byte[9 + userBytes.Length];
        request[0] = 0x04;
        request[1] = 0x01;
        request[2] = (byte)(context.DnsEndPoint.Port >> 8);
        request[3] = (byte)context.DnsEndPoint.Port;
        destinationAddress.Address.GetAddressBytes().CopyTo(request, 4);
        userBytes.CopyTo(request, 8);

        await stream.WriteAsync(request, cancellationToken);

        var response = new byte[8];
        await stream.ReadExactlyAsync(response, cancellationToken);

        if (response[1] != 0x5A)
        {
            throw new Exception($"SOCKS4 proxy connection failed: {response[1]}");
        }
    }

    private async Task ConnectSocks5Proxy(NetworkStream stream, SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var credentials = webProxy.Credentials as NetworkCredential;

        // SOCKS5 negotiation may arrive in multiple TCP segments.
        byte authMethod = credentials != null ? (byte)0x02 : (byte)0x00;
        await stream.WriteAsync(new byte[] { 0x05, 0x01, authMethod }, cancellationToken);

        var response = new byte[2];
        await stream.ReadExactlyAsync(response, cancellationToken);

        if (response[0] != 0x05 || response[1] != authMethod)
            throw new IOException("SOCKS5 handshake rejected the offered authentication method.");

        // 如果需要身份验证
        if (response[1] == 0x02 && credentials != null)
        {
            var userBytes = Encoding.ASCII.GetBytes(credentials.UserName);
            var passBytes = Encoding.ASCII.GetBytes(credentials.Password);
            if (userBytes.Length is 0 or > 255 || passBytes.Length is 0 or > 255)
                throw new ArgumentException("SOCKS5 credentials must contain 1 to 255 bytes each.");
            var authRequest = new byte[3 + userBytes.Length + passBytes.Length];
            authRequest[0] = 0x01;
            authRequest[1] = (byte)userBytes.Length;
            userBytes.CopyTo(authRequest, 2);
            authRequest[2 + userBytes.Length] = (byte)passBytes.Length;
            passBytes.CopyTo(authRequest, 3 + userBytes.Length);

            await stream.WriteAsync(authRequest, cancellationToken);

            var authResponse = new byte[2];
            await stream.ReadExactlyAsync(authResponse, cancellationToken);

            if (authResponse[0] != 0x01 || authResponse[1] != 0x00)
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

        if (addressType == 3 && addressBytes.Length is 0 or > 255)
            throw new ArgumentException("SOCKS5 host names must contain 1 to 255 bytes.");
        var addressLength = addressType == 3 ? 1 : 0;
        var request = new byte[4 + addressLength + addressBytes.Length + 2];
        request[0] = 0x05;
        request[1] = 0x01;
        request[3] = addressType;
        if (addressLength != 0)
            request[4] = (byte)addressBytes.Length;
        addressBytes.CopyTo(request, 4 + addressLength);
        request[^2] = (byte)(context.DnsEndPoint.Port >> 8);
        request[^1] = (byte)context.DnsEndPoint.Port;
        await stream.WriteAsync(request, cancellationToken);

        var connectResponse = new byte[258];
        await stream.ReadExactlyAsync(connectResponse.AsMemory(0, 4), cancellationToken);
        if (connectResponse[0] != 0x05 || connectResponse[1] != 0x00 || connectResponse[2] != 0)
            throw new IOException($"SOCKS5 connection failed: {connectResponse[1]}");
        var boundAddressLength = connectResponse[3] switch
        {
            1 => 4,
            4 => 16,
            3 => -1,
            _ => throw new IOException("Invalid SOCKS5 response address type."),
        };
        if (boundAddressLength == -1)
        {
            await stream.ReadExactlyAsync(connectResponse.AsMemory(0, 1), cancellationToken);
            boundAddressLength = connectResponse[0];
        }
        await stream.ReadExactlyAsync(connectResponse.AsMemory(0, boundAddressLength + 2), cancellationToken);
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
        try
        {
            await socket.ConnectAsync(ipEndPoint, cancellationToken);
            var stream = new NetworkStream(socket, ownsSocket: true);
            var requestContext = context.InitialRequestMessage.GetRequestContext();
            if (!requestContext.IsHttps)
                return stream;

            var tlsSniValue = requestContext.TlsSniValue.WithIPAddress(ipEndPoint.Address);
            var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
            try
            {
                await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                {
                    TargetHost = tlsSniValue.Value,
                    RemoteCertificateValidationCallback = (_, cert, _, errors) => ValidateServerCertificate(context, cert, errors),
                }, cancellationToken);
                return sslStream;
            }
            catch
            {
                await sslStream.DisposeAsync();
                throw;
            }
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    bool ValidateServerCertificate(SocketsHttpConnectionContext context, X509Certificate? cert, SslPolicyErrors errors)
    {
        if ((errors & ~SslPolicyErrors.RemoteCertificateNameMismatch) != SslPolicyErrors.None)
            return false;
        if (errors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
        {
            if (domainConfig.TlsIgnoreNameMismatch)
                return true;
            var domain = context.DnsEndPoint.Host;
            return ReadDnsNames(cert).Any(dns => IsMatch(dns, domain));
        }
        return errors == SslPolicyErrors.None;
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