using System.Collections.Generic;
using System.Text;
using System.Net;
using AndroidX.AppCompat.App;
using System.Application.UI.Activities;
using AndroidApplication = Android.App.Application;
#if NET6_0_OR_GREATER
using XEPlatform = Microsoft.Maui.ApplicationModel.Platform;
#else
using XEPlatform = Xamarin.Essentials.Platform;
#endif

namespace System.Application.Services.Implementation
{
    internal sealed partial class AndroidPlatformServiceImpl : IPlatformService
    {
        static bool IsDarkMode => DarkModeUtil.IsDarkMode(AndroidApplication.Context);

        public bool? IsLightOrDarkTheme => !IsDarkMode;

        public void SetLightOrDarkThemeFollowingSystem(bool enable)
        {
            if (enable)
            {
                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;
            }
            else
            {
                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightAutoBattery;
            }
        }

        void IPlatformService.OpenFileByTextReader(string filePath)
        {
            var activity = XEPlatform.CurrentActivity;
            if (filePath.StartsWith(IOPath.AppDataDirectory) || filePath.StartsWith(IOPath.CacheDirectory))
            {
                OpenFileByTextReaderByTextBlockActivity();
                return;
            }
            var result = GoToPlatformPages.OpenFile(
                activity,
                new(filePath),
                MediaTypeNames.TXT);
            if (!result)
            {
                OpenFileByTextReaderByTextBlockActivity();
            }

            void OpenFileByTextReaderByTextBlockActivity()
            {
                TextBlockActivity.StartActivity(activity, new() { FilePath = filePath });
            }
        }
    }
}
