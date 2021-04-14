using System.Properties;
#if DEBUG
using System.Diagnostics;
using System.Security.Cryptography;
#endif

namespace System.Application
{
    public static class BuildConfig
    {
        [Obsolete("use ThisAssembly.Debuggable", true)]
        public const bool DEBUG = ThisAssembly.Debuggable;

        public const string APPLICATION_ID = "net.steampp.app";

        [Obsolete("NotImplemented", true)]
        public const string BUILD_TYPE = "";

        [Obsolete("NotImplemented", true)]
        public const string FLAVOR = "";

        [Obsolete("NotImplemented", true)]
        public const int VERSION_CODE = default;

        [Obsolete("NotImplemented", true)]
        public const string VERSION_NAME = "";

#if DEBUG
        static readonly Lazy<bool> mIsAigioPC = new(() =>
        {
            return Hashs.String.Crc32(Environment.MachineName, false) == "88DF9AB0" ||
            Hashs.String.Crc32(Environment.UserName, false) == "8AA383BC";
        });
        public static bool IsAigioPC => mIsAigioPC.Value;
        public static bool IsDebuggerAttached => Debugger.IsAttached;
#endif
    }
}