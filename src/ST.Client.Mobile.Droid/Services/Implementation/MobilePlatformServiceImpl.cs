using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.Services.Implementation
{
    internal sealed class MobilePlatformServiceImpl : IMobilePlatformService
    {
        public object CurrentPlatformUIHost => XEPlatform.CurrentActivity;

        void IPlatformService.OpenFileByTextReader(string filePath)
            => GoToPlatformPages.OpenFile(
                XEPlatform.CurrentActivity,
                new(filePath),
                MediaTypeNames.TXT);
    }
}