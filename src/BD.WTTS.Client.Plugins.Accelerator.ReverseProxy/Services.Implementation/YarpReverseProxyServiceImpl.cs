#if !DISABLE_ASPNET_CORE && (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using NLog.Web;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed partial class YarpReverseProxyServiceImpl : ReverseProxyServiceImpl, IReverseProxyService, IAsyncDisposable
{
    public override ReverseProxyEngine ReverseProxyEngine => ReverseProxyEngine.Yarp;

    static readonly string RootPath = Path.Combine(IOPath.AppDataDirectory, "Yarp");

    WebApplication? app;
    readonly IPCService ipc;

    public YarpReverseProxyServiceImpl(
        IPCService ipc,
        IDnsAnalysisService dnsAnalysis,
        ICertificateManager certificateManager) : base(dnsAnalysis)
    {
        this.ipc = ipc;
        CertificateManager = certificateManager;
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
#if WINDOWS
#if !NET7_0_OR_GREATER
                if (OperatingSystem2.IsWindows7())
                {
                    //https://github.com/dotnet/aspnetcore/issues/22563
                    options.ConfigureHttpsDefaults(httpsOptions =>
                    {
                        httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                    });
                }
#endif
                options.ListenSshReverseProxy();
                options.ListenGitReverseProxy();
#endif

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

            Task.Factory.StartNew(() =>
            {
                try
                {
                    app.Run();
                }
                catch (Exception ex)
                {
                    app = null;
                    ipc.Send(ReverseProxyCommand.OnExceptionTerminating, ex.Message);
                }
            }, TaskCreationOptions.LongRunning);

            return true;
        }
        catch (Exception ex)
        {
            OnException(ex);
            return false;
        }
    }

    public async ValueTask StopProxyAsync()
    {
        if (app == null) return;
        await app.StopAsync();
        if (app == null) return;
        await app.DisposeAsync();
        app = null;
    }

    public FlowStatistics? GetFlowStatistics() => app?.Services.GetService<IFlowAnalyzer>()?.GetFlowStatistics();

    // IDisposable

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
}
#endif