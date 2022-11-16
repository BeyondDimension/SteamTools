using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;
using System.Application.Models;
using System.Security.Authentication;

namespace System.Application.Services.Implementation;

sealed partial class YarpReverseProxyServiceImpl : ReverseProxyServiceImpl, IReverseProxyService, IAsyncDisposable
{
    public override EReverseProxyEngine ReverseProxyEngine => EReverseProxyEngine.Yarp;

    static readonly string RootPath = Path.Combine(IOPath.AppDataDirectory, "Yarp");

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
            IOPath.DirCreateByNotExists(RootPath);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = RootPath,
                WebRootPath = RootPath,
            });

            builder.Host.UseNLog();
            StartupConfigureServices(builder.Services);
            builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(1d));
            builder.WebHost.UseKestrel(options =>
            {
                options.NoLimit();
                if (OperatingSystem.IsWindows())
                {
                    if (OperatingSystem2.IsWindows7())
                    {
                        //https://github.com/dotnet/aspnetcore/issues/22563
                        options.ConfigureHttpsDefaults(httpsOptions =>
                        {
                            httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                        });
                    }
                    options.ListenSshReverseProxy();
                    options.ListenGitReverseProxy();
                }

                if (ProxyMode is ProxyMode.System or ProxyMode.PAC or ProxyMode.VPN)
                {
                    options.ListenHttpProxy();
                }
                else
                {
                    options.ListenHttpsReverseProxy();
                    if (EnableHttpProxyToHttps)
                        options.ListenHttpReverseProxy();
                }
            });

            app = builder.Build();
            StartupConfigure(app);

            Task.Factory.StartNew(() => app.Run(), TaskCreationOptions.LongRunning);

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
        if (app == null) return;
        await app.DisposeAsync();
        app = null;
    }

    public FlowStatistics? GetFlowStatistics() => app?.Services.GetService<IFlowAnalyzer>()?.GetFlowStatistics();

    #region IDisposable

    protected override void DisposeCore()
    {
        (app as IDisposable)?.Dispose();
    }

    // https://docs.microsoft.com/zh-cn/dotnet/standard/garbage-collection/implementing-disposeasync#implement-both-dispose-and-async-dispose-patterns

    async ValueTask DisposeAsyncCore()
    {
        if (app is not null)
        {
            await app.DisposeAsync().ConfigureAwait(false);
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    #endregion
}