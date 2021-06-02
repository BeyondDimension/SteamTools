using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.Services.Implementation
{
    internal sealed class MobilePlatformServiceImpl : IMobilePlatformService
    {
        public object CurrentPlatformUIHost => XEPlatform.CurrentActivity;
    }
}