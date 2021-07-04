using Windows.ApplicationModel;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    public static class DesktopBridgeHelper
    {
        public static bool Init(int min_os_build = 17763)
        {
            if (DI.Platform != Platform.Windows) return false;
            var osVer = Environment.OSVersion.Version;
            if (osVer.Major != 10 || osVer.Build < min_os_build) return false;
            try
            {
                if (string.IsNullOrWhiteSpace(Package.Current.Id.Name)) return false;
            }
            catch
            {
                return false;
            }
            DI.IsDesktopBridge = true;
            FileSystemDesktopBridge.InitFileSystem();
            return true;
        }
    }
}