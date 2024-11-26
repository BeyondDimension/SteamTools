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
        DnsAnalysisServiceImpl dnsAnalysisServiceImpl,
        DnsDohAnalysisService dnsDohAnalysisService,
        ICertificateManager certificateManager) : base(dnsAnalysisServiceImpl, dnsDohAnalysisService)
    {
        this.ipc = ipc;
        platformService = GetIPCService<IPCPlatformService>(nameof(platformService));
        toast = GetIPCService<IPCToastService>(nameof(toast));
        CertificateManager = (CertificateManagerImpl)certificateManager;
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

    public override CertificateManagerImpl CertificateManager { get; }

    public override bool ProxyRunning => app != null;

    protected override void CheckRootCertificate()
    {
        if (OperatingSystem.IsWindows())
        {
            ICertificateManager.Constants.CheckRootCertificate(
                platformService,
                CertificateManager);

            X509Certificate2? cer = CertificateManager.RootCertificatePackable;
            if (cer is not null && DateTime.Now <= cer.NotAfter && cer.NotAfter <= DateTime.Now.AddMonths(1))
            {
                var interval = cer.NotAfter - DateTime.Now;

                _certificateTimer = new System.Timers.Timer(interval)
                {
                    AutoReset = false,
                };

                _certificateTimer.Elapsed += async (_, _) =>
                {
                    ICertificateManager.Constants.CheckRootCertificate(
                        platformService,
                        CertificateManager);

                    await StopProxyAsync();
                    await StartProxyImpl();
                };
                _certificateTimer.Start();
            }
        }
    }

    private System.Timers.Timer? _certificateTimer;

    private void StopCertificateTimer()
    {
        _certificateTimer?.Stop();
        _certificateTimer?.Dispose();
        _certificateTimer = null;
    }

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

            builder.Logging.AddProvider(new LogConsoleService.Utf8StringLoggerProvider(AssemblyInfo.Accelerator));

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
                options.RequestHeaderEncodingSelector = _ => Encoding.UTF8;
                options.ResponseHeaderEncodingSelector = _ => Encoding.UTF8;
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

    public void Exit()
    {
        ipc.Dispose();
    }

    public async Task StopProxyAsync()
    {
        StopCertificateTimer();
        Scripts = null;
        if (app == null) return;
        await app.StopAsync();
        if (app == null) return;
        await app.DisposeAsync();
        app = null;
    }

    public byte[]? GetFlowStatistics_Bytes()
    {
        var flowAnalyzer = app?.Services.GetService<IFlowAnalyzer>();
        var flowStatistics = flowAnalyzer?.GetFlowStatistics();
        var bytes = Serializable.SMP2(flowStatistics);
        return bytes;
    }

    public string? GetLogAllMessage()
    {
        var result = LogConsoleService.Builder;
        return result.ToString();
    }

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