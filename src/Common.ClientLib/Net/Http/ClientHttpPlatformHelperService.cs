using System.Application;
using System.Threading.Tasks;

namespace System.Net.Http
{
    /// <inheritdoc cref="IHttpPlatformHelperService"/>
    public abstract class ClientHttpPlatformHelperService : HttpPlatformHelperService
    {
        public override Task<bool> IsConnectedAsync() => MainThread2.InvokeOnMainThreadAsync(() => IsConnected);
    }
}
