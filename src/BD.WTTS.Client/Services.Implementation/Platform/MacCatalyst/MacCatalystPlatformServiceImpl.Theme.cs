#if MACOS || MACCATALYST || IOS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    public bool? IsLightOrDarkTheme
    {
        get
        {
            try
            {
                var value = NSUserDefaults.StandardUserDefaults.StringForKey("AppleInterfaceStyle");
                switch (value)
                {
                    case "Light":
                        return true;
                    case "Dark":
                        return false;
                }
            }
            catch
            {
            }
            return null;
        }
    }

    public void SetLightOrDarkThemeFollowingSystem(bool enable)
    {
    }
}
#endif