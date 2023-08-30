using Fusillade;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Startup : IRequestCache
{
    protected virtual IRequestCache RequestCache => Ioc.Get<IRequestCacheRepository>();

    Task IRequestCache.Save(
        HttpRequestMessage request,
        HttpResponseMessage response,
        string key,
        CancellationToken cancellationToken)
            => RequestCache.Save(request, response, key, cancellationToken);

    Task<byte[]> IRequestCache.Fetch(
        HttpRequestMessage request,
        string key,
        CancellationToken cancellationToken)
           => RequestCache.Fetch(request, key, cancellationToken);
}
