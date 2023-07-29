// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Http/RequestContextExtensions.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

static class RequestContextExtensions
{
    static readonly HttpRequestOptionsKey<RequestContext> key = new(nameof(RequestContext));

    /// <summary>
    /// 设置 <see cref="RequestContext"/>
    /// </summary>
    /// <param name="httpRequestMessage"></param>
    /// <param name="requestContext"></param>
    public static void SetRequestContext(this HttpRequestMessage httpRequestMessage, RequestContext requestContext)
    {
        httpRequestMessage.Options.Set(key, requestContext);
    }

    /// <summary>
    /// 获取 <see cref="RequestContext"/>
    /// </summary>
    /// <param name="httpRequestMessage"></param>
    /// <returns></returns>
    public static RequestContext GetRequestContext(this HttpRequestMessage httpRequestMessage)
    {
        return httpRequestMessage.Options.TryGetValue(key, out var requestContext)
            ? requestContext
            : throw new InvalidOperationException($"Please call first {nameof(SetRequestContext)}.");
    }
}