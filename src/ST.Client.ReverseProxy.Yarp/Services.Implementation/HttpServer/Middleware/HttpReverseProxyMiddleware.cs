// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/HttpReverseProxyMiddleware.cs

using DynamicData.Kernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.Application.Models;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Net;
using System.Text;
using Yarp.ReverseProxy.Forwarder;

namespace System.Application.Services.Implementation.HttpServer.Middleware;

/// <summary>
/// 反向代理中间件
/// </summary>
sealed class HttpReverseProxyMiddleware
{
    static readonly IDomainConfig defaultDomainConfig = new DomainConfig() { TlsSni = true, };

    readonly IHttpForwarder httpForwarder;
    readonly IReverseProxyHttpClientFactory httpClientFactory;
    readonly IReverseProxyConfig reverseProxyConfig;
    readonly ILogger<HttpReverseProxyMiddleware> logger;

    public HttpReverseProxyMiddleware(
        IHttpForwarder httpForwarder,
        IReverseProxyHttpClientFactory httpClientFactory,
        IReverseProxyConfig reverseProxyConfig,
        ILogger<HttpReverseProxyMiddleware> logger)
    {
        this.httpForwarder = httpForwarder;
        this.httpClientFactory = httpClientFactory;
        this.reverseProxyConfig = reverseProxyConfig;
        this.logger = logger;
    }

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var url = context.Request.GetDisplayUrl();

        var isScriptInject = reverseProxyConfig.TryGetScriptConfig(url, out var scriptConfigs);

        var originalBody = context.Response.Body;
        MemoryStream? memoryStream = null;

        if (isScriptInject)
        {
            memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;
        }

        if (TryGetDomainConfig(url, out var domainConfig) == false)
        {
            await next(context);
            return;
        }

