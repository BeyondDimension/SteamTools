// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
#if DEBUG
    [Obsolete("use CurrentAppIsInstallVersion", true)]
    bool IsInstall => CurrentAppIsInstallVersion;
#endif

    /// <summary>
    /// 当前程序是否为安装版
    /// </summary>
    bool CurrentAppIsInstallVersion =>
#if IOS || ANDROID
        true;
#else
        false;
#endif
}