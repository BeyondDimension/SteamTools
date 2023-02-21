using JsonSerializer = Newtonsoft.Json.JsonSerializer;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

internal sealed class SteamGridDBWebApiServiceImpl : GeneralHttpClientFactory, ISteamGridDBWebApiServiceImpl
{
    const string ApiKey = "ae93db7411cac53190aa5a9b633bf5e2";

    const string TAG = "SteamGridDBWebApiS";

    protected override string? DefaultClientName => TAG;

    readonly JsonSerializer jsonSerializer = new();

    public SteamGridDBWebApiServiceImpl(
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
        return client.SendAsync<T>(logger, jsonSerializer, requestUri,
            () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                request.Headers.Accept.ParseAdd(accept);
                var userAgent = http_helper.UserAgent;
                if (userAgent != null)
                    request.Headers.UserAgent.ParseAdd(userAgent);
                return request;
            }
            , accept, cancellationToken: cancellationToken);
    }

    public async Task<SteamGridApp?> GetSteamGridAppBySteamAppId(long appId)
    {
        var url = string.Format(SteamGridDBApiUrls.RetrieveGameBySteamAppId_Url, appId);
        var rsp = await GetAsync<SteamGridAppData>(url, MediaTypeNames.JSON, default);

        if (rsp != null)
        {
            if (rsp.Success == true)
            {
                return rsp.Data;
            }
            else
            {
                Log.Error(nameof(GetSteamGridAppBySteamAppId), string.Join(",", rsp.Errors));
            }
        }
        return null;
    }

    public async Task<List<SteamGridItem>?> GetSteamGridItemsByGameId(long gameId, SteamGridItemType type = SteamGridItemType.Grid)
    {
        var url = type switch
        {
            SteamGridItemType.Hero => string.Format(SteamGridDBApiUrls.RetrieveHeros_Url, gameId),
            SteamGridItemType.Logo => string.Format(SteamGridDBApiUrls.RetrieveLogos_Url, gameId),
            SteamGridItemType.Icon => string.Format(SteamGridDBApiUrls.RetrieveIcons_Url, gameId),
            SteamGridItemType.Header => string.Format(SteamGridDBApiUrls.RetrieveGrids_Url, gameId),
            _ => string.Format(SteamGridDBApiUrls.RetrieveGrids_Url, gameId),
        };

        //url += "?nsfw=any&humor=any";

        url += "?types=static,animated";

        if (type == SteamGridItemType.Header)
        {
            url += "&dimensions=460x215,920x430";
        }
        else if (type == SteamGridItemType.Grid)
        {
            url += "&dimensions=600x900";
        }

        var rsp = await GetAsync<SteamGridItemData>(url, MediaTypeNames.JSON, default);

        if (rsp != null)
        {
            if (rsp.Success == true)
            {
                foreach (var item in rsp.Data)
                {
                    item.GridType = type;
                }
                return rsp.Data;
            }
            else
            {
                Log.Error(nameof(GetSteamGridAppBySteamAppId), string.Join(",", rsp.Errors));
            }
        }
        return null;
    }
}