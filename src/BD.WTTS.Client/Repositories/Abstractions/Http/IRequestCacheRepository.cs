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
}
