using System.Runtime.Versioning;
using Windows.ApplicationModel;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    public sealed class DesktopBridgeHelper : DesktopBridge
    {
        private DesktopBridgeHelper() => throw new NotSupportedException();

        [SupportedOSPlatform("Windows10.0.10240.0")]
        public static bool Init(int min_os_build = 17763)
        {
            if (!OperatingSystem2.IsWindowsVersionAtLeast(10, 0, min_os_build)) return false;
            try
            {
                if (string.IsNullOrWhiteSpace(Package.Current.Id.Name)) return false;
            }
            catch
            {
                return false;
            }
            IsRunningOnUWP = true;
            FileSystemDesktopBridge.InitFileSystem();
            return true;
        }
    }
}