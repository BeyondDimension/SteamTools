#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub/Startup.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

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
        app.UseHttpLocalRequest();

        app.UseHttpProxyPac();
        app.UseRequestLogging();
        app.UseHttpReverseProxy();

        //app.UseRouting();
        app.DisableRequestLogging();

        //app.UseEndpoints(endpoint =>
        //{
        //    endpoint.MapGet("/flowStatistics", context =>
        //    {
        //        var flowStatistics = context.RequestServices.GetRequiredService<IFlowAnalyzer>().GetFlowStatistics();
        //        return context.Response.WriteAsJsonAsync(flowStatistics);
        //    });
        //});
    }
}
#endif