using System.Application.UI.Resx;
using System.Net.Http;

namespace System.Application.Services.Implementation
{
    public class DesktopHttpPlatformHelper : HttpPlatformHelper
    {
        protected const string ChromiumVersion = "90.0.4430.93";
        protected const string AppleWebKitCompatVersion = "537.36";

        public override string AcceptLanguage => R.AcceptLanguage;

        public override string UserAgent => DefaultUserAgent;
    }
}