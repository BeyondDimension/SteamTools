using Fusillade;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Startup : IRequestCache
{
    protected virtual IRequestCache RequestCache => Ioc.Get<IRequestCacheRepository>();

    Task IRequestCache.SaveAsync(
        HttpRequestMessage request,
        HttpResponseMessage response,
        string key,
        CancellationToken cancellationToken)
            => RequestCache.SaveAsync(request, response, key, cancellationToken);

    Task<byte[]?> IRequestCache.FetchAsync(
        HttpRequestMessage request,
        string key,
        CancellationToken cancellationToken)
           => RequestCache.FetchAsync(request, key, cancellationToken);
}
