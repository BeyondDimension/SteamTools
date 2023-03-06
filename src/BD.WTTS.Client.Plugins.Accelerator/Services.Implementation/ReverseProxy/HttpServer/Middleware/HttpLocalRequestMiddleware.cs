#if !DISABLE_ASPNET_CORE && (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
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

        if (context.Request.Host.Host.Equals(IReverseProxyService.LocalDomain, StringComparison.OrdinalIgnoreCase))
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
                    var content = reverseProxyConfig.Service.Scripts?.FirstOrDefault(x => x.LocalId == lid)?.Content;
                    if (string.IsNullOrEmpty(content))
                    {
                        await Handle404NotFoundAsync(context);
                        return;
                    }
                    context.Response.Headers.ContentType = "text/javascript;charset=UTF-8";
                    await context.Response.WriteAsync(content);
                    return;
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
            if (string.IsNullOrEmpty(context.Request.QueryString.Value))
                return;

            var url = HttpUtility.UrlDecode(context.Request.QueryString.Value.Replace("?request=", ""));
            string? cookie = context.Request.Headers["cookie-steamTool"];
            if (string.IsNullOrEmpty(cookie))
                cookie = context.Request.Headers["Cookie"];
            string? referer = context.Request.Headers["Referer-steamTool"];

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
                var hasContent = method != HttpMethods.Get && context.Request.ContentLength > 0;
                var req = new HttpRequestMessage
                {
                    RequestUri = new Uri(url),
                    Method = new(method),
                    Content = hasContent ? new StreamContent(context.Request.Body) : null,
                };
                req.Headers.UserAgent.ParseAdd(context.Request.Headers.UserAgent);
                if (cookie != null)
                {
                    CookieHttpClient.CookieContainer.Add(new Cookie
                    {
                        CommentUri = req.RequestUri,
                        Domain = req.RequestUri.Host,
                        Value = cookie,
                    });
                    // req.Headers.Add("Cookie", cookie);
                }
                if (referer != null) req.Headers.Referrer = new Uri(referer);
                if (hasContent)
                {
                    req.Content!.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);
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

            if (context.Request.Method == HttpMethods.Get)
            {
                await SendAsync();
                return;
            }
            else if (context.Request.Method == HttpMethods.Post)
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
#endif