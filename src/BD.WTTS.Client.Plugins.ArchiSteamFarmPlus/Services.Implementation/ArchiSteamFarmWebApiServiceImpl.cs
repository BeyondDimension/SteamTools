namespace BD.WTTS.Services.Implementation;

sealed partial class ArchiSteamFarmWebApiServiceImpl // Constructor
    : IArchiSteamFarmWebApiService
{
    const string TAG = "ASFWebApiS";

    readonly IApiConnectionPlatformHelper conn_helper;

    public ArchiSteamFarmWebApiServiceImpl(
            ILoggerFactory loggerFactory,
            IHttpPlatformHelperService http_helper,
            IHttpClientFactory clientFactory,
            IApiConnectionPlatformHelper conn_helper)
            : base(loggerFactory.CreateLogger(TAG), http_helper, clientFactory)
    {
        this.conn_helper = conn_helper;
    }
}
