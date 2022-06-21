// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub/Startup.cs

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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

        if (OperatingSystem.IsWindows())
        {
            services.AddPacketIntercept();
        }
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
                endpoint.MapGet("/flowStatistics", context =>
                {
                    var flowStatistics = context.RequestServices.GetRequiredService<IFlowAnalyzer>().GetFlowStatistics();
                    return context.Response.WriteAsJsonAsync(flowStatistics);
                });
            });
        });
    }
}
