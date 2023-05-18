using Fusillade;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories;

internal sealed class RequestCacheRepository : Repository<RequestCache, string>, IRequestCacheRepository
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string GetRequestUri(HttpRequestMessage request)
        => request.RequestUri?.ToString() ?? "/";

    async Task IRequestCache.Save(
        HttpRequestMessage request,
        HttpResponseMessage response,
        string key,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            return; // 仅缓存成功的状态码

        if (request.Headers.TryGetValues(ApiConstants.Headers.Request.SecurityKey, out var _) ||
            request.Headers.TryGetValues(ApiConstants.Headers.Request.SecurityKeyHex, out var _))
        {
            // 加密的数据不进行缓存
            return;
        }

        var rspContent = response.Content;
        if (rspContent == null)
            return;

        var bytes = await rspContent.ReadAsByteArrayAsync();
        if (!bytes.Any_Nullable())
            return;

        var entity = new RequestCache
        {
            Id = key,
            HttpMethod = request.Method.Method,
            RequestUri = GetRequestUri(request),
            Response = bytes,
        };

        await InsertOrUpdateAsync(entity, cancellationToken);
    }

    async Task<byte[]> IRequestCache.Fetch(
        HttpRequestMessage request,
        string key,
        CancellationToken cancellationToken)
    {
        var requestUri = GetRequestUri(request);
        var entity = await FirstOrDefaultAsync(x => x.Id == key &&
            x.HttpMethod == request.Method.Method &&
            x.RequestUri == requestUri,
            cancellationToken);

        if (entity != null)
            return entity.Response;

        return Array.Empty<byte>();
    }
}
