using System.Net;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// Http 代理策略中间件
/// </summary>
sealed class HttpLocalRequestMiddleware
{
    readonly IReverseProxyConfig reverseProxyConfig;
    readonly CookieHttpClient cookieHttpClient;

    HttpClient HttpClient => cookieHttpClient.HttpClient;

    public HttpLocalRequestMiddleware(
        IReverseProxyConfig reverseProxyConfig,
        CookieHttpClient cookieHttpClient)
    {
        this.reverseProxyConfig = reverseProxyConfig;
        this.cookieHttpClient = cookieHttpClient;
    }

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (HttpMethods.IsOptions(context.Request.Method) && context.Request.Headers.ContainsKey("Access-Control-Request-Private-Network"))
        {
            // https://wicg.github.io/private-network-access/
            context.Response.Headers["Access-Control-Allow-Private-Network"] = "true";
        }

        if (context.Request.Host.Host.Equals(IReverseProxyService.Constants.LocalDomain, StringComparison.OrdinalIgnoreCase))
        {
            if (HttpMethods.IsOptions(context.Request.Method))
            {
                context.Response.Headers.AccessControlAllowOrigin = context.Request.Headers.Origin.Count == 0 ? "*" : context.Request.Headers.Origin;
                context.Response.Headers.AccessControlAllowHeaders = "*";
                context.Response.Headers.AccessControlAllowMethods = "*";
                context.Response.Headers.AccessControlAllowCredentials = "true";

                await context.Response.WriteAsync(string.Empty);
                return;
            }

            var type = context.Request.Headers["requestType"];
            switch (type)
            {
                case "status":
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync("OK");
                    return;
                case "xhr":
                    await HandleHttpRequestAsync(context);
                    return;
                default: //默认处理脚本匹配
                    if (!int.TryParse(context.Request.Path.Value?.Trim('/'), out var lid) && lid <= 0)
                    {
                        await Handle404NotFoundAsync(context);
                        return;
                    }
                    // TODO: Scripts
                    if (reverseProxyConfig.TryGetScriptContent(lid, out string? content))
                    {
                        if (string.IsNullOrEmpty(content))
                        {
                            await Handle404NotFoundAsync(context);
                            return;
                        }
                        context.Response.Headers.ContentType = "text/javascript;charset=UTF-8";
                        await context.Response.WriteAsync(content);
                        return;
                    }
                    else
                    {
                        await Handle404NotFoundAsync(context);
                        return;
                    }
            }
        }

        await next(context);
    }

    /// <summary>
    /// 处理脚本所需要的 HTTP 请求
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    async Task HandleHttpRequestAsync(HttpContext context)
    {
        try
        {
            var url = context.Request.QueryString.Value;
            if (string.IsNullOrEmpty(url))
                return;
            url = HttpUtility.UrlDecode(url.Replace("?request=", ""));
            Uri requestUri;
            try
            {
                requestUri = new Uri(url);
            }
            catch
            {
                return;
            }

            var method = context.Request.Method;
            var methodObj = method switch
            {
                "GET" => HttpMethod.Get,
                "PUT" => HttpMethod.Put,
                "POST" => HttpMethod.Post,
                "DELETE" => HttpMethod.Delete,
                "HEAD" => HttpMethod.Head,
                "OPTIONS" => HttpMethod.Options,
                "TRACE" => HttpMethod.Trace,
                "PATCH" => HttpMethod.Patch,
                "CONNECT" => HttpMethod.Connect,
                _ => TryParse(method),
            };
            if (methodObj == null)
                return;

            static HttpMethod? TryParse(string method)
            {
                try
                {
                    return new HttpMethod(method.ToUpperInvariant());
                }
                catch
                {

                }
                return null;
            }

            context.Response.Headers.AccessControlAllowOrigin = context.Request.Headers.Origin.Count == 0 ? "*" : context.Request.Headers.Origin;
            context.Response.Headers.AccessControlAllowHeaders = "*";
            context.Response.Headers.AccessControlAllowMethods = "*";
            context.Response.Headers.AccessControlAllowCredentials = "true";

            //if (cookie != null)
            //    context.Response.Cookies.Append(cookie);

            if (context.Request.ContentType != null)
                context.Response.Headers.ContentType = context.Request.ContentType;

            async Task SendAsync()
            {
                var method = context.Request.Method;
                var hasReqContent = !HttpMethods.IsGet(method) && context.Request.ContentLength > 0;
                var req = new HttpRequestMessage
                {
                    RequestUri = requestUri,
                    Method = methodObj,
                    Content = hasReqContent ? new StreamContent(context.Request.Body) : null,
                };
                foreach (var item in context.Request.Headers)
                {
                    var headers = item.Key.ToLower();
                    if (headers.EndsWith("-steamtool"))
                    {

                        if (headers.Equals("cookie", StringComparison.OrdinalIgnoreCase))
                        {
                            CookieHttpClient.CookieContainer.Add(new Cookie
                            {
                                CommentUri = requestUri,
                                Domain = requestUri.Host,
                                Value = item.Value
                            });
                        }
                        if (headers.Equals("referer", StringComparison.OrdinalIgnoreCase))
                        {
                            var refererUri = new Uri(item.Value.ToString());
                            req.Headers.Referrer = refererUri;
                        }
                        req.Headers.TryAddWithoutValidation(item.Key.TrimEnd("-steamtool"), (IEnumerable<string?>)item.Value);
                    }
                }
                req.Headers.UserAgent.ParseAdd(context.Request.Headers.UserAgent);
                if (hasReqContent)
                {
                    req.Content!.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType.ThrowIsNull());
                    req.Content.Headers.ContentLength = context.Request.ContentLength;
                }
                var rsp = await HttpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted).ConfigureAwait(false);
                if (rsp.IsSuccessStatusCode && rsp.Content != null)
                {
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    using var body = await rsp.Content.ReadAsStreamAsync(context.RequestAborted);
                    await body.CopyToAsync(context.Response.BodyWriter.AsStream(), context.RequestAborted);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    return;
                }
            }

            if (HttpMethods.IsGet(context.Request.Method))
            {
                await SendAsync();
                return;
            }
            else if (HttpMethods.IsPost(context.Request.Method))
            {
                if (context.Request.ContentLength > 0)
                {
                    await SendAsync();
                    return;
                }
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
        catch (Exception error)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync(error.Message, context.RequestAborted);
        }
    }

    static async Task HandleStatusCodeAsync(HttpContext context, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(string.Empty, context.RequestAborted);
    }

    /// <summary>
    /// 返回未匹配信息
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    static Task Handle404NotFoundAsync(HttpContext context)
        => HandleStatusCodeAsync(context, StatusCodes.Status404NotFound);
}