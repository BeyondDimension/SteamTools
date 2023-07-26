// https://github.com/dotnetcore/FastGithub/blob/58f79ddc19410c92b18e8d4de1c4b61376e97be7/FastGithub.HttpServer/TcpMiddlewares/HttpProxyMiddleware.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// 正向代理中间件
/// </summary>
sealed class HttpProxyMiddleware
{
    private readonly HttpParser<HttpRequestHandler> httpParser = new();
    private readonly byte[] http200 = Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection Established\r\n\r\n");
    private readonly byte[] http400 = Encoding.ASCII.GetBytes("HTTP/1.1 400 Bad Request\r\n\r\n");

    /// <summary>
    /// 执行中间件
    /// </summary>
    /// <param name="next"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(ConnectionDelegate next, ConnectionContext context)
    {
        var input = context.Transport.Input;
        var output = context.Transport.Output;
        var request = new HttpRequestHandler();

        while (context.ConnectionClosed.IsCancellationRequested == false)
        {
            var result = await input.ReadAsync();
            if (result.IsCanceled)
            {
                break;
            }

            try
            {
                if (this.ParseRequest(result, request, out var consumed))
                {
                    if (request.ProxyProtocol == ProxyProtocol.TunnelProxy)
                    {
                        input.AdvanceTo(consumed);
                        await output.WriteAsync(this.http200, context.ConnectionClosed);
                    }
                    else
                    {
                        input.AdvanceTo(result.Buffer.Start);
                    }

                    context.Features.Set<IHttpProxyFeature>(request);
                    await next(context);

                    break;
                }
                else
                {
                    input.AdvanceTo(result.Buffer.Start, result.Buffer.End);
                }

                if (result.IsCompleted)
                {
                    break;
                }
            }
            catch (Exception)
            {
                await output.WriteAsync(this.http400, context.ConnectionClosed);
                break;
            }
        }
    }

    /// <summary>
    /// 解析http请求
    /// </summary>
    /// <param name="result"></param>
    /// <param name="requestHandler"></param>
    /// <param name="consumed"></param>
    /// <returns></returns>
    private bool ParseRequest(ReadResult result, HttpRequestHandler request, out SequencePosition consumed)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        if (this.httpParser.ParseRequestLine(request, ref reader) &&
            this.httpParser.ParseHeaders(request, ref reader))
        {
            consumed = reader.Position;
            return true;
        }
        else
        {
            consumed = default;
            return false;
        }
    }

    /// <summary>
    /// 代理请求处理器
    /// </summary>
    private class HttpRequestHandler : IHttpRequestLineHandler, IHttpHeadersHandler, IHttpProxyFeature
    {
        private AspNetCoreHttpMethod method;

        public HostString ProxyHost { get; private set; }

        public ProxyProtocol ProxyProtocol
        {
            get
            {
                if (this.ProxyHost.HasValue == false)
                {
                    return ProxyProtocol.None;
                }
                if (this.method == AspNetCoreHttpMethod.Connect)
                {
                    return ProxyProtocol.TunnelProxy;
                }
                return ProxyProtocol.HttpProxy;
            }
        }

        void IHttpRequestLineHandler.OnStartLine(HttpVersionAndMethod versionAndMethod, TargetOffsetPathLength targetPath, Span<byte> startLine)
        {
            this.method = versionAndMethod.Method;
            var host = Encoding.ASCII.GetString(startLine.Slice(targetPath.Offset, targetPath.Length));
            if (versionAndMethod.Method == AspNetCoreHttpMethod.Connect)
            {
                this.ProxyHost = HostString.FromUriComponent(host);
            }
            else if (Uri.TryCreate(host, UriKind.Absolute, out var uri))
            {
                this.ProxyHost = HostString.FromUriComponent(uri);
            }
        }

        void IHttpHeadersHandler.OnHeader(ReadOnlySpan<byte> name, ReadOnlySpan<byte> value)
        {
        }

        void IHttpHeadersHandler.OnHeadersComplete(bool endStream)
        {
        }

        void IHttpHeadersHandler.OnStaticIndexedHeader(int index)
        {
        }

        void IHttpHeadersHandler.OnStaticIndexedHeader(int index, ReadOnlySpan<byte> value)
        {
        }
    }
}