using NetworkAccess = BD.Common.Enums.NetworkAccess;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

public class ClientHttpPlatformHelperServiceImpl : HttpPlatformHelperService
{
    protected const string ChromiumVersion = "110.0.0.0";
    protected const string AppleWebKitCompatVersion = "537.36";

    protected override bool IsConnected
    {
        get
        {
            if (CommonEssentials.IsSupported)
            {
                var networkAccess = Connectivity2.NetworkAccess();
                return networkAccess == NetworkAccess.Internet;
            }
            return true;
        }
    }

    //public override Task<bool> IsConnectedAsync() => MainThread2.InvokeOnMainThreadAsync(() => IsConnected);

    public override string AcceptLanguage => ResourceService.AcceptLanguage;

    public override string UserAgent => DefaultUserAgent;
}