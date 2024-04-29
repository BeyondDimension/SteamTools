// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public interface IProxyService : IDisposable, IAsyncDisposable
{
    bool ProxyStatus { get; set; }

    Task StartOrStopProxyService(bool startOrStop);
}