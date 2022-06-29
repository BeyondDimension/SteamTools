// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub/Startup.cs

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace System.Application.Services.Implementation;

partial class YarpReverseProxyServiceImpl
{
    void StartupConfigureServices(IServiceCollection services)
    {
        services.AddConfiguration(this);
        services.AddDomainResolve();
        services.AddReverseProxyHttpClient();
        services.AddReverseProxyServer();
        services.AddFlowAnalyze();

#if WINDOWS
        if (ProxyMode == ProxyMode.DNSIntercept) 
        {
            services.AddPacketIntercept();
        }
#endif
    }

    void StartupConfigure(IApplicationBuilder app)
    {
        app.MapWhen(context => context.Connection.LocalPort == ProxyPort, appBuilder =>
        {
            appBuilder.UseHttpProxy();
        });

        app.MapWhen(context => context.Connection.LocalPort != ProxyPort, appBuilder =>
        {
            appBuilder.UseRequestLogging();
            appBuilder.UseHttpReverseProxy();

            appBuilder.UseRouting();
            appBuilder.DisableRequestLogging();
            appBuilder.UseEndpoints(endpoint =>
            {
                endpoint.MapGet("/", context =>
                {
                    Task return404()
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return context.Response.WriteAsync(string.Empty);
                    }

                    if (context.Request.Host.Host.Contains(IReverseProxyService.LocalDomain, StringComparison.OrdinalIgnoreCase))
                    {
                        async Task HttpRequest()
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
                            //    headrs.Add(new HttpHeader("Cookie", cookie));

                            if (context.Request.ContentType != null)
                                context.Response.Headers.ContentType = context.Request.ContentType;

                            switch (context.Request.Method.ToUpperInvariant())
                            {
                                case "GET":
                                    var body = await IHttpService.Instance.GetAsync<string>(url, cookie: cookie);
                                    if (string.IsNullOrEmpty(body))
                                    {
                                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                                        break;
                                    }
                                    await context.Response.WriteAsync(body);
                                    break;
                                case "POST":
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
                                                break;
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
                                    break;
                            }
                        }

                        if (context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.Headers.AccessControlAllowOrigin = context.Request.Headers.Origin.Count == 0 ? "*" : context.Request.Headers.Origin;
                            context.Response.Headers.AccessControlAllowHeaders = "*";
                            context.Response.Headers.AccessControlAllowMethods = "*";
                            context.Response.Headers.AccessControlAllowCredentials = "true";

                            return context.Response.WriteAsync(string.Empty);
                        }

                        var type = context.Request.Headers["requestType"];
                        switch (type)
                        {
                            case "xhr":
                                return HttpRequest();
                            default:
                                var content = Scripts?.FirstOrDefault(x => x.JsPathUrl == new Uri(context.Request.GetDisplayUrl()).LocalPath)?.Content;
                                if (string.IsNullOrEmpty(content))
                                {
                                    return return404();
                                }
                                context.Response.Headers.ContentType = "text/javascript;charset=UTF-8";
                                return context.Response.WriteAsync(content);
                        }
                    }

                    return return404();
                });
            });
            //appBuilder.UseEndpoints(endpoint =>
            //{
            //    endpoint.MapGet("/flowStatistics", context =>
            //    {
            //        var flowStatistics = context.RequestServices.GetRequiredService<IFlowAnalyzer>().GetFlowStatistics();
            //        return context.Response.WriteAsJsonAsync(flowStatistics);
            //    });
            //});
        });
    }
}
