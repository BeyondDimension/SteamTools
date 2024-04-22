namespace Mobius;

partial class XunYouSDK // 3. 查询会员时间接口
{
#if WINDOWS
    static readonly Lazy<HttpClient> httpClient = new(() =>
    {
        var httpClient = Ioc.Get_Nullable<IHttpClientFactory>()
            ?.CreateClient(nameof(XunYouSDK)) ?? new HttpClient();
        httpClient.BaseAddress = new Uri(webapi_host);
        return httpClient;
    }, LazyThreadSafetyMode.ExecutionAndPublication);
#endif

    public static async Task<XunYouVipEndTimeResponse?> GetVipEndTime(string userId)
    {
#if WINDOWS
        if (string.IsNullOrWhiteSpace(webapi_vip_endtime))
            return null;

        XunYouVipEndTimeRequest req = new()
        {
            UserId = userId,
        };
        req.Sign = CalcWebApiSign(req);

        using var response = await httpClient.Value.PostAsJsonAsync(webapi_vip_endtime, req, SystemTextJsonSerializerContext_XunYouSDK.Default.XunYouVipEndTimeRequest);
        var rsp = await response.Content.ReadFromJsonAsync(SystemTextJsonSerializerContext_XunYouSDK.Default.XunYouVipEndTimeResponse);
        return rsp;
#else
        await Task.CompletedTask;
        return null;
#endif
    }
}