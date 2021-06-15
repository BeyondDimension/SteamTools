using System.Runtime.Versioning;
using Windows.Networking.Connectivity;
using Xamarin.Essentials;

namespace System.Application.Services.Implementation
{
    internal sealed class PlatformHttpPlatformHelper : DesktopHttpPlatformHelper
    {
        static readonly Lazy<string> mUserAgent = new(() =>
        {
            var value = $"Mozilla/5.0 (Windows NT {Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}{(Environment.Is64BitOperatingSystem ? "; WOW64" : null)}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.72 Safari/537.36";
            return value;
        });

        public override string UserAgent => mUserAgent.Value;

        public override bool IsConnected
        {
            get
            {
                if (DI.IsWindows10)
                {
                    var networkAccess = PlatformNetworkAccess;
                    return networkAccess == NetworkAccess.Internet;
                }
                else
                {
                    return base.IsConnected;
                }
            }
        }

        [SupportedOSPlatform("Windows10.0.10240.0")]
        static NetworkAccess PlatformNetworkAccess
        {
            get
            {
                // https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/Connectivity/Connectivity.uwp.cs#L19

                var profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile == null)
                    return NetworkAccess.Unknown;

                var level = profile.GetNetworkConnectivityLevel();
                switch (level)
                {
                    case NetworkConnectivityLevel.LocalAccess:
                        return NetworkAccess.Local;
                    case NetworkConnectivityLevel.InternetAccess:
                        return NetworkAccess.Internet;
                    case NetworkConnectivityLevel.ConstrainedInternetAccess:
                        return NetworkAccess.ConstrainedInternet;
                    default:
                        return NetworkAccess.None;
                }
            }
        }
    }
}