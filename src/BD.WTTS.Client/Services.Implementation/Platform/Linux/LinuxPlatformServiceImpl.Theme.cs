#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    public bool? IsLightOrDarkTheme => null;

    public void SetLightOrDarkThemeFollowingSystem(bool enable)
    {
        // GNOM KDE Xfce ?
    }
}
#endif