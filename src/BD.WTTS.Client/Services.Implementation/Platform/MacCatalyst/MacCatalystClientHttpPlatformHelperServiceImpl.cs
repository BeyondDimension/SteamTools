#if MACOS || MACCATALYST || IOS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class MacCatalystClientHttpPlatformHelperServiceImpl : ClientHttpPlatformHelperServiceImpl
{
    static readonly Lazy<string> mUserAgent = new(() =>
    {
        var v = Environment.OSVersion.Version;
#if IOS || MACCATALYST
        const string ios = "(iPhone; CPU iPhone OS {0}_{1}_{2} like Mac OS X)";
#endif
#if MACOS || MACCATALYST
        const string macos = "(Macintosh; Intel Mac OS X; {0}_{1}_{2})";
#endif
#if !MACCATALYST
        const
#endif
        string value =
        "Mozilla/5.0 " +
#if MACOS
        macos
#elif IOS
        ios
#else
        (OperatingSystem.IsMacOS() ? macos : ios)
#endif
        + " AppleWebKit/" +
        AppleWebKitCompatVersion +
        " (KHTML, like Gecko) Chrome/" +
        ChromiumVersion +
        " Safari/" +
        AppleWebKitCompatVersion;
        return string.Format(value, v.Major, v.Minor, v.Build);
    });

    public override string UserAgent => mUserAgent.Value;
}
#endif