// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Http/HttpClientFactory.cs

// ReSharper disable once CheckNamespace
using System.Net;

namespace BD.WTTS.Services.Implementation;

sealed class ReverseProxyHttpClientFactory : IReverseProxyHttpClientFactory
{
    readonly IDomainResolver domainResolver;
    readonly IWebProxy webProxy;

    /// <summary>
    /// 首次生命周期
    /// </summary>
    readonly TimeSpan firstLiftTime = TimeSpan.FromSeconds(10d);

    /// <summary>
    /// 非首次生命周期
    /// </summary>
    readonly TimeSpan nextLifeTime = TimeSpan.FromSeconds(100d);

    /// <summary>
    /// <see cref="LifetimeHttpHandler"/> 清理器
    /// </summary>
    readonly LifetimeHttpHandlerCleaner httpHandlerCleaner = new();

    /// <summary>
    /// Lazy(LifetimeHttpHandler) 缓存
    /// </summary>
    readonly ConcurrentDictionary<LifeTimeKey, Lazy<LifetimeHttpHandler>> httpHandlerLazyCache = new();

    public ReverseProxyHttpClientFactory(IDomainResolver domainResolver, IReverseProxyConfig reverseProxyConfig)
    {
        this.domainResolver = domainResolver;
        if (reverseProxyConfig.Service.TwoLevelAgentEnable)
        {
            this.webProxy = new WebProxy($"{reverseProxyConfig.Service.TwoLevelAgentProxyType}://{reverseProxyConfig.Service.TwoLevelAgentIp}:{reverseProxyConfig.Service.TwoLevelAgentPortId}", true, null, null);
            if (!string.IsNullOrEmpty(reverseProxyConfig.Service.TwoLevelAgentUserName) || !string.IsNullOrEmpty(reverseProxyConfig.Service.TwoLevelAgentPassword))
            {
                webProxy.Credentials = new NetworkCredential(reverseProxyConfig.Service.TwoLevelAgentUserName, reverseProxyConfig.Service.TwoLevelAgentPassword);
            }
        }
        else
            this.webProxy = HttpNoProxy.Instance;
    }

    public ReverseProxyHttpClient CreateHttpClient(string domain, IDomainConfig domainConfig)
    {
        var lifeTimeKey = new LifeTimeKey(domain, domainConfig);
        var lifetimeHttpHandler = httpHandlerLazyCache.GetOrAdd(lifeTimeKey, CreateFirstLifetimeHttpHandlerLazy).Value;
        return new ReverseProxyHttpClient(lifetimeHttpHandler, disposeHandler: false);

        Lazy<LifetimeHttpHandler> CreateFirstLifetimeHttpHandlerLazy(LifeTimeKey lifeTimeKey)
            => new(() => CreateLifetimeHttpHandler(lifeTimeKey, firstLiftTime), true);
    }

    LifetimeHttpHandler CreateLifetimeHttpHandler(LifeTimeKey lifeTimeKey, TimeSpan lifeTime)
        => new(domainResolver, webProxy, lifeTimeKey, lifeTime, OnLifetimeHttpHandlerDeactivate);

    void OnLifetimeHttpHandlerDeactivate(LifetimeHttpHandler lifetimeHttpHandler)
    {
        var lifeTimeKey = lifetimeHttpHandler.LifeTimeKey;
        httpHandlerLazyCache[lifeTimeKey] = CreateNextLifetimeHttpHandlerLazy(lifeTimeKey);
        httpHandlerCleaner.Add(lifetimeHttpHandler);

        Lazy<LifetimeHttpHandler> CreateNextLifetimeHttpHandlerLazy(LifeTimeKey lifeTimeKey)
            => new(() => CreateLifetimeHttpHandler(lifeTimeKey, nextLifeTime), true);
    }
}