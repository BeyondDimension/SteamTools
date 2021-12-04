using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.Services.Implementation
{
    internal sealed class ApplePlatformServiceImpl : IPlatformService
    {
        public object CurrentPlatformUIHost => XEPlatform.GetCurrentUIViewController();

        public bool? IsLightOrDarkTheme => throw new NotImplementedException();

        public void SetLightOrDarkThemeFollowingSystem(bool enable)
        {
            throw new NotImplementedException();
        }
    }
}