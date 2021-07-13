using Microsoft.Win32;
using System.Application.Models;
using System.Application.UI;
using System.Management;
using System.Security.Principal;

namespace System.Application.Services.Implementation
{
    partial class WindowsDesktopPlatformServiceImpl
    {
        const string Themes_Personalize = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        const string AppsUseLightTheme = "AppsUseLightTheme";

        public bool? IsLightOrDarkTheme
        {
            get
            {
                // 设置 - 个性化 - 颜色 - 选择你的默认 Windows 模式 SystemUsesLightTheme
                // 设置 - 个性化 - 颜色 - 选择默认应用模式 AppsUseLightTheme
                if (DI.IsWindows10OrLater)
                {
                    try
                    {
                        var value = Registry.CurrentUser.Read(Themes_Personalize, AppsUseLightTheme);
                        switch (value)
                        {
                            case "1":
                                return true;
                            case "0":
                                return false;
                        }
                    }
                    catch
                    {
                    }
                }
                return null;
            }
        }

        ManagementEventWatcher? isLightOrDarkThemeWatch;

        public void SetLightOrDarkThemeFollowingSystem(bool enable)
        {
            var major = Environment.OSVersion.Version.Major;
            if (major < 10 || (major == 10 && Environment.OSVersion.Version.Build < 18282)) return;

            var currentUser = WindowsIdentity.GetCurrent()?.User;
            if (currentUser == null) return;

            try
            {
                if (enable)
                {
                    if (isLightOrDarkThemeWatch == null)
                    {
                        var queryStr_ = @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'";

                        var queryStr = string.Format(
                            queryStr_,
                            currentUser.Value,
                            Themes_Personalize.Replace(@"\", @"\\"),
                            AppsUseLightTheme);

                        isLightOrDarkThemeWatch = new ManagementEventWatcher(queryStr);
                        isLightOrDarkThemeWatch.EventArrived += (_, _) =>
                        {
                            var value = IsLightOrDarkTheme;
                            if (value.HasValue)
                            {
                                var theme_value = value.Value ? AppTheme.Light : AppTheme.Dark;
                                MainThread2.BeginInvokeOnMainThread(() =>
                                {
                                    AppHelper.Current.SetThemeNotChangeValue(theme_value);
                                });
                            }
                        };
                    }
                    isLightOrDarkThemeWatch.Start();
                }
                else
                {
                    if (isLightOrDarkThemeWatch == null)
                    {
                        return;
                    }
                    else
                    {
                        isLightOrDarkThemeWatch.Stop();
                    }
                }
            }
            catch
            {
            }
        }
    }
}