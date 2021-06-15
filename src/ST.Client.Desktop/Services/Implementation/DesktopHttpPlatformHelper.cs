using System.Application.UI.Resx;
using System.Net.Http;

namespace System.Application.Services.Implementation
{
    public class DesktopHttpPlatformHelper : HttpPlatformHelper
    {
        public override string AcceptLanguage => R.AcceptLanguage;

        public override string UserAgent => DefaultUserAgent;
    }
}