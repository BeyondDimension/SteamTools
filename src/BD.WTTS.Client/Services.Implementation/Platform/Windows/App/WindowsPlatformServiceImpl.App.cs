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
        var value = reg32.Read(@"SOFTWARE\Steam++", "InstPath").TrimEnd(Path.DirectorySeparatorChar);
        return string.Equals(IOPath.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar), value, StringComparison.OrdinalIgnoreCase);
    });

    /// <inheritdoc cref="IPlatformService.CurrentAppIsInstallVersion"/>
    public static bool CurrentAppIsInstallVersion => _CurrentAppIsInstallVersion.Value;

    bool IPlatformService.CurrentAppIsInstallVersion => CurrentAppIsInstallVersion;
}
#endif