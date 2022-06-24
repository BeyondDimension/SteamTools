#if WINDOWS

namespace System.Application.Services;

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