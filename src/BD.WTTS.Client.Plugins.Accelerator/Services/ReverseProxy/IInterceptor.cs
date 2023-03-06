#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 拦截器
/// </summary>
interface IInterceptor
{
    /// <summary>
    /// 拦截处理
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task InterceptAsync(CancellationToken cancellationToken);
}

#endif