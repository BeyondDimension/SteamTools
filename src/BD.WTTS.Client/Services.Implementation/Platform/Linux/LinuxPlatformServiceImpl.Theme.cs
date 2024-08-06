#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    [Mobius(
"""
Mobius.UI.App.SetLightOrDarkThemeFollowingSystem
""")]
    public bool? IsLightOrDarkTheme => null;

    [Mobius(
"""
Mobius.UI.App.SetLightOrDarkThemeFollowingSystem
""")]
    public void SetLightOrDarkThemeFollowingSystem(bool enable)
    {
        // GNOM KDE Xfce ?
    }
}
#endif