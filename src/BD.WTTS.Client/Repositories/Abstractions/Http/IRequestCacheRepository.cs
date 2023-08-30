using Fusillade;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories.Abstractions;

public interface IRequestCacheRepository : IRequestCache
{
    /// <summary>
    /// 根据主键更新使用时间
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateUsageTimeByIdAsync(string id, CancellationToken cancellationToken);

    /// <summary>
    /// 删除所有缓存数据
    /// </summary>
    /// <returns></returns>
    Task<int> DeleteAllAsync();

    /// <summary>
    /// 根据时间删除在此时间之前的缓存数据
    /// </summary>
    /// <returns></returns>
    Task<int> DeleteAllAsync(DateTimeOffset dateTimeOffset);

    const string DefaultRequestUri = "/";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string GetOriginalRequestUri(HttpRequestMessage request)
    {
        string? originalRequestUri;
        if (request is ImageHttpClientService.ImageHttpRequestMessage request2)
        {
            // 对于一些重定向的 Url 使用原始 Url 进行唯一性的计算
            originalRequestUri = request2.OriginalRequestUri;
        }
        else
        {
            originalRequestUri = request.RequestUri?.ToString()!;
        }
        if (string.IsNullOrEmpty(originalRequestUri))
            originalRequestUri = DefaultRequestUri;
        return originalRequestUri;
    }
}
