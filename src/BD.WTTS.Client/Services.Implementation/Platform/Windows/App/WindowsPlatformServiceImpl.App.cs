#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    static readonly Lazy<bool> _CurrentAppIsInstallVersion = new(() =>
    {
        if (DesktopBridge.IsRunningAsUwp)
            return false;
        using var reg32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
#if WTTS_NATIVE_AOT
        var nativePath = reg32.Read(@"SOFTWARE\Steam++ AOT", "InstPath").TrimEnd(Path.DirectorySeparatorChar);
        if (string.Equals(IOPath.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar), nativePath, StringComparison.OrdinalIgnoreCase))
            return true;
#endif
        var value = reg32.Read(@"SOFTWARE\Steam++", "InstPath").TrimEnd(Path.DirectorySeparatorChar);
        return string.Equals(IOPath.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar), value, StringComparison.OrdinalIgnoreCase);
    });

    /// <inheritdoc cref="IPlatformService.CurrentAppIsInstallVersion"/>
    public static bool CurrentAppIsInstallVersion => _CurrentAppIsInstallVersion.Value;

    bool IPlatformService.CurrentAppIsInstallVersion => CurrentAppIsInstallVersion;
}
#endif