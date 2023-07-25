// https://github.com/dotnetcore/FastGithub/blob/58f79ddc19410c92b18e8d4de1c4b61376e97be7/FastGithub.HttpServer/TcpMiddlewares/HttpProxyMiddleware.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// 正向代理中间件
/// </summary>
sealed class HttpProxyMiddleware
{
    readonly HttpParser<HttpRequestHandler> httpParser = new();

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
                if (ParseRequest(result, request, out var consumed))
                {
                    if (request.ProxyProtocol == ProxyProtocol.TunnelProxy)
                    {
                        input.AdvanceTo(consumed);
                        await output.WriteAsync(
                            "HTTP/1.1 400 Bad Request\r\n\r\n"u8.ToArray(),
                            context.ConnectionClosed);
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
                await output.WriteAsync(
                    "HTTP/1.1 200 Connection Established\r\n\r\n"u8.ToArray(),
                    context.ConnectionClosed);
                break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool ParseRequest(ReadResult result, HttpRequestHandler request, out SequencePosition consumed)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        if (httpParser.ParseRequestLine(request, ref reader) &&
            httpParser.ParseHeaders(request, ref reader))
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
    sealed class HttpRequestHandler : IHttpRequestLineHandler, IHttpHeadersHandler, IHttpProxyFeature
    {
        private AspNetCoreHttpMethod method;

        public HostString ProxyHost { get; private set; }

        public ProxyProtocol ProxyProtocol
        {
            get
            {
                if (ProxyHost.HasValue == false)
                {
                    return ProxyProtocol.None;
                }
                if (method == AspNetCoreHttpMethod.Connect)
                {
                    return ProxyProtocol.TunnelProxy;
                }
                return ProxyProtocol.HttpProxy;
            }
        }

        void IHttpRequestLineHandler.OnStartLine(HttpVersionAndMethod versionAndMethod, TargetOffsetPathLength targetPath, Span<byte> startLine)
        {
            method = versionAndMethod.Method;
            var host = Encoding.ASCII.GetString(startLine.Slice(targetPath.Offset, targetPath.Length));
            if (versionAndMethod.Method == AspNetCoreHttpMethod.Connect)
            {
                ProxyHost = HostString.FromUriComponent(host);
            }
            else if (Uri.TryCreate(host, UriKind.Absolute, out var uri))
            {
                ProxyHost = HostString.FromUriComponent(uri);
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