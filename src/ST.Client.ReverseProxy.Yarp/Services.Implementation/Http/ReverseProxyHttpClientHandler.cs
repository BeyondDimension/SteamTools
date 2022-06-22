// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Http/HttpClientHandler.cs

using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Application.Models;
using System.Collections;

namespace System.Application.Services.Implementation.Http;

sealed class ReverseProxyHttpClientHandler : DelegatingHandler
{
    readonly IDomainConfig domainConfig;
    readonly IDomainResolver domainResolver;
    readonly TimeSpan connectTimeout = TimeSpan.FromSeconds(10d);

    public ReverseProxyHttpClientHandler(IDomainConfig domainConfig, IDomainResolver domainResolver)
    {
        this.domainConfig = domainConfig;
        this.domainResolver = domainResolver;
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
        var uri = request.RequestUri;
        if (uri == null)
        {
            throw new ApplicationException("The requested URI must be specified.");
        }

        // 请求上下文信息
        var isHttps = uri.Scheme == Uri.UriSchemeHttps;
        var tlsSniValue = domainConfig.GetTlsSniPattern().WithDomain(uri.Host).WithRandom();
        request.SetRequestContext(new RequestContext(isHttps, tlsSniValue));

        // 设置请求头 host，修改协议为 http
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
    SocketsHttpHandler CreateSocketsHttpHandler() => new SocketsHttpHandler
    {
        Proxy = null,
        UseProxy = false,
        UseCookies = false,
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.None,
        ConnectCallback = ConnectCallback,
    };

    /// <summary>
    /// 连接回调
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    async ValueTask<Stream> ConnectCallback(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
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

            if (domainConfig.DomainName != null)
            {
                await foreach (var item in domainResolver.ResolveAsync(new DnsEndPoint(domainConfig.DomainName, dnsEndPoint.Port), cancellationToken))
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
        if (cert == null)
        {
            yield break;
        }
        var parser = new Org.BouncyCastle.X509.X509CertificateParser();
        var x509Cert = parser.ReadCertificate(cert.GetRawCertData());
        var subjects = x509Cert.GetSubjectAlternativeNames();
        if (subjects == null)
        {
            yield break;
        }

        foreach (var subject in subjects)
        {
            if (subject is IList list)
            {
                if (list.Count >= 2 && list[0] is int nameType && nameType == 2)
                {
                    var dnsName = list[1]?.ToString();
                    if (dnsName != null)
                    {
                        yield return dnsName;
                    }
                }
            }
        }
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
