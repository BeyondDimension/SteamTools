// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

[Mobius(
"""
Mobius.Services.Abstractions.IProxyService
""")]
public interface IProxyService : IDisposable, IAsyncDisposable
{
    bool ProxyStatus { get; set; }

    [Mobius(
"""
StartAsync OR StopAsync
""", Obsolete = true)]
    Task StartOrStopProxyService(bool startOrStop);
}