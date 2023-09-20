namespace BD.WTTS.Services.Implementation;

partial class ArchiSteamFarmWebApiServiceImpl // SendAsync(HttpClient)
    : GeneralHttpClientFactory
{
    [Obsolete("TODO: 更改为设置项的默认值")]
    const string DefaultBaseAddress = "http://localhost:1242";

    [Obsolete("TODO: 更改为从设置项中读取")]
    string BaseAddress => "";

    HttpClient CreateClient()
    {
        string baseAddressStr = BaseAddress;
        Uri baseAddress;
        try
        {
            baseAddress = new Uri(baseAddressStr);
        }
        catch
        {
            baseAddressStr = DefaultBaseAddress;
            baseAddress = new Uri(DefaultBaseAddress);
        }

        var client = CreateClient($"{TAG}_{baseAddressStr}", HttpHandlerCategory.Default);
        client.BaseAddress = baseAddress;
        return client;
    }

    async Task<IApiRsp<TResponseBody?>> SendAsync<TRequestBody, TResponseBody>(
        string requestUrl,
        HttpMethod method,
        TRequestBody? reqBody,
        bool responseContentMaybeNull = false,
        bool isShowResponseErrorMessage = true,
        string? errorAppendText = null,
        CancellationToken cancellationToken = default)
        where TRequestBody : notnull
        where TResponseBody : notnull
    {
        var result = await SendCoreAsync();
        if (isShowResponseErrorMessage)
        {
            conn_helper.ShowResponseErrorMessage(requestUrl, result, errorAppendText);
        }
        return result;

        async Task<IApiRsp<TResponseBody?>> SendCoreAsync()
        {
            var client = CreateClient();
            using var req = new HttpRequestMessage(method, requestUrl);
            var typeVoid = typeof(Void);
            var typeRequestBody = typeof(TRequestBody);
            var isVoidRequestBody = typeRequestBody == typeVoid;
            if (!isVoidRequestBody)
            {
                try
                {
                    var reqContent = Serialize(reqBody, typeRequestBody);
#if DEBUG
                    var reqContentJsonStr = await reqContent.ReadAsStringAsync(cancellationToken);
#endif
                    req.Content = reqContent;
                }
                catch (Exception ex)
                {
                    return ApiRspHelper.Code<TResponseBody?>(
                        ApiRspCode.ClientException,
                        "Serialize request fail",
                        default,
                        ex);
                }
            }

            using var rsp = await client.SendAsync(req, cancellationToken);
            try
            {
                var result = await DeserializeAsync<TResponseBody?>(rsp.Content, cancellationToken);
                if (!responseContentMaybeNull && result == null)
                {
                    return ApiRspHelper.Code<TResponseBody?>(ApiRspCode.NoResponseContent);
                }
                return ApiRspHelper.Code<TResponseBody?>(
                    (ApiRspCode)rsp.StatusCode, null, result);
            }
            catch (Exception ex)
            {
                return ApiRspHelper.Code<TResponseBody?>(
                    ApiRspCode.ClientException,
                    "Serialize request fail",
                    default,
                    ex);
            }
        }
    }

    struct Void
    {
        // https://github.com/dotnet/runtime/blob/v7.0.11/src/libraries/System.Private.CoreLib/src/System/Void.cs
    }
}
