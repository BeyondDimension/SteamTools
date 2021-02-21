using System.Properties;

namespace System.Application
{
    public static class BuildConfig
    {
        [Obsolete("use ThisAssembly.Debuggable", true)]
        public const bool DEBUG = ThisAssembly.Debuggable;

        public const string APPLICATION_ID = "com.github.aigiol.steamtools";

        [Obsolete("NotImplemented", true)]
        public const string BUILD_TYPE = "";

        [Obsolete("NotImplemented", true)]
        public const string FLAVOR = "";

        [Obsolete("NotImplemented", true)]
        public const int VERSION_CODE = default;

        [Obsolete("NotImplemented", true)]
        public const string VERSION_NAME = "";
    }
}