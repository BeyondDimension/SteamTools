#if IOS || MACOS || MACCATALYST

namespace BD.WTTS;

[Register("AppDelegate")]
public sealed class AppDelegate : UIApplicationDelegate
{
    public sealed override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        return true;
    }
}
#endif