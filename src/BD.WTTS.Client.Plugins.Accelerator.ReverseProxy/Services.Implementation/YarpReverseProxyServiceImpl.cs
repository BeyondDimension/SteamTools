using Microsoft.AspNetCore.HostFiltering;
using NLog.Web;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed partial class YarpReverseProxyServiceImpl : ReverseProxyServiceImpl, IReverseProxyService, IReverseProxySettings, IAsyncDisposable
{
    static readonly string RootPath = Path.Combine(IOPath.AppDataDirectory, "Yarp");

    WebApplication? app;
    readonly IPCSubProcessService ipc;
    readonly IPCToastService toast;
    readonly IPCPlatformService platformService;

    public YarpReverseProxyServiceImpl(
        IPCSubProcessService ipc,
        IDnsAnalysisService dnsAnalysis,
        ICertificateManager certificateManager) : base(dnsAnalysis)
    {
        this.ipc = ipc;
        platformService = GetIPCService<IPCPlatformService>(nameof(platformService));
        toast = GetIPCService<IPCToastService>(nameof(toast));
        CertificateManager = certificateManager;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    TIPCService GetIPCService<TIPCService>(string paramName) where TIPCService : class
    {
        var ipcService = ipc.GetService<TIPCService>() ?? throw new ArgumentNullException(paramName);
        ipcAddSingletonServices += s =>
        {
            s.AddSingleton(ipcService);
        };
        return ipcService;
    }

    public override ICertificateManager CertificateManager { get; }

    public override bool ProxyRunning => app != null;

    protected override Task<StartProxyResult> StartProxyImpl() => Task.FromResult(StartProxyCore());

    StartProxyResult StartProxyCore()
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

            builder.Services.Configure<HostFilteringOptions>(static o =>
            {
                o.AllowEmptyHosts = true;
                o.AllowedHosts = new List<string>
                {
                    "*",
                };
            });

            builder.Host.UseNLog();
            StartupConfigureServices(builder.Services);
            builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(1d));
            builder.WebHost.UseKestrel(options =>
            {
                options.AddServerHeader = false;
                options.NoLimit();
#if !NOT_WINDOWS
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
                //options.ListenSshReverseProxy();
                //options.ListenGitReverseProxy();
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
            app.UseHostFiltering();
            StartupConfigure(app);

            Exception? exception = null;
            const int timeout_ms = 650;
            var waitTask = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(timeout_ms);
            });
            var appTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    app.Run();
                }
                catch (Exception ex)
                {
                    app = null;
                    exception = ex;
                    OnException(ex);
                    //toast.ShowAppend(IPCToastService.ToastText.CommunityFix_OnRunCatch, Environment.NewLine + "ExceptionMessage: " + ex.Message);
                }
            }, TaskCreationOptions.LongRunning);
            Task.WaitAny(waitTask, appTask);
            return exception;
        }
        catch (Exception ex)
        {
            OnException(ex);
            return ex;
        }
    }

    public async Task StopProxyAsync()
    {
        Scripts = null;
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