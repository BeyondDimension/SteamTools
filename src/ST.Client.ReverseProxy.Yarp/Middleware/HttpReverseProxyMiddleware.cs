// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/HttpReverseProxyMiddleware.cs

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Application.Models;
using System.Application.Services;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Yarp.ReverseProxy.Forwarder;

namespace System.Application.Middleware;

/// <summary>
/// 反向代理中间件
/// </summary>
sealed class HttpReverseProxyMiddleware
{
    static readonly AccelerateProjectDTO defaultDomainConfig = new() { IgnoreServerName = true, };

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
        var host = context.Request.Host;
        if (TryGetDomainConfig(host, out var domainConfig) == false)
        {
            await next(context);
        }
        else if (domainConfig.Response == null)
        {
            var scheme = context.Request.Scheme;
            var destinationPrefix = GetDestinationPrefix(scheme, host, domainConfig.Destination);
            var httpClient = httpClientFactory.CreateHttpClient(host.Host, domainConfig);
            var error = await httpForwarder.SendAsync(context, destinationPrefix, httpClient);
            await HandleErrorAsync(context, error);
        }
        else
        {
            context.Response.StatusCode = domainConfig.Response.StatusCode;
            context.Response.ContentType = domainConfig.Response.ContentType;
            if (domainConfig.Response.ContentValue != null)
            {
                await context.Response.WriteAsync(domainConfig.Response.ContentValue);
            }
        }
    }

    bool TryGetDomainConfig(HostString host, [MaybeNullWhen(false)] out AccelerateProjectDTO domainConfig)
    {
        if (reverseProxyConfig.TryGetDomainConfig(host.Host, out domainConfig) == true)
        {
            return true;
        }

        // 未配置的域名，但仍然被解析到本机 IP 的域名
        if (OperatingSystem.IsWindows() && IsDomain(host.Host))
        {
            logger.LogWarning(
                $"域名 {host.Host} 可能已经被 DNS 污染，如果域名为本机域名，请解析为非回环 IP。");
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
        if (error == ForwarderError.None || context.Response.HasStarted)
        {
            return;
        }

        await context.Response.WriteAsJsonAsync(new
        {
            error = error.ToString(),
            message = context.GetForwarderErrorFeature()?.Exception?.Message,
        });
    }
}
