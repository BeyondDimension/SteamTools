#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class LinuxClientHttpPlatformHelperServiceImpl : ClientHttpPlatformHelperServiceImpl
{
    const string mUserAgent =
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/" +
        AppleWebKitCompatVersion +
        " (KHTML, like Gecko) Chrome/" +
        ChromiumVersion + " Safari/" +
        AppleWebKitCompatVersion;

    public override string UserAgent => mUserAgent;
}
#endif