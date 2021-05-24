using System.ApplicationModel;
using Windows.ApplicationModel;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    static class DesktopBridgeHelper2
    {
        internal static bool Init()
        {
            if (DI.Platform != Platform.Windows) return false;
            var osVer = Environment.OSVersion.Version;
            if (osVer.Major != 10 || osVer.Build < 17763) return false;
            try
            {
                if (string.IsNullOrWhiteSpace(Package.Current.Id.Name)) return false;
            }
            catch
            {
                return false;
            }
            DesktopBridgeHelper.IsDesktopBridge = true;
            FileSystemDesktopBridge.InitFileSystem();
            return true;
        }
    }
}