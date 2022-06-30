using System.Text;
using System.Application.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Extensions;

namespace System.Application.Services.Implementation.HttpServer.Middleware;

/// <summary>
/// Http 代理策略中间件
/// </summary>
sealed class HttpLocalRequestMiddleware
{
    readonly IReverseProxyConfig reverseProxyConfig;

    public HttpLocalRequestMiddleware(IReverseProxyConfig reverseProxyConfig)
    {
        this.reverseProxyConfig = reverseProxyConfig;
    }

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Host.Host.Equals(IReverseProxyService.LocalDomain, StringComparison.OrdinalIgnoreCase))
        {
            if (context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
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
        else
        {
            await next(context);
        }

    }

    /// <summary>
    /// 处理脚本所需要的 HTTP 请求
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    static async Task HandleHttpRequestAsync(HttpContext context)
    {
        if (string.IsNullOrEmpty(context.Request.QueryString.Value))
            return;

        var url = Web.HttpUtility.UrlDecode(context.Request.QueryString.Value.Replace("?request=", ""));
        var cookie = context.Request.Headers["cookie-steamTool"];
        if (string.IsNullOrEmpty(cookie))
            cookie = context.Request.Headers["Cookie"];

        context.Response.Headers.AccessControlAllowOrigin = context.Request.Headers.Origin.Count == 0 ? "*" : context.Request.Headers.Origin;
        context.Response.Headers.AccessControlAllowHeaders = "*";
        context.Response.Headers.AccessControlAllowMethods = "*";
        context.Response.Headers.AccessControlAllowCredentials = "true";

        //if (cookie != null)
        //    context.Response.Cookies.Append(cookie);

        if (context.Request.ContentType != null)
            context.Response.Headers.ContentType = context.Request.ContentType;

        if (context.Request.Method == HttpMethods.Get)
        {
            var body = await IHttpService.Instance.GetAsync<string>(url, cookie: cookie);
            if (string.IsNullOrEmpty(body))
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
            await context.Response.WriteAsync(body);
        }
        else if (context.Request.Method == HttpMethods.Post)
        {
            try
            {
                if (context.Request.ContentLength > 0)
                {
                    var conext = await IHttpService.Instance.SendAsync<string>(url, () =>
                    {
                        var req = new HttpRequestMessage
                        {
                            Method = HttpMethod.Post,
                            Content = new StreamContent(context.Request.Body),
                        };
                        req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);
                        req.Content.Headers.ContentLength = context.Request.Body.Length;
                        return req;
                    }, null/*, false*/, default);

                    if (string.IsNullOrEmpty(conext))
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        return;
                    }
                    await context.Response.WriteAsync(conext);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }
            }
            catch (Exception error)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync(error.Message);
            }
        }
    }

    /// <summary>
    /// 返回未匹配信息
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    static async Task Handle404NotFoundAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync(string.Empty);
    }
}
