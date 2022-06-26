using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;
using System.Application.Models;

namespace System.Application.Services.Implementation;

sealed partial class YarpReverseProxyServiceImpl : ReverseProxyServiceImpl, IReverseProxyService
{
    public override EReverseProxyEngine ReverseProxyEngine => EReverseProxyEngine.Yarp;

    WebApplication? app;

    public YarpReverseProxyServiceImpl(
        IPlatformService platformService,
        IDnsAnalysisService dnsAnalysis) : base(platformService, dnsAnalysis)
    {
        CertificateManager = new YarpCertificateManagerImpl(platformService, this);
    }

    public override ICertificateManager CertificateManager { get; }

    public override bool ProxyRunning => app != null;

    protected override Task<bool> StartProxyImpl() => Task.FromResult(StartProxyCore());

    bool StartProxyCore()
    {
        // https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub/Program.cs#L29
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

            Task.Factory.StartNew(() => app.Run());

            return true;
        }
        catch (Exception ex)
        {
            OnException(ex);
            return false;
        }
    }

    public async Task StopProxy()
    {
        if (app == null) return;
        await app.StopAsync();
        await app.DisposeAsync();
        app = null;
    }

    public FlowStatistics? GetFlowStatistics()
    {
        if (app == null) return null;

        return app.Services.GetRequiredService<IFlowAnalyzer>().GetFlowStatistics();
    }

    protected override void DisposeCore()
    {
        StopProxy();
    }
}