#if WINDOWS
using System.Security.Principal;
#endif

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 当前程序是否以 Administrator/System(Windows) 或 Root(FreeBSD/Linux/MacOS/Android/iOS) 权限运行
    /// </summary>
    bool IsAdministrator
    {
        get
        {
#if WINDOWS
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            return identity.IsSystem || new WindowsPrincipal(identity)
                .IsInRole(WindowsBuiltInRole.Administrator);
#elif MACCATALYST || MACOS || LINUX || IOS || ANDROID
            return GetEUID() == 0;
            throw new PlatformNotSupportedException();
#endif
        }
    }

#if MACCATALYST || MACOS || LINUX
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("libc", EntryPoint = "geteuid", SetLastError = true)]
    private static partial uint GetEUID();
#endif
}