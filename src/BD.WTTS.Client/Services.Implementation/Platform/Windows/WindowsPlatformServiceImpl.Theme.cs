#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    const string Themes_Personalize = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    const string AppsUseLightTheme = "AppsUseLightTheme";

    public bool? IsLightOrDarkTheme
    {
        get
        {
            // 设置 - 个性化 - 颜色 - 选择你的默认 Windows 模式 SystemUsesLightTheme
            // 设置 - 个性化 - 颜色 - 选择默认应用模式 AppsUseLightTheme
            if (OperatingSystem2.IsWindows10AtLeast())
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
                            var app = IApplication.Instance;
                            var theme_value = value.Value ? AppTheme.Light : AppTheme.Dark;
                            MainThread2.BeginInvokeOnMainThread(() =>
                            {
                                IApplication.Instance.SetThemeNotChangeValue(theme_value);
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
#endif