#if WINDOWS

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

sealed class DesktopBridgeHelper : DesktopBridge
{
    DesktopBridgeHelper() => throw new NotSupportedException();

    internal static bool Init(int min_os_build = 17763)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, min_os_build))
            return false;
        try
        {
            var pkgName = Package.Current.Id.Name;
            if (string.IsNullOrWhiteSpace(pkgName))
                return false;
        }
        catch
        {
            return false;
        }
        IsRunningAsUwp = true;
        return true;
    }
}
#endif