#if MACOS || MACCATALYST || IOS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    [Mobius(
"""
Mobius.UI.App.SetLightOrDarkThemeFollowingSystem
""")]
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

    [Mobius(
"""
Mobius.UI.App.SetLightOrDarkThemeFollowingSystem
""")]
    public void SetLightOrDarkThemeFollowingSystem(bool enable)
    {
    }
}
#endif