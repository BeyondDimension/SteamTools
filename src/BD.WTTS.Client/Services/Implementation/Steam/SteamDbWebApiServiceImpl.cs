// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

internal sealed class SteamDbWebApiServiceImpl : GeneralHttpClientFactory, ISteamDbWebApiService
{
    const string TAG = "SteamDbWebApiS";

    protected override string? DefaultClientName => TAG;

    public SteamDbWebApiServiceImpl(
        IHttpClientFactory clientFactory,
        ILoggerFactory loggerFactory,
        IHttpPlatformHelperService http_helper) : base(
            loggerFactory.CreateLogger(TAG),
            http_helper, clientFactory)
    {

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task<T?> GetAsync<T>(string requestUri, string accept = MediaTypeNames.JSON, CancellationToken cancellationToken = default) where T : notnull
    {
        var client = CreateClient();
        return client.GetAsync<T>(logger, requestUri, accept,
            cancellationToken: cancellationToken, userAgent: http_helper.UserAgent);
    }

    public async Task<SteamUser> GetUserInfo(long steamId64)
    {
        var requestUri = string.Format(SteamApiUrls.STEAMDB_USERINFO_URL, steamId64);
        var rsp = await GetAsync<SteamUser>(requestUri);
        return rsp ?? new SteamUser() { SteamId64 = steamId64 };
    }

    public async Task<List<SteamUser>> GetUserInfo(IEnumerable<long> steamId64s)
    {
        var users = new List<SteamUser>();
        foreach (var i in steamId64s)
        {
            users.Add(await GetUserInfo(i));
        }
        return users;
    }

    public async Task<SteamApp> GetAppInfo(int appId)
    {
        var requestUri = string.Format(SteamApiUrls.STEAMDB_APPINFO_URL, appId);
        var rsp = await GetAsync<SteamApp>(requestUri);
        return rsp ?? new SteamApp() { AppId = (uint)appId };
    }
}