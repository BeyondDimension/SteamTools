using System.Application;

namespace System.Net.Http;

/// <inheritdoc cref="IHttpPlatformHelperService"/>
public abstract class ClientHttpPlatformHelperService : HttpPlatformHelperService
{
    protected override bool IsConnected
    {
        get
        {
            if (Essentials.IsSupported)
            {
                var networkAccess = Connectivity2.NetworkAccess();
                return networkAccess == NetworkAccess.Internet;
            }
            return true;
        }
    }

    //public override Task<bool> IsConnectedAsync() => MainThread2.InvokeOnMainThreadAsync(() => IsConnected);
}
