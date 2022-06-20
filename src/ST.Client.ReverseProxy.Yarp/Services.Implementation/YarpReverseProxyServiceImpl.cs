using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;
using Titanium.Web.Proxy.Network;

namespace System.Application.Services.Implementation;

sealed class YarpReverseProxyServiceImpl : ReverseProxyServiceImpl, IReverseProxyService
{
    WebApplication? app;

    public YarpReverseProxyServiceImpl(
        IPlatformService platformService,
        IDnsAnalysisService dnsAnalysis) : base(platformService, dnsAnalysis)
    {
        const bool userTrustRootCertificate = true;
        const bool machineTrustRootCertificate = false;
        const bool trustRootCertificateAsAdmin = false;
        CertificateManager = new(null, null, userTrustRootCertificate, machineTrustRootCertificate, trustRootCertificateAsAdmin, OnException);

        InitCertificateManager();
    }

    public override CertificateManager CertificateManager { get; }

    public override bool ProxyRunning => app != null;

    public Task<bool> StartProxy() => Task.FromResult(StartProxyCore());

    bool StartProxyCore()
    {
        try
        {
            var builder = WebApplication.CreateBuilder();

            builder.Host.UseNLog();
            StartupConfigureServices(builder.Services);
            builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(1d));
            builder.WebHost.UseKestrel(options =>
            {
                options.NoLimit();
                options.ListenHttpsReverseProxy();
                options.ListenHttpReverseProxy();

                if (OperatingSystem.IsWindows())
                {
                    options.ListenSshReverseProxy();
                    options.ListenGitReverseProxy();
                }
                else
                {
                    options.ListenHttpProxy();
                }
            });

            app = builder.Build();
            StartupConfigure(app);
            app.Run();

            return true;
        }
        catch (Exception ex)
        {
            OnException(ex);
            return false;
        }
    }

    public async void StopProxy() => await StopProxyCoreAsync();

    async Task StopProxyCoreAsync()
    {
        if (app == null) return;
        await app.StopAsync();
        await app.DisposeAsync();
        app = null;
    }

    protected override void DisposeCore()
    {
        StopProxy();
        CertificateManager.Dispose();
    }

    void StartupConfigureServices(IServiceCollection services)
    {
        services.AddConfiguration(this);
        services.AddDomainResolve();
        services.AddReverseProxyHttpClient();
        services.AddReverseProxyServer();
        services.AddFlowAnalyze();
        services.AddHostedService<AppHostedService>();

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