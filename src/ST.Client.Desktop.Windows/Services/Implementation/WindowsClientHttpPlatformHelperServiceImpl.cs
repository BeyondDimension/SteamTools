using System.Runtime.Versioning;
using Windows.Networking.Connectivity;
using Xamarin.Essentials;

namespace System.Application.Services.Implementation
{
    internal sealed class WindowsClientHttpPlatformHelperServiceImpl : ClientHttpPlatformHelperServiceImpl
    {
        static readonly Lazy<string> mUserAgent = new(() =>
        {
            var value = $"Mozilla/5.0 (Windows NT {Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}{(Environment.Is64BitOperatingSystem ? "; Win64; x64" : null)}) AppleWebKit/" + AppleWebKitCompatVersion + " (KHTML, like Gecko) Chrome/" + ChromiumVersion + " Safari/" + AppleWebKitCompatVersion;
            return value;
        });

        public override string UserAgent => mUserAgent.Value;

        //        protected override bool IsConnected
        //        {
        //            get
        //            {
        //                if (OperatingSystem2.IsWindows10AtLeast())
        //                {
        //#pragma warning disable CA1416 // 验证平台兼容性
        //                    var networkAccess = PlatformNetworkAccess;
        //#pragma warning restore CA1416 // 验证平台兼容性
        //                    return networkAccess == NetworkAccess.Internet;
        //                }
        //                else
        //                {
        //                    return base.IsConnected;
        //                }
        //            }
        //        }

        //[SupportedOSPlatform("Windows10.0.10240.0")]
        //static NetworkAccess PlatformNetworkAccess
        //{
        //    get
        //    {
        //        // https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/Connectivity/Connectivity.uwp.cs#L19

        //        // 必须在同一个线程中调用，否则其他线程将返回 null。
        //        var profile = NetworkInformation.GetInternetConnectionProfile();
        //        if (profile == null)
        //            return NetworkAccess.Unknown;

        //        var level = profile.GetNetworkConnectivityLevel();
        //        return level switch
        //        {
        //            NetworkConnectivityLevel.LocalAccess => NetworkAccess.Local,
        //            NetworkConnectivityLevel.InternetAccess => NetworkAccess.Internet,
        //            NetworkConnectivityLevel.ConstrainedInternetAccess => NetworkAccess.ConstrainedInternet,
        //            _ => NetworkAccess.None,
        //        };
        //    }
        //}
    }
}