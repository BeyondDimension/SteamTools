// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/HttpProxyMiddleware.cs

using System.Application.Models;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Yarp.ReverseProxy.Forwarder;

namespace System.Application.Services.Implementation.HttpServer.Middleware;

/// <summary>
/// Http 代理中间件
/// </summary>
[Obsolete]
sealed class HttpProxyMiddleware
{
    const string LOCALHOST = "localhost";
    const int HTTP_PORT = 80;
    const int HTTPS_PORT = 443;

    readonly IReverseProxyConfig reverseProxyConfig;
    readonly IDomainResolver domainResolver;
    readonly IHttpForwarder httpForwarder;
    readonly HttpReverseProxyMiddleware httpReverseProxy;

    readonly HttpMessageInvoker defaultHttpClient;
    readonly TimeSpan connectTimeout = TimeSpan.FromSeconds(10d);

    static HttpProxyMiddleware()
    {
        // https://github.com/dotnet/aspnetcore/issues/37421
        var authority = typeof(HttpParser<>).Assembly
            .GetType("Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure.HttpCharacters")?
            .GetField("_authority", BindingFlags.NonPublic | BindingFlags.Static)?
            .GetValue(null);

        if (authority is bool[] authorityArray)
        {
            authorityArray['-'] = true;
        }
    }

    public HttpProxyMiddleware(
        IReverseProxyConfig reverseProxyConfig,
        IDomainResolver domainResolver,
        IHttpForwarder httpForwarder,
        HttpReverseProxyMiddleware httpReverseProxy)
    {
        this.reverseProxyConfig = reverseProxyConfig;
        this.domainResolver = domainResolver;
        this.httpForwarder = httpForwarder;
        this.httpReverseProxy = httpReverseProxy;

        defaultHttpClient = new HttpMessageInvoker(CreateDefaultHttpHandler(), disposeHandler: false);
    }

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host;
        if (IsReverseProxyServer(host))
        {
            var proxyPac = CreateProxyPac(host);
            context.Response.ContentType = "application/x-ns-proxy-autoconfig";
            context.Response.Headers.Add("Content-Disposition", $"attachment;filename=proxy.pac");
            await context.Response.WriteAsync(proxyPac);
        }
        else if (context.Request.Method == HttpMethods.Connect)
        {
            var cancellationToken = context.RequestAborted;
            using var connection = await CreateConnectionAsync(host, cancellationToken);
            var responseFeature = context.Features.Get<IHttpResponseFeature>();
            if (responseFeature != null)
            {
                responseFeature.ReasonPhrase = "Connection Established";
            }
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.CompleteAsync();

            var transport = context.Features.Get<IConnectionTransportFeature>()?.Transport;
            if (transport != null)
            {
                var task1 = connection.CopyToAsync(transport.Output, cancellationToken);
                var task2 = transport.Input.CopyToAsync(connection, cancellationToken);
                await Task.WhenAny(task1, task2);
            }
        }
        else
        {
            await httpReverseProxy.InvokeAsync(context, async next =>
            {
                var destinationPrefix = $"{context.Request.Scheme}://{context.Request.Host}";
                await httpForwarder.SendAsync(context, destinationPrefix, defaultHttpClient);
            });
        }
    }

    /// <summary>
    /// 是否为反向代理服务
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    bool IsReverseProxyServer(HostString host)
    {
        if (host.Port != reverseProxyConfig.HttpProxyPort)
        {
            return false;
        }

        if (host.Host == LOCALHOST)
        {
            return true;
        }

        return IPAddress.TryParse(host.Host, out var address) && IPAddress.IsLoopback(address);
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
        foreach (var domain in reverseProxyConfig.GetDomainPatterns())
        {
            buidler.AppendLine($"    if (shExpMatch(host, '{domain}')) return pac;");
        }
        buidler.AppendLine("    return 'DIRECT';");
        buidler.AppendLine("}");
        return buidler.ToString();
    }

    /// <summary>
    /// 创建连接
    /// </summary>
    /// <param name="host"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="AggregateException"></exception>
    async Task<Stream> CreateConnectionAsync(HostString host, CancellationToken cancellationToken)
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
                return new NetworkStream(socket, ownsSocket: false);
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

    async IAsyncEnumerable<EndPoint> GetUpstreamEndPointsAsync(
        HostString host,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
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
        else if (targetPort == HTTP_PORT)
        {
            yield return new IPEndPoint(IPAddress.Loopback, IReverseProxyConfig.HttpPort);
        }
        else if (targetPort == HTTPS_PORT)
        {
            yield return new IPEndPoint(IPAddress.Loopback, IReverseProxyConfig.HttpsPort);
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

    static SocketsHttpHandler CreateDefaultHttpHandler() => new()
    {
        Proxy = null,
        UseProxy = false,
        UseCookies = false,
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.None,
    };
}
