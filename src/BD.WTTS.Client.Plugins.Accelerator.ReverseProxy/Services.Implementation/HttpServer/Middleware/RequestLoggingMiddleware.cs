// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/RequestLoggingMilldeware.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// 请求日志中间件
/// </summary>
sealed class RequestLoggingMiddleware
{
    readonly ILogger<RequestLoggingMiddleware> logger;

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var feature = new RequestLoggingFeature();
        context.Features.Set<IRequestLoggingFeature>(feature);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
        }

        if (feature.Enable == false)
        {
            return;
        }

        var request = context.Request;
        var response = context.Response;
        var exception = context.GetForwarderErrorFeature()?.Exception;
        if (exception == null)
        {
            logger.LogInformation($"{request.Method} {request.Scheme}://{request.Host}{request.Path} responded {response.StatusCode} in {stopwatch.Elapsed.TotalMilliseconds} ms");
        }
        else if (IsError(exception))
        {
            logger.LogError($"{request.Method} {request.Scheme}://{request.Host}{request.Path} responded {response.StatusCode} in {stopwatch.Elapsed.TotalMilliseconds} ms{Environment.NewLine}{exception}");
        }
        else
        {
            logger.LogWarning($"{request.Method} {request.Scheme}://{request.Host}{request.Path} responded {response.StatusCode} in {stopwatch.Elapsed.TotalMilliseconds} ms{Environment.NewLine}{GetMessage(exception)}");
        }
    }

    /// <summary>
    /// 是否为错误
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    static bool IsError(Exception exception)
    {
        if (exception is OperationCanceledException)
        {
            return false;
        }

        if (HasInnerException<ConnectionAbortedException>(exception))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 是否有内部异常异常
    /// </summary>
    /// <typeparam name="TInnerException"></typeparam>
    /// <param name="exception"></param>
    /// <returns></returns>
    static bool HasInnerException<TInnerException>(Exception exception)
        where TInnerException : Exception
    {
        var inner = exception.InnerException;
        while (inner != null)
        {
            if (inner is TInnerException)
            {
                return true;
            }
            inner = inner.InnerException;
        }
        return false;
    }

    /// <summary>
    /// 获取异常信息
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    static string GetMessage(Exception exception)
    {
        var ex = exception;
        var builder = new StringBuilder();

        while (ex != null)
        {
            var type = ex.GetType();
            builder.Append(type.Namespace).Append('.').Append(type.Name).Append(": ").AppendLine(ex.Message);
            ex = ex.InnerException;
        }
        return builder.ToString();
    }

    sealed class RequestLoggingFeature : IRequestLoggingFeature
    {
        public bool Enable { get; set; } = true;
    }
}