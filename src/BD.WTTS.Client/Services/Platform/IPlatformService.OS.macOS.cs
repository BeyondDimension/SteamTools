// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
#if MACCATALYST || MACOS

    /// <summary>
    /// https://support.apple.com/zh-cn/guide/remote-desktop/apdd0c5a2d5/mac
    /// </summary>
    /// <returns></returns>
    string[] GetMacOSNetworkSetup() => Array.Empty<string>();

#endif
}