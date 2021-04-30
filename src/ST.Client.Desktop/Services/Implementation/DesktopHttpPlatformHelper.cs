using System.Application.UI.Resx;
using System.Net.Http;

namespace System.Application.Services.Implementation
{
    internal sealed class DesktopHttpPlatformHelper : HttpPlatformHelper
    {
        public override string AcceptLanguage => R.AcceptLanguage;
    }
}