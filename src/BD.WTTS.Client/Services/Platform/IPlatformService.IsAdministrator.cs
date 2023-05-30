// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    internal static readonly Lazy<bool> _IsAdministrator = new(() =>
    {
#if WINDOWS
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        return identity.IsSystem || new WindowsPrincipal(identity)
            .IsInRole(WindowsBuiltInRole.Administrator);
#elif MACCATALYST || MACOS || LINUX || IOS || ANDROID
        return Interop.Libc.GetEUID() == 0;
#else
        throw new PlatformNotSupportedException();
#endif
    });

    /// <summary>
    /// 当前程序是否以 Administrator/System(Windows) 或 Root(FreeBSD/Linux/MacOS/Android/iOS) 权限运行
    /// </summary>
    bool IsAdministrator => _IsAdministrator.Value;
}