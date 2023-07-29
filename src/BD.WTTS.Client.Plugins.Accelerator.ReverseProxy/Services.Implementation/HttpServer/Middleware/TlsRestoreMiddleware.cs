// https://github.com/dotnetcore/FastGithub/blob/58f79ddc19410c92b18e8d4de1c4b61376e97be7/FastGithub.HttpServer/TlsMiddlewares/TlsRestoreMiddleware.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// Https 恢复中间件
/// </summary>
static class TlsRestoreMiddleware
{
    /// <summary>
    /// 执行中间件
    /// </summary>
    /// <param name="next"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task InvokeAsync(ConnectionDelegate next, ConnectionContext context)
    {
        if (context.Features.Get<ITlsConnectionFeature>() == FakeTlsConnectionFeature.Instance)
        {
            // 擦除入侵
            context.Features.Set<ITlsConnectionFeature>(null);
        }
        await next(context);
    }
}