        if (domainConfig == defaultDomainConfig &&
            !reverseProxyConfig.Service.OnlyEnableProxyScript)
        {
            //部分运营商将奇怪的域名解析到127.0.0.1 再此排除这些不支持的代理域名
            var ip = await reverseProxyConfig.DnsAnalysis.AnalysisDomainIpAsync(context.Request.Host.Value, IDnsAnalysisService.DNS_Dnspods).FirstOrDefaultAsync();
            if (ip == null || IPAddress.IsLoopback(ip))
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"域名 {context.Request.Host.Host} 可能已经被 DNS 污染，如果域名为本机域名，请解析为非回环 IP。", Encoding.UTF8);
                return;
            }
        }

        if (domainConfig.Items.Any_Nullable())
            domainConfig = RecursionMatchDomainConfig(url, domainConfig);

        if (domainConfig.Response == null)
        {
            if (reverseProxyConfig.Service.EnableHttpProxyToHttps && context.Request.Scheme == Uri.UriSchemeHttp)
            {
                context.Response.Redirect(Uri.UriSchemeHttps + "://" + context.Request.Host.Host + context.Request.RawUrl());
                return;
            }

            var destinationPrefix = GetDestinationPrefix(context.Request.Scheme, context.Request.Host, domainConfig.Destination);
            var httpClient = httpClientFactory.CreateHttpClient(context.Request.Host.Host, domainConfig);
            if (!string.IsNullOrEmpty(domainConfig.UserAgent))
            {
                context.Request.Headers.UserAgent = domainConfig.UserAgent.Replace("${origin}", context.Request.Headers.UserAgent, StringComparison.OrdinalIgnoreCase);
            }

            var error = await httpForwarder.SendAsync(context, destinationPrefix, httpClient, ForwarderRequestConfig.Empty, HttpTransformer.Empty);

            if (error != ForwarderError.None)
            {
                await HandleErrorAsync(context, error);
            }
            else if (isScriptInject)
            {
                await HandleScriptInject(context, scriptConfigs, memoryStream!, originalBody!);
            }
        }
        else
        {
            context.Response.StatusCode = (int)domainConfig.Response.StatusCode;
            context.Response.ContentType = domainConfig.Response.ContentType;
            if (domainConfig.Response.ContentValue != null)
            {
                await context.Response.WriteAsync(domainConfig.Response.ContentValue);
            }
        }
    }

    /// <summary>
    /// 递归匹配子域名配置
    /// </summary>
    /// <param name="url"></param>
    /// <param name="domainConfig"></param>
    /// <returns></returns>
    static IDomainConfig RecursionMatchDomainConfig(string url, IDomainConfig domainConfig)
    {
        if (domainConfig.Items.Any_Nullable())
        {
            var item = domainConfig.Items.FirstOrDefault(s => s.Key.IsMatch(url)).Value;
            if (item != null)
                return RecursionMatchDomainConfig(url, item);
        }
        return domainConfig;
    }

    bool TryGetDomainConfig(string uri, [MaybeNullWhen(false)] out IDomainConfig domainConfig)
    {
        domainConfig = null;

        if (!reverseProxyConfig.Service.OnlyEnableProxyScript && reverseProxyConfig.TryGetDomainConfig(uri, out domainConfig) == true)
        {
            return true;
        }

        var host = new Uri(uri).Host;
        // 未配置的域名，但仍然被解析到本机 IP 的域名
        if (IsDomain(host))
        {
            logger.LogWarning(
                $"域名 {host} 可能已经被 DNS 污染，如果域名为本机域名，请解析为非回环 IP。");
            domainConfig = defaultDomainConfig;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 是否为域名
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    static bool IsDomain(string host) => !IPAddress.TryParse(host, out _) && host.Contains('.');

    /// <summary>
    /// 获取目标前缀
    /// </summary>
    /// <param name="scheme"></param>
    /// <param name="host"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    string GetDestinationPrefix(string scheme, HostString host, Uri? destination)
    {
        var defaultValue = $"{scheme}://{host}/";
        if (destination == null)
        {
            return defaultValue;
        }

        var baseUri = new Uri(defaultValue);
        var result = new Uri(baseUri, destination).ToString();
        logger.LogInformation($"{defaultValue} => {result}");
        return result;
    }

    /// <summary>
    /// 处理错误信息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    static async Task HandleErrorAsync(HttpContext context, ForwarderError error)
    {
        await context.Response.WriteAsync($"{error}:{context.GetForwarderErrorFeature()?.Exception?.Message}");
    }

    /// <summary>
    /// 处理脚本注入内容
    /// </summary>
    /// <param name="context"></param>
    /// <param name="scripts"></param>
    /// <param name="body"></param>
    /// <param name="originalBody"></param>
    /// <returns></returns>
    async Task HandleScriptInject(HttpContext context, IEnumerable<IScriptConfig>? scripts, MemoryStream body, Stream originalBody)
    {
        async Task ResetBody()
        {
            body.Seek(0, SeekOrigin.Begin);
            context.Response.ContentLength = body.Length;
            await body.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }

        if (!scripts.Any_Nullable() ||
            context.Request.Method != HttpMethods.Get ||
            context.Response.StatusCode != StatusCodes.Status200OK ||
            !context.Response.ContentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
        {
            await ResetBody();
            return;
        }

        if (IReverseProxyService.Instance.IsOnlyWorkSteamBrowser && context.Request.UserAgent()?.Contains("Valve Steam") == false)
        {
            await ResetBody();
            return;
        }

        if (!string.IsNullOrEmpty(context.Response.Headers.ContentSecurityPolicy))
        {
            //var csp = context.Response.Headers.ContentSecurityPolicy.ToString();
            //var marks = new string[] { "default-src", "script-src", "connect-src" };

            //foreach (var mark in marks)
            //{
            //    var cspIndex = csp.IndexOf(mark);
            //    if (cspIndex >= 0)
            //    {
            //        context.Response.Headers.ContentSecurityPolicy = csp.Insert(cspIndex + mark.Length, " " + IReverseProxyService.LocalDomain);
            //    }
            //}
            context.Response.Headers.Remove("Content-Security-Policy");
        }

        var isSetBody = false;

        if (scripts.Any() && body.Length < int.MaxValue)
        {
            try
            {
                body.Seek(0, SeekOrigin.Begin);
                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Encoding
                var contentCompression = context.Response.Headers.ContentEncoding.ToString().ToLowerInvariant();
                using Stream bodyDecompress = GetStreamByContentCompression(body, contentCompression, CompressionMode.Decompress, true) ?? body;

                var buffer_ = await bodyDecompress.ToByteArrayAsync();

                // https://github.com/dotnet/runtime/blob/v6.0.6/src/libraries/System.Net.Http/src/System/Net/Http/HttpContent.cs#L175
                Encoding? encoding = context.Response.ContentTextEncoding();
                if (encoding == null)
                {
                    if (!TryDetectEncoding(buffer_, out encoding))
                    {
                        encoding = Encoding.UTF8;
                    }
                }

                if (FindScriptInjectInsertPosition(buffer_, encoding, out var buffer, out var position))
                {
                    using var bodyWriter = new MemoryStream();
                    using Stream? bodyCompress = GetStreamByContentCompression(bodyWriter, contentCompression, CompressionMode.Compress, true);

                    if (bodyCompress != null)
                    {
                        await WriteAsync(bodyCompress);
                        await bodyCompress.DisposeAsync(); // 不主动释放压缩流会有残余数据未写入
                    }
                    else
                    {
                        await WriteAsync(bodyWriter);
                        context.Response.Headers.Remove("Content-Encoding");
                    }

                    async Task WriteAsync(Stream bodyCoreWriter)
                    {
                        var html_start = buffer[..position];
                        //#if DEBUG
                        //                        var html_start_string = encoding.GetString(html_start.Span);
                        //#endif
                        await bodyCoreWriter.WriteAsync(html_start);
                        (var script_xml_start, var script_xml_end) = GetScriptXmls(encoding);
                        foreach (var script in scripts)
                        {
                            await bodyCoreWriter.WriteAsync(script_xml_start);
                            await bodyCoreWriter.WriteAsync(encoding.GetBytes(script.LocalId.ToString()));
                            await bodyCoreWriter.WriteAsync(script_xml_end);
                        }
                        var html_end = buffer[position..];
                        //#if DEBUG
                        //                        var html_end_string = encoding.GetString(html_end.Span);
                        //#endif
                        await bodyCoreWriter.WriteAsync(html_end);
                    }

                    isSetBody = true;
                    await SetBodyAsync(bodyWriter);
                }
            }
#if !DEBUG
            catch
#else
            catch (Exception e)
#endif
            {
#if DEBUG
                var rawUrl = context.Request.RawUrl();
                logger.LogError(e, "HandleScriptInject fail, rawUrl: {rawUrl}", rawUrl);
#endif
                await ResetBody();
            }
            finally
            {
                if (!isSetBody)
                {
                    await SetBodyAsync(body);
                }
                await body.DisposeAsync();
            }

            async Task SetBodyAsync(Stream stream)
            {
                context.Response.ContentLength = stream.Length;
                stream.Seek(0, SeekOrigin.Begin);
                await stream.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
            }
        }
    }

    static Stream? GetStreamByContentCompression(Stream stream, string contentCompression, CompressionMode mode, bool leaveOpen) => contentCompression switch
    {
        "gzip" => new GZipStream(stream, mode, leaveOpen),
        "deflate" => new DeflateStream(stream, mode, leaveOpen),
        "br" => new BrotliStream(stream, mode, leaveOpen),
        _ => null,
    };

    static bool TryDetectEncoding(byte[] data, [NotNullWhen(true)] out Encoding? encoding/*, out int preambleLength*/)
    {
        // https://github.com/dotnet/runtime/blob/v6.0.6/src/libraries/System.Net.Http/src/System/Net/Http/HttpContent.cs#L773

        const long offset = 0L;
        var dataLength = data.Length;

        if (dataLength >= 2)
        {
            int first2Bytes = data[offset + 0] << 8 | data[offset + 1];

            switch (first2Bytes)
            {
                case UTF8PreambleFirst2Bytes:
                    if (dataLength >= UTF8PreambleLength && data[offset + 2] == UTF8PreambleByte2)
                    {
                        encoding = Encoding.UTF8;
                        //preambleLength = UTF8PreambleLength;
                        return true;
                    }
                    break;

                case UTF32OrUnicodePreambleFirst2Bytes:
                    // UTF32 not supported on Phone
                    if (dataLength >= UTF32PreambleLength && data[offset + 2] == UTF32PreambleByte2 && data[offset + 3] == UTF32PreambleByte3)
                    {
                        encoding = Encoding.UTF32;
                        //preambleLength = UTF32PreambleLength;
                    }
                    else
                    {
                        encoding = Encoding.Unicode;
                        //preambleLength = UnicodePreambleLength;
                    }
                    return true;

                case BigEndianUnicodePreambleFirst2Bytes:
                    encoding = Encoding.BigEndianUnicode;
                    //preambleLength = BigEndianUnicodePreambleLength;
                    return true;
            }
        }

        encoding = null;
        //preambleLength = 0;
        return false;
    }

    const int UTF8PreambleLength = 3;
    const byte UTF8PreambleByte2 = 0xBF;
    const int UTF8PreambleFirst2Bytes = 0xEFBB;

    const int UTF32PreambleLength = 4;
    const byte UTF32PreambleByte2 = 0x00;
    const byte UTF32PreambleByte3 = 0x00;
    const int UTF32OrUnicodePreambleFirst2Bytes = 0xFFFE;

    //const int UnicodePreambleLength = 2;

    //const int BigEndianUnicodePreambleLength = 2;
    const int BigEndianUnicodePreambleFirst2Bytes = 0xFEFF;

    static readonly object marksLock = new();
    static readonly object scriptXmlsLock = new();
    static readonly Dictionary<int, (byte[] mark_start, byte[] mark_end)> marksDict = new();
    static readonly Dictionary<int, (byte[] script_xml_start, byte[] script_xml_end)> scriptXmlsDict = new();

    static (byte[] mark_start, byte[] mark_end) GetMarks(Encoding encoding)
    {
        var codePage = encoding.CodePage;
        if (marksDict.ContainsKey(codePage)) return marksDict[codePage];
        lock (marksLock)
        {
            var mark_start = encoding.GetBytes("</");
            var mark_end = encoding.GetBytes(">");
            var value = (mark_start, mark_end);
            marksDict.Add(codePage, value);
            return value;
        }
    }

    static (byte[] script_xml_start, byte[] script_xml_end) GetScriptXmls(Encoding encoding)
    {
        var codePage = encoding.CodePage;
        if (scriptXmlsDict.ContainsKey(codePage)) return scriptXmlsDict[codePage];
        lock (scriptXmlsLock)
        {
            const string scriptXmlStart = $"<script type=\"text/javascript\" src=\"https://{IReverseProxyService.LocalDomain}/";
            const string scriptXmlEnd = "\"></script>";
            var script_xml_start = encoding.GetBytes(scriptXmlStart);
            var script_xml_end = encoding.GetBytes(scriptXmlEnd);
            var value = (script_xml_start, script_xml_end);
            scriptXmlsDict.Add(codePage, value);
            return value;
        }
    }

    /// <summary>
    /// 查找脚本注入位置
    /// </summary>
    /// <param name="buffer_">Response.Body ByteArray</param>
    /// <param name="encoding">Response.Body Encoding</param>
    /// <param name="buffer">Response.Body Byte[]</param>
    /// <param name="insertPosition">Insert Script Xml Position</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    static bool FindScriptInjectInsertPosition(byte[] buffer_, Encoding encoding, out ReadOnlyMemory<byte> buffer, out int insertPosition)
    {
        buffer = buffer_.AsMemory();

        // 匹配 </...> 60 47 ... 62
        (var mark_start, var mark_end) = GetMarks(encoding);
        if (mark_start.Length <= 0 || mark_end.Length <= 0) goto notfound;

        int index_name_end = 0;
        int match_mark_end_index = 0;
        int match_mark_start_index = 0;

        for (int i = buffer_.Length - 1; i >= 0; i--) // 倒序匹配，对应之前的 LastIndexOf(string
        {
            var item = buffer_[i];
            if (index_name_end == 0)
            {
                var index = mark_end.Length - 1 - match_mark_end_index;
                if (index >= 0 && index < mark_end.Length && item == mark_end[index]) // 匹配末尾
                {
                    if (item == mark_end[index])
                    {
                        match_mark_end_index++;
                        if (match_mark_end_index >= mark_end.Length)
                        {
                            if (index_name_end == 0)
                            {
                                index_name_end = i;
                                continue;
                            }
                        }
                    }
                }
            }
            else
            {
                var index = mark_start.Length - 1 - match_mark_start_index;
                if (index >= 0 && index < mark_start.Length && item == mark_start[index]) // 匹配开头
                {
                    match_mark_start_index++;
                    if (match_mark_start_index >= mark_start.Length)
                    {
                        const int matchCharCount = 4;
                        var index_name_start = i + mark_start.Length;
                        //if (encoding.GetMaxCharCount(index_name_end - index_name_start) >= matchCharCount)
                        //{
                        var bytes = buffer.Span[index_name_start..index_name_end];
                        var charCount = encoding.GetCharCount(bytes);
                        if (charCount == matchCharCount)
                        {
                            var name = encoding.GetString(bytes);
                            if (name.Equals("body", StringComparison.OrdinalIgnoreCase) ||
                                name.Equals("head", StringComparison.OrdinalIgnoreCase))
                            {
                                insertPosition = index_name_start - mark_start.Length;
                                return true;
                            }
                        }
                        //}
                        goto reset;
                    }
                }
            }

            continue;

        reset: index_name_end = match_mark_end_index = match_mark_start_index = 0;
        }

    notfound: insertPosition = -1;
        return false;
    }
}
