using System.Application.UI.Resx;
using System.Net.Http;

namespace System.Application.Services.Implementation
{
    internal sealed class DesktopHttpPlatformHelper : HttpPlatformHelper
    {
        readonly string mUserAgent;
        public DesktopHttpPlatformHelper()
        {
            mUserAgent = DI.Platform switch
            {
                Platform.Windows => $"Mozilla/5.0 (Windows NT {Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}{(Environment.Is64BitOperatingSystem ? "; WOW64" : null)}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.72 Safari/537.36",
                Platform.Linux => $"Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.72 Safari/537.36",
                Platform.Apple =>
                $"Mozilla/5.0 (Macintosh; Intel Mac OS X; {Environment.OSVersion.Version.Major}_{Environment.OSVersion.Version.Minor}_{Environment.OSVersion.Version.Build}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.72 Safari/537.36",
                _ => DefaultUserAgent,
            };
        }

        public override string AcceptLanguage => R.AcceptLanguage;

        public override string UserAgent => mUserAgent;
    }
}