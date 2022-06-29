// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/ApplicationBuilderExtensions.cs

using Microsoft.Extensions.DependencyInjection;
using System.Application.Services.Implementation.HttpServer.Middleware;
using System.Application.Services;

namespace Microsoft.AspNetCore.Builder;

static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 使用 Http 代理策略中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    internal static IApplicationBuilder UseHttpProxyPac(this IApplicationBuilder app)
    {
        var middleware = app.ApplicationServices.GetRequiredService<HttpProxyPacMiddleware>();
        return app.Use(next => context => middleware.InvokeAsync(context, next));
    }

    /// <summary>
    /// 使用请求日志中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    internal static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        var middleware = app.ApplicationServices.GetRequiredService<RequestLoggingMiddleware>();
        return app.Use(next => context => middleware.InvokeAsync(context, next));
    }

    /// <summary>
    /// 禁用请求日志中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    internal static IApplicationBuilder DisableRequestLogging(this IApplicationBuilder app)
    {
        return app.Use(next => context =>
        {
            var loggingFeature = context.Features.Get<IRequestLoggingFeature>();
            if (loggingFeature != null)
            {
                loggingFeature.Enable = false;
            }
            return next(context);
        });
    }

    /// <summary>
    /// 使用反向代理中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    internal static IApplicationBuilder UseHttpReverseProxy(this IApplicationBuilder app)
    {
        var middleware = app.ApplicationServices.GetRequiredService<HttpReverseProxyMiddleware>();
        return app.Use(next => context => middleware.InvokeAsync(context, next));
    }
}
