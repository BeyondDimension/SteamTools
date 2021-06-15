namespace System.Application.Services.Implementation
{
    internal sealed class PlatformHttpPlatformHelper : DesktopHttpPlatformHelper
    {
        static readonly Lazy<string> mUserAgent = new(() =>
        {
            var value = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.72 Safari/537.36";
            return value;
        });

        public override string UserAgent => mUserAgent.Value;
    }
}