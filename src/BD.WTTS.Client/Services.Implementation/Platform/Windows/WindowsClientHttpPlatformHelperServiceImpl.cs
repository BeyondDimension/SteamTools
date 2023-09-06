#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

internal sealed class WindowsClientHttpPlatformHelperServiceImpl : ClientHttpPlatformHelperServiceImpl
{
    static readonly Lazy<string> mUserAgent = new(() =>
    {
        var v = Environment.OSVersion.Version;
        var value = $"Mozilla/5.0 (Windows NT {v.Major}.{v.Minor}{(Environment.Is64BitOperatingSystem ? "; Win64; x64" : null)}) AppleWebKit/" +
        AppleWebKitCompatVersion +
        " (KHTML, like Gecko) Chrome/" +
        ChromiumVersion +
        " Safari/" +
        AppleWebKitCompatVersion;
        return value;
    });

    protected override bool IsConnected => true;

    public override Task<bool> IsConnectedAsync() => Task.FromResult(true);

    public override string UserAgent => mUserAgent.Value;
}
#endif