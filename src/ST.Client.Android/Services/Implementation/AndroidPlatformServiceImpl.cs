using System;
using System.Collections.Generic;
using System.Text;
using XEPlatform = Xamarin.Essentials.Platform;
using AndroidApplication = Android.App.Application;
using AndroidX.AppCompat.App;

namespace System.Application.Services.Implementation
{
    internal sealed class AndroidPlatformServiceImpl : IPlatformService
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
           => GoToPlatformPages.OpenFile(
               XEPlatform.CurrentActivity,
               new(filePath),
               MediaTypeNames.TXT);
    }
}
