namespace System.Application.Services.Implementation
{
    internal sealed class PlatformHttpPlatformHelper : DesktopHttpPlatformHelper
    {
        static readonly Lazy<string> mUserAgent = new(() =>
        {
            var value = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/" + AppleWebKitCompatVersion + " (KHTML, like Gecko) Chrome/" + ChromiumVersion + " Safari/" + AppleWebKitCompatVersion;
            return value;
        });

        public override string UserAgent => mUserAgent.Value;
    }
}