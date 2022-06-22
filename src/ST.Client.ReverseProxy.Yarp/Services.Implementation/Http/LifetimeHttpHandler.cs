// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Http/LifetimeHttpHandler.cs

namespace System.Application.Services.Implementation.Http;

/// <summary>
/// 具有生命周期的 <see cref="HttpMessageHandler"/>
/// </summary>
sealed class LifetimeHttpHandler : DelegatingHandler
{
    readonly Timer timer;

    public LifeTimeKey LifeTimeKey { get; }

    public LifetimeHttpHandler(IDomainResolver domainResolver, LifeTimeKey lifeTimeKey, TimeSpan lifeTime, Action<LifetimeHttpHandler> deactivateAction)
    {
        LifeTimeKey = lifeTimeKey;
        InnerHandler = new ReverseProxyHttpClientHandler(lifeTimeKey.DomainConfig, domainResolver);
        timer = new Timer(OnTimerCallback, deactivateAction, lifeTime, Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// <see cref="timer"/> 触发时
    /// </summary>
    /// <param name="state"></param>
    void OnTimerCallback(object? state)
    {
        timer.Dispose();
        ((Action<LifetimeHttpHandler>)state!)(this);
    }

    protected override void Dispose(bool disposing)
    {
        // 这里不释放资源
    }
}