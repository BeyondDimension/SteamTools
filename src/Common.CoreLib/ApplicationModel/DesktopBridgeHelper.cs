using System.ApplicationModel;

namespace System.ApplicationModel
{
    public static class DesktopBridgeHelper
    {
        public static bool IsDesktopBridge { internal get; set; }
    }
}

namespace System
{
    partial class DI
    {
        public static bool IsDesktopBridge => DesktopBridgeHelper.IsDesktopBridge;
    }
}