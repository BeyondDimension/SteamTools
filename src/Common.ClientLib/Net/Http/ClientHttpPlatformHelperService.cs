using System.Application;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace System.Net.Http
{
    /// <inheritdoc cref="IHttpPlatformHelperService"/>
    public abstract class ClientHttpPlatformHelperService : HttpPlatformHelperService
    {
        protected override bool IsConnected
        {
            get
            {
                if (Essentials.IsSupported)
                {
                    var networkAccess = Connectivity.NetworkAccess;
                    return networkAccess == NetworkAccess.Internet;
                }
                return true;
            }
        }

        public override Task<bool> IsConnectedAsync() => MainThread2.InvokeOnMainThreadAsync(() => IsConnected);
    }
}
