using Microsoft.Win32;

namespace System.Application.Services.Implementation
{
    partial class WindowsDesktopPlatformServiceImpl
    {
        public bool? IsLightOrDarkTheme
        {
            get
            {
                // 设置 - 个性化 - 颜色 - 选择你的默认 Windows 模式 SystemUsesLightTheme
                // 设置 - 个性化 - 颜色 - 选择默认应用模式 AppsUseLightTheme
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    try
                    {
                        var value = Registry.CurrentUser.Read(
                            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                            "AppsUseLightTheme");
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
    }
